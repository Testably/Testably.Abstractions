using System.IO;
using System.IO.Compression;
using System.Text;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions;

internal sealed class ZipArchiveFactory : IZipArchiveFactory
{
	public ZipArchiveFactory(IFileSystem fileSystem)
	{
		FileSystem = fileSystem;
	}

	#region IZipArchiveFactory Members

	/// <inheritdoc cref="IFileSystemExtensionPoint.FileSystem" />
	public IFileSystem FileSystem { get; }

	/// <inheritdoc cref="IZipArchiveFactory.New(Stream)" />
	public IZipArchive New(Stream stream)
		=> new ZipArchiveWrapper(FileSystem, new ZipArchive(stream));

	/// <inheritdoc cref="IZipArchiveFactory.New(Stream, ZipArchiveMode)" />
	public IZipArchive New(Stream stream, ZipArchiveMode mode)
		=> new ZipArchiveWrapper(FileSystem, new ZipArchive(stream, mode));

	/// <inheritdoc cref="IZipArchiveFactory.New(Stream, ZipArchiveMode, bool)" />
	public IZipArchive New(Stream stream, ZipArchiveMode mode, bool leaveOpen)
		=> new ZipArchiveWrapper(FileSystem, new ZipArchive(stream, mode, leaveOpen));

	/// <inheritdoc cref="IZipArchiveFactory.New(Stream, ZipArchiveMode, bool, Encoding?)" />
	public IZipArchive New(Stream stream,
		ZipArchiveMode mode,
		bool leaveOpen,
		Encoding? entryNameEncoding)
		=> new ZipArchiveWrapper(FileSystem,
			new ZipArchive(stream, mode, leaveOpen, entryNameEncoding));

	#endregion
}