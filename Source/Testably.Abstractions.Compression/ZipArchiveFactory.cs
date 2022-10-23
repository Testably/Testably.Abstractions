using System.IO;
using System.IO.Compression;
using System.Text;

namespace Testably.Abstractions;

internal class ZipArchiveFactory : IZipArchiveFactory
{
	public ZipArchiveFactory(IFileSystem fileSystem)
	{
		FileSystem = fileSystem;
	}

	#region IZipArchiveFactory Members

	/// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
	public IFileSystem FileSystem { get; }

	/// <inheritdoc />
	public IZipArchive New(Stream stream)
		=> new ZipArchiveWrapper(FileSystem, new ZipArchive(stream));

	/// <inheritdoc />
	public IZipArchive New(Stream stream, ZipArchiveMode mode)
		=> new ZipArchiveWrapper(FileSystem, new ZipArchive(stream, mode));

	/// <inheritdoc />
	public IZipArchive New(Stream stream, ZipArchiveMode mode, bool leaveOpen)
		=> new ZipArchiveWrapper(FileSystem, new ZipArchive(stream, mode, leaveOpen));

	/// <inheritdoc />
	public IZipArchive New(Stream stream, ZipArchiveMode mode, bool leaveOpen,
	                       Encoding? entryNameEncoding)
		=> new ZipArchiveWrapper(FileSystem,
			new ZipArchive(stream, mode, leaveOpen, entryNameEncoding));

	#endregion
}