using System.IO;
using System.IO.Compression;
using System.Text;

namespace Testably.Abstractions;

/// <inheritdoc cref="ZipArchive" />
public interface IZipArchiveFactory : IFileSystemExtensionPoint
{
	/// <inheritdoc cref="ZipArchive(Stream)" />
	IZipArchive New(Stream stream);

	/// <inheritdoc cref="ZipArchive(Stream, ZipArchiveMode)" />
	IZipArchive New(Stream stream, ZipArchiveMode mode);

	/// <inheritdoc cref="ZipArchive(Stream, ZipArchiveMode, bool)" />
	IZipArchive New(Stream stream, ZipArchiveMode mode, bool leaveOpen);

	/// <inheritdoc cref="ZipArchive(Stream, ZipArchiveMode, bool, Encoding?)" />
	IZipArchive New(Stream stream,
	                ZipArchiveMode mode,
	                bool leaveOpen,
	                Encoding? entryNameEncoding);
}