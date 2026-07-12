using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using Testably.Abstractions.Internal;

namespace Testably.Abstractions;

/// <summary>
///     A memory-mapped file backed directly by the (in-memory) bytes of a
///     <see cref="FileSystemStream" /> of the <c>MockFileSystem</c>.
/// </summary>
internal sealed class MemoryMappedFileMock : IMemoryMappedFile
{
	private const string AccessToPathDeniedMessage = "Access to the path is denied.";

	private readonly MemoryMappedFileAccess _access;
	private readonly SharedBacking _backing;
	private readonly long _capacity;
	private bool _disposed;
	private readonly IDisposable _fileReference;
	private readonly MemoryMappedViewBacking _sharedViewBacking;

	public MemoryMappedFileMock(IFileSystem fileSystem, FileSystemStream stream,
		long capacity, MemoryMappedFileAccess access, bool ownsStream)
	{
		FileSystem = fileSystem;
		_access = access;

		if (capacity == 0)
		{
			if (stream.Length == 0)
			{
				#pragma warning disable MA0015 // Matches the parameter-less BCL message for an empty file.
				throw new ArgumentException(
					"A positive capacity must be specified for a Memory Mapped File backed by an empty file.");
				#pragma warning restore MA0015
			}

			capacity = stream.Length;
		}
		else if (capacity < stream.Length)
		{
			throw new ArgumentOutOfRangeException(nameof(capacity), capacity,
				"The capacity may not be smaller than the file size.");
		}

		// The mapping always reads the file, and the write-through accesses also require write
		// access to it; the real memory-mapped file fails in the same way when the file handle
		// was opened without the required access. A copy-on-write mapping never writes to the
		// file, so (like the real one) it only needs read access.
		bool requiresWritableStream = access is MemoryMappedFileAccess.ReadWrite
			or MemoryMappedFileAccess.ReadWriteExecute;
		if (!stream.CanRead || (requiresWritableStream && !stream.CanWrite))
		{
			throw new UnauthorizedAccessException(AccessToPathDeniedMessage);
		}

		if (capacity > stream.Length)
		{
			if (access is MemoryMappedFileAccess.Read)
			{
				#pragma warning disable MA0015 // Matches the parameter-less BCL message for this combination.
				throw new ArgumentException(
					"The capacity may not be larger than the file size when creating a read-only memory-mapped file.");
				#pragma warning restore MA0015
			}

			if (access is MemoryMappedFileAccess.ReadExecute)
			{
				// The BCL only special-cases `Read` above; growing the file for a `ReadExecute`
				// mapping fails later through the read-only file handle, surfacing on Windows as
				// this UnauthorizedAccessException.
				throw new UnauthorizedAccessException(AccessToPathDeniedMessage);
			}

			if (access is MemoryMappedFileAccess.CopyOnWrite)
			{
				// A copy-on-write mapping may never modify the underlying file, so it cannot be
				// grown to the requested capacity; matches the IOException of the BCL on Windows.
				throw new IOException(
					"Not enough memory resources are available to process this command.");
			}

			stream.SetLength(capacity);
		}

		_capacity = capacity;
		_backing = new SharedBacking(stream, ownsStream);
		_sharedViewBacking = new StreamViewBacking(stream);
		_fileReference = _backing.Acquire();
	}

	#region IMemoryMappedFile Members

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem { get; }

	/// <inheritdoc cref="IMemoryMappedFile.CreateViewAccessor()" />
	public IMemoryMappedViewAccessor CreateViewAccessor()
		=> CreateViewAccessor(0, 0, MemoryMappedFileAccess.ReadWrite);

	/// <inheritdoc cref="IMemoryMappedFile.CreateViewAccessor(long, long)" />
	public IMemoryMappedViewAccessor CreateViewAccessor(long offset, long size)
		=> CreateViewAccessor(offset, size, MemoryMappedFileAccess.ReadWrite);

	/// <inheritdoc cref="IMemoryMappedFile.CreateViewAccessor(long, long, MemoryMappedFileAccess)" />
	public IMemoryMappedViewAccessor CreateViewAccessor(long offset, long size,
		MemoryMappedFileAccess access)
	{
		long viewSize = ValidateAndNormalizeView(offset, size, access);
		ValidateViewAccess(access);
		MemoryMappedViewBacking backing =
			CreateViewBacking(access, out IDisposable backingOwner);
		return new MemoryMappedViewAccessorMock(FileSystem, backing, offset, viewSize,
			access, backingOwner);
	}

	/// <inheritdoc cref="IMemoryMappedFile.CreateViewStream()" />
	public MemoryMappedFileSystemViewStream CreateViewStream()
		=> CreateViewStream(0, 0, MemoryMappedFileAccess.ReadWrite);

	/// <inheritdoc cref="IMemoryMappedFile.CreateViewStream(long, long)" />
	public MemoryMappedFileSystemViewStream CreateViewStream(long offset, long size)
		=> CreateViewStream(offset, size, MemoryMappedFileAccess.ReadWrite);

	/// <inheritdoc cref="IMemoryMappedFile.CreateViewStream(long, long, MemoryMappedFileAccess)" />
	public MemoryMappedFileSystemViewStream CreateViewStream(long offset, long size,
		MemoryMappedFileAccess access)
	{
		long viewSize = ValidateAndNormalizeView(offset, size, access);
		ValidateViewAccess(access);
		MemoryMappedViewBacking backing =
			CreateViewBacking(access, out IDisposable backingOwner);
		return new MemoryMappedViewStreamMock(backing, offset, viewSize, access,
			backingOwner);
	}

	/// <inheritdoc cref="System.IDisposable.Dispose()" />
	public void Dispose()
	{
		// The memory-mapped file releases only its own reference to the shared backing; any
		// view that is still open keeps the backing alive, matching the independent lifetime
		// of the real memory-mapped file and its views.
		_disposed = true;
		_fileReference.Dispose();
	}

	#endregion

	/// <summary>
	///     Returns the <see cref="MemoryMappedViewBacking" /> for a view with the given
	///     <paramref name="access" />.
	/// </summary>
	/// <remarks>
	///     For <see cref="MemoryMappedFileAccess.CopyOnWrite" /> a page-privatizing backing over
	///     the shared stream is returned, so that reads keep observing the shared bytes until the
	///     view writes a page, and writes are neither persisted to the underlying file nor
	///     visible to other views (matching the real memory-mapped file).
	///     Every view holds a reference that keeps the shared underlying stream alive until the
	///     view is disposed.
	/// </remarks>
	private MemoryMappedViewBacking CreateViewBacking(MemoryMappedFileAccess access,
		out IDisposable backingOwner)
	{
		backingOwner = _backing.Acquire();
		if (access != MemoryMappedFileAccess.CopyOnWrite)
		{
			return _sharedViewBacking;
		}

		return new CopyOnWriteViewBacking(_sharedViewBacking, _capacity);
	}

	private long ValidateAndNormalizeView(long offset, long size,
		MemoryMappedFileAccess access)
	{
		MemoryMappedFileHelpers.ThrowIfNegative(offset, nameof(offset));
		MemoryMappedFileHelpers.ThrowIfNegative(size, nameof(size));
		access.ThrowIfOutOfRange(nameof(access));

		if (_disposed)
		{
			throw new ObjectDisposedException(null);
		}

		if (size > long.MaxValue - offset)
		{
			// A view whose `offset + size` overflows `long` can never be reserved; the real
			// memory-mapped file fails with this IOException from the operating system.
			throw new IOException("Not enough memory to map view.");
		}

		// `_capacity - offset` cannot overflow because `0 <= offset <= _capacity` holds whenever
		// this subtraction is evaluated.
		if (offset > _capacity || size > _capacity - offset)
		{
			throw new UnauthorizedAccessException(AccessToPathDeniedMessage);
		}

		return size == 0 ? _capacity - offset : size;
	}

	private void ValidateViewAccess(MemoryMappedFileAccess access)
	{
		// Read, ReadExecute and CopyOnWrite mappings never permit write-through views; a
		// copy-on-write mapping only allows Read or CopyOnWrite views, matching the BCL.
		bool mappingProhibitsWritableViews = _access is MemoryMappedFileAccess.Read
			or MemoryMappedFileAccess.ReadExecute
			or MemoryMappedFileAccess.CopyOnWrite;
		bool viewRequiresWrite = access is MemoryMappedFileAccess.Write
			or MemoryMappedFileAccess.ReadWrite
			or MemoryMappedFileAccess.ReadWriteExecute;
		if (mappingProhibitsWritableViews && viewRequiresWrite)
		{
			throw new UnauthorizedAccessException(AccessToPathDeniedMessage);
		}

		// Execute views require a mapping that was created with execute access; the real
		// memory-mapped file fails in the same way because the section lacks the execute
		// page protection.
		bool mappingSupportsExecute = _access is MemoryMappedFileAccess.ReadExecute
			or MemoryMappedFileAccess.ReadWriteExecute;
		bool viewRequiresExecute = access is MemoryMappedFileAccess.ReadExecute
			or MemoryMappedFileAccess.ReadWriteExecute;
		if (viewRequiresExecute && !mappingSupportsExecute)
		{
			throw new UnauthorizedAccessException(AccessToPathDeniedMessage);
		}
	}

	/// <summary>
	///     A reference-counted holder around the shared backing <see cref="Stream" />, so the
	///     memory-mapped file and its views can be disposed in any order: the stream is disposed
	///     only once the file and every view that acquired a reference have released it.
	/// </summary>
	private sealed class SharedBacking(Stream stream, bool ownsStream)
	{
		private int _refCount;

		public IDisposable Acquire()
		{
			Interlocked.Increment(ref _refCount);
			return new Reference(this);
		}

		private void Release()
		{
			if (Interlocked.Decrement(ref _refCount) == 0 && ownsStream)
			{
				stream.Dispose();
			}
		}

		private sealed class Reference(SharedBacking owner) : IDisposable
		{
			private int _disposed;

			#region IDisposable Members

			public void Dispose()
			{
				if (Interlocked.Exchange(ref _disposed, 1) == 0)
				{
					owner.Release();
				}
			}

			#endregion
		}
	}
}
