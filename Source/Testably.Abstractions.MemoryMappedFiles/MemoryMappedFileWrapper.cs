using System.IO.MemoryMappedFiles;

namespace Testably.Abstractions;

internal sealed class MemoryMappedFileWrapper : IMemoryMappedFile
{
	private readonly FileSystemStream? _backingStream;
	private readonly MemoryMappedFile _instance;

	public MemoryMappedFileWrapper(IFileSystem fileSystem, MemoryMappedFile instance,
		FileSystemStream? backingStream)
	{
		_instance = instance;
		_backingStream = backingStream;
		FileSystem = fileSystem;
	}

	#region IMemoryMappedFile Members

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem { get; }

	/// <inheritdoc cref="IMemoryMappedFile.CreateViewAccessor()" />
	public IMemoryMappedViewAccessor CreateViewAccessor()
		=> new MemoryMappedViewAccessorWrapper(FileSystem,
			_instance.CreateViewAccessor());

	/// <inheritdoc cref="IMemoryMappedFile.CreateViewAccessor(long, long)" />
	public IMemoryMappedViewAccessor CreateViewAccessor(long offset, long size)
		=> new MemoryMappedViewAccessorWrapper(FileSystem,
			_instance.CreateViewAccessor(offset, size));

	/// <inheritdoc cref="IMemoryMappedFile.CreateViewAccessor(long, long, MemoryMappedFileAccess)" />
	public IMemoryMappedViewAccessor CreateViewAccessor(long offset, long size,
		MemoryMappedFileAccess access)
		=> new MemoryMappedViewAccessorWrapper(FileSystem,
			_instance.CreateViewAccessor(offset, size, access));

	/// <inheritdoc cref="IMemoryMappedFile.CreateViewStream()" />
	public MemoryMappedFileSystemViewStream CreateViewStream()
		=> new MemoryMappedViewStreamWrapper(_instance.CreateViewStream());

	/// <inheritdoc cref="IMemoryMappedFile.CreateViewStream(long, long)" />
	public MemoryMappedFileSystemViewStream CreateViewStream(long offset, long size)
		=> new MemoryMappedViewStreamWrapper(_instance.CreateViewStream(offset, size));

	/// <inheritdoc cref="IMemoryMappedFile.CreateViewStream(long, long, MemoryMappedFileAccess)" />
	public MemoryMappedFileSystemViewStream CreateViewStream(long offset, long size,
		MemoryMappedFileAccess access)
		=> new MemoryMappedViewStreamWrapper(
			_instance.CreateViewStream(offset, size, access));

	/// <inheritdoc cref="System.IDisposable.Dispose()" />
	public void Dispose()
	{
		_instance.Dispose();
		_backingStream?.Dispose();
	}

	#endregion
}
