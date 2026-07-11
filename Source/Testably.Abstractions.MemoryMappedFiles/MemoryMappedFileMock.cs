using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace Testably.Abstractions;

/// <summary>
///     A memory-mapped file backed directly by the (in-memory) bytes of a
///     <see cref="FileSystemStream" /> of the <c>MockFileSystem</c>.
/// </summary>
internal sealed class MemoryMappedFileMock : IMemoryMappedFile
{
	private readonly MemoryMappedFileAccess _access;
	private readonly SharedBacking _backing;
	private readonly long _capacity;
	private readonly IDisposable _fileReference;
	private bool _disposed;

	public MemoryMappedFileMock(IFileSystem fileSystem, FileSystemStream stream,
		long capacity, MemoryMappedFileAccess access, bool ownsStream)
	{
		FileSystem = fileSystem;
		_access = access;
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(capacity), capacity,
				$"capacity ('{capacity}') must be a non-negative value.");
		}

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
		else if (capacity > stream.Length)
		{
			if (access is MemoryMappedFileAccess.Read
				or MemoryMappedFileAccess.ReadExecute)
			{
#pragma warning disable MA0015 // Matches the parameter-less BCL message for this combination.
				throw new ArgumentException(
					"The capacity may not be larger than the file size when creating a read-only memory-mapped file.");
#pragma warning restore MA0015
			}

			stream.SetLength(capacity);
		}

		_capacity = capacity;
		_backing = new SharedBacking(stream, ownsStream);
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
		long viewSize = ValidateAndNormalizeView(offset, size);
		ValidateViewAccess(access);
		Stream backing = CreateViewBacking(access, out IDisposable backingOwner);
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
		long viewSize = ValidateAndNormalizeView(offset, size);
		ValidateViewAccess(access);
		Stream backing = CreateViewBacking(access, out IDisposable backingOwner);
		return new MemoryMappedViewStreamMock(backing, offset, viewSize, access,
			backingOwner);
	}

	/// <inheritdoc cref="System.IDisposable.Dispose()" />
	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			// The memory-mapped file releases only its own reference to the shared backing; any
			// view that is still open keeps the backing alive, matching the independent lifetime
			// of the real memory-mapped file and its views.
			_fileReference.Dispose();
		}
	}

	#endregion

	/// <summary>
	///     Returns the backing <see cref="Stream" /> for a view with the given
	///     <paramref name="access" />.
	/// </summary>
	/// <remarks>
	///     For <see cref="MemoryMappedFileAccess.CopyOnWrite" /> a private in-memory copy of the
	///     mapped bytes is returned, so that writes are neither persisted to the underlying file
	///     nor visible to other views (matching the real memory-mapped file). The returned
	///     <paramref name="backingOwner" /> disposes that copy.
	///     For every other access the shared underlying stream is returned, and
	///     <paramref name="backingOwner" /> is a reference that keeps it alive until the view is
	///     disposed.
	/// </remarks>
	private Stream CreateViewBacking(MemoryMappedFileAccess access,
		out IDisposable backingOwner)
	{
		if (access != MemoryMappedFileAccess.CopyOnWrite)
		{
			backingOwner = _backing.Acquire();
			return _backing.Stream;
		}

		byte[] copy = new byte[_capacity];
		Stream stream = _backing.Stream;
		long position = stream.Position;
		stream.Position = 0;
		int read = 0;
		while (read < copy.Length)
		{
			int r = stream.Read(copy, read, copy.Length - read);
			if (r == 0)
			{
				break;
			}

			read += r;
		}

		stream.Position = position;
		MemoryStream copyStream = new(copy, 0, copy.Length, writable: true,
			publiclyVisible: false);
		backingOwner = copyStream;
		return copyStream;
	}

	private void ValidateViewAccess(MemoryMappedFileAccess access)
	{
		bool mappingIsReadOnly = _access is MemoryMappedFileAccess.Read
			or MemoryMappedFileAccess.ReadExecute;
		bool viewRequiresWrite = access is MemoryMappedFileAccess.Write
			or MemoryMappedFileAccess.ReadWrite
			or MemoryMappedFileAccess.ReadWriteExecute;
		if (mappingIsReadOnly && viewRequiresWrite)
		{
			throw new UnauthorizedAccessException("Access to the path is denied.");
		}
	}

	private long ValidateAndNormalizeView(long offset, long size)
	{
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(offset), offset,
				$"offset ('{offset}') must be a non-negative value.");
		}

		if (size < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(size), size,
				$"size ('{size}') must be a non-negative value.");
		}

		// `_capacity - offset` cannot overflow because `0 <= offset <= _capacity` holds whenever
		// this subtraction is evaluated, so comparing it against `size` correctly rejects even
		// absurdly large (near `long.MaxValue`) sizes that would overflow `offset + size`.
		if (offset > _capacity || size > _capacity - offset)
		{
			throw new UnauthorizedAccessException("Access to the path is denied.");
		}

		return size == 0 ? _capacity - offset : size;
	}

	/// <summary>
	///     A reference-counted holder around the shared backing <see cref="Stream" />, so the
	///     memory-mapped file and its views can be disposed in any order: the stream is disposed
	///     only once the file and every view that acquired a reference have released it.
	/// </summary>
	private sealed class SharedBacking
	{
		private readonly bool _ownsStream;
		private int _refCount;

		public SharedBacking(Stream stream, bool ownsStream)
		{
			Stream = stream;
			_ownsStream = ownsStream;
		}

		public Stream Stream { get; }

		public IDisposable Acquire()
		{
			Interlocked.Increment(ref _refCount);
			return new Reference(this);
		}

		private void Release()
		{
			if (Interlocked.Decrement(ref _refCount) == 0 && _ownsStream)
			{
				Stream.Dispose();
			}
		}

		private sealed class Reference : IDisposable
		{
			private readonly SharedBacking _owner;
			private int _disposed;

			public Reference(SharedBacking owner)
			{
				_owner = owner;
			}

			public void Dispose()
			{
				if (Interlocked.Exchange(ref _disposed, 1) == 0)
				{
					_owner.Release();
				}
			}
		}
	}
}
