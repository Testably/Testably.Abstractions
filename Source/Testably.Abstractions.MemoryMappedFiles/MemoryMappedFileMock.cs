using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Testably.Abstractions;

/// <summary>
///     A memory-mapped file backed directly by the (in-memory) bytes of a
///     <see cref="FileSystemStream" /> of the <c>MockFileSystem</c>.
/// </summary>
internal sealed class MemoryMappedFileMock : IMemoryMappedFile
{
	private readonly MemoryMappedFileAccess _access;
	private readonly long _capacity;
	private readonly bool _ownsStream;
	private readonly FileSystemStream _stream;

	public MemoryMappedFileMock(IFileSystem fileSystem, FileSystemStream stream,
		long capacity, MemoryMappedFileAccess access, bool ownsStream)
	{
		FileSystem = fileSystem;
		_stream = stream;
		_access = access;
		_ownsStream = ownsStream;
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
	}

	#region IMemoryMappedFile Members

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem { get; }

	/// <inheritdoc cref="IMemoryMappedFile.CreateViewAccessor()" />
	public IMemoryMappedViewAccessor CreateViewAccessor()
		=> CreateViewAccessor(0, 0, _access);

	/// <inheritdoc cref="IMemoryMappedFile.CreateViewAccessor(long, long)" />
	public IMemoryMappedViewAccessor CreateViewAccessor(long offset, long size)
		=> CreateViewAccessor(offset, size, _access);

	/// <inheritdoc cref="IMemoryMappedFile.CreateViewAccessor(long, long, MemoryMappedFileAccess)" />
	public IMemoryMappedViewAccessor CreateViewAccessor(long offset, long size,
		MemoryMappedFileAccess access)
	{
		long viewSize = ValidateAndNormalizeView(offset, size);
		Stream backing = CreateViewBacking(access, out bool ownsBacking);
		return new MemoryMappedViewAccessorMock(FileSystem, backing, offset, viewSize,
			access, ownsBacking);
	}

	/// <inheritdoc cref="IMemoryMappedFile.CreateViewStream()" />
	public MemoryMappedFileSystemViewStream CreateViewStream()
		=> CreateViewStream(0, 0, _access);

	/// <inheritdoc cref="IMemoryMappedFile.CreateViewStream(long, long)" />
	public MemoryMappedFileSystemViewStream CreateViewStream(long offset, long size)
		=> CreateViewStream(offset, size, _access);

	/// <inheritdoc cref="IMemoryMappedFile.CreateViewStream(long, long, MemoryMappedFileAccess)" />
	public MemoryMappedFileSystemViewStream CreateViewStream(long offset, long size,
		MemoryMappedFileAccess access)
	{
		long viewSize = ValidateAndNormalizeView(offset, size);
		Stream backing = CreateViewBacking(access, out bool ownsBacking);
		return new MemoryMappedViewStreamMock(backing, offset, viewSize, access,
			ownsBacking);
	}

	/// <inheritdoc cref="System.IDisposable.Dispose()" />
	public void Dispose()
	{
		if (_ownsStream)
		{
			_stream.Dispose();
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
	///     nor visible to other views (matching the real memory-mapped file). The caller owns and
	///     must dispose that copy (<paramref name="ownsBacking" /> is <see langword="true" />).
	///     For every other access the shared underlying stream is returned unchanged.
	/// </remarks>
	private Stream CreateViewBacking(MemoryMappedFileAccess access, out bool ownsBacking)
	{
		if (access != MemoryMappedFileAccess.CopyOnWrite)
		{
			ownsBacking = false;
			return _stream;
		}

		byte[] copy = new byte[_capacity];
		long position = _stream.Position;
		_stream.Position = 0;
		int read = 0;
		while (read < copy.Length)
		{
			int r = _stream.Read(copy, read, copy.Length - read);
			if (r == 0)
			{
				break;
			}

			read += r;
		}

		_stream.Position = position;
		ownsBacking = true;
		return new MemoryStream(copy, 0, copy.Length, writable: true,
			publiclyVisible: false);
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

		if (offset > _capacity || (size > 0 && offset + size > _capacity))
		{
			throw new UnauthorizedAccessException("Access to the path is denied.");
		}

		return size == 0 ? _capacity - offset : size;
	}
}
