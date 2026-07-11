using System.IO.MemoryMappedFiles;

namespace Testably.Abstractions;

internal sealed class MemoryMappedFileWrapper(
	IFileSystem fileSystem,
	MemoryMappedFile instance,
	FileSystemStream? backingStream)
	: IMemoryMappedFile
{
	#region IMemoryMappedFile Members

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem { get; } = fileSystem;

	/// <inheritdoc cref="IMemoryMappedFile.CreateViewAccessor()" />
	public IMemoryMappedViewAccessor CreateViewAccessor()
		=> new MemoryMappedViewAccessorWrapper(FileSystem,
			instance.CreateViewAccessor());

	/// <inheritdoc cref="IMemoryMappedFile.CreateViewAccessor(long, long)" />
	public IMemoryMappedViewAccessor CreateViewAccessor(long offset, long size)
		=> new MemoryMappedViewAccessorWrapper(FileSystem,
			instance.CreateViewAccessor(offset, size));

	/// <inheritdoc cref="IMemoryMappedFile.CreateViewAccessor(long, long, MemoryMappedFileAccess)" />
	public IMemoryMappedViewAccessor CreateViewAccessor(long offset, long size,
		MemoryMappedFileAccess access)
		=> new MemoryMappedViewAccessorWrapper(FileSystem,
			instance.CreateViewAccessor(offset, size, access));

	/// <inheritdoc cref="IMemoryMappedFile.CreateViewStream()" />
	public MemoryMappedFileSystemViewStream CreateViewStream()
		=> new MemoryMappedViewStreamWrapper(instance.CreateViewStream());

	/// <inheritdoc cref="IMemoryMappedFile.CreateViewStream(long, long)" />
	public MemoryMappedFileSystemViewStream CreateViewStream(long offset, long size)
		=> new MemoryMappedViewStreamWrapper(instance.CreateViewStream(offset, size));

	/// <inheritdoc cref="IMemoryMappedFile.CreateViewStream(long, long, MemoryMappedFileAccess)" />
	public MemoryMappedFileSystemViewStream CreateViewStream(long offset, long size,
		MemoryMappedFileAccess access)
		=> new MemoryMappedViewStreamWrapper(
			instance.CreateViewStream(offset, size, access));

	/// <inheritdoc cref="System.IDisposable.Dispose()" />
	public void Dispose()
	{
		instance.Dispose();
		backingStream?.Dispose();
	}

	#endregion
}
