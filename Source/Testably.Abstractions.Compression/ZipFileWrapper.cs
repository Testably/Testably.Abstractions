using System.IO.Compression;
using System.Text;
using Testably.Abstractions.Internal;

namespace Testably.Abstractions;

internal sealed class ZipFileWrapper : IZipFile
{
	public ZipFileWrapper(IFileSystem fileSystem)
	{
		FileSystem = fileSystem;
	}

	#region IZipFile Members

	/// <inheritdoc cref="IFileSystemExtensionPoint.FileSystem" />
	public IFileSystem FileSystem { get; }

	/// <inheritdoc cref="IZipFile.CreateFromDirectory(string, string)" />
	public void CreateFromDirectory(string sourceDirectoryName,
	                                string destinationArchiveFileName)
		=> Execute.WhenRealFileSystem(FileSystem,
			() => ZipFile.CreateFromDirectory(
				sourceDirectoryName,
				destinationArchiveFileName),
			() => ZipUtilities.CreateFromDirectory(
				FileSystem,
				sourceDirectoryName,
				destinationArchiveFileName));

	/// <inheritdoc cref="IZipFile.CreateFromDirectory(string, string, CompressionLevel, bool)" />
	public void CreateFromDirectory(string sourceDirectoryName,
	                                string destinationArchiveFileName,
	                                CompressionLevel compressionLevel,
	                                bool includeBaseDirectory)
		=> Execute.WhenRealFileSystem(FileSystem,
			() => ZipFile.CreateFromDirectory(
				sourceDirectoryName,
				destinationArchiveFileName,
				compressionLevel,
				includeBaseDirectory),
			() => ZipUtilities.CreateFromDirectory(
				FileSystem,
				sourceDirectoryName,
				destinationArchiveFileName,
				compressionLevel,
				includeBaseDirectory));

	/// <inheritdoc cref="IZipFile.CreateFromDirectory(string, string, CompressionLevel, bool, Encoding)" />
	public void CreateFromDirectory(string sourceDirectoryName,
	                                string destinationArchiveFileName,
	                                CompressionLevel compressionLevel,
	                                bool includeBaseDirectory,
	                                Encoding entryNameEncoding)
		=> Execute.WhenRealFileSystem(FileSystem,
			() => ZipFile.CreateFromDirectory(
				sourceDirectoryName,
				destinationArchiveFileName,
				compressionLevel,
				includeBaseDirectory,
				entryNameEncoding),
			() => ZipUtilities.CreateFromDirectory(
				FileSystem,
				sourceDirectoryName,
				destinationArchiveFileName,
				compressionLevel,
				includeBaseDirectory,
				entryNameEncoding));

	/// <inheritdoc cref="IZipFile.ExtractToDirectory(string, string)" />
	public void ExtractToDirectory(string sourceArchiveFileName,
	                               string destinationDirectoryName)
		=> Execute.WhenRealFileSystem(FileSystem,
			() => ZipFile.ExtractToDirectory(
				sourceArchiveFileName,
				destinationDirectoryName),
			() => ZipUtilities.ExtractToDirectory(
				FileSystem,
				sourceArchiveFileName,
				destinationDirectoryName));

#if FEATURE_COMPRESSION_OVERWRITE
	/// <inheritdoc cref="IZipFile.ExtractToDirectory(string, string, bool)" />
	public void ExtractToDirectory(string sourceArchiveFileName,
	                               string destinationDirectoryName,
	                               bool overwriteFiles)
		=> Execute.WhenRealFileSystem(FileSystem,
			() => ZipFile.ExtractToDirectory(
				sourceArchiveFileName,
				destinationDirectoryName,
				overwriteFiles),
			() => ZipUtilities.ExtractToDirectory(
				FileSystem,
				sourceArchiveFileName,
				destinationDirectoryName,
				overwriteFiles: overwriteFiles));
#endif

	/// <inheritdoc cref="IZipFile.ExtractToDirectory(string, string, Encoding?)" />
	public void ExtractToDirectory(string sourceArchiveFileName,
	                               string destinationDirectoryName,
	                               Encoding? entryNameEncoding)
		=> Execute.WhenRealFileSystem(FileSystem,
			() => ZipFile.ExtractToDirectory(
				sourceArchiveFileName,
				destinationDirectoryName,
				entryNameEncoding),
			() => ZipUtilities.ExtractToDirectory(
				FileSystem,
				sourceArchiveFileName,
				destinationDirectoryName,
				entryNameEncoding: entryNameEncoding));

#if FEATURE_COMPRESSION_OVERWRITE
	/// <inheritdoc cref="IZipFile.ExtractToDirectory(string, string, Encoding?, bool)" />
	public void ExtractToDirectory(string sourceArchiveFileName,
	                               string destinationDirectoryName,
	                               Encoding? entryNameEncoding,
	                               bool overwriteFiles)
		=> Execute.WhenRealFileSystem(FileSystem,
			() => ZipFile.ExtractToDirectory(
				sourceArchiveFileName,
				destinationDirectoryName,
				entryNameEncoding,
				overwriteFiles),
			() => ZipUtilities.ExtractToDirectory(
				FileSystem,
				sourceArchiveFileName,
				destinationDirectoryName,
				entryNameEncoding,
				overwriteFiles));
#endif

	/// <inheritdoc cref="IZipFile.Open(string, ZipArchiveMode)" />
	public IZipArchive Open(string archiveFileName, ZipArchiveMode mode)
		=> new ZipArchiveWrapper(FileSystem,
			Execute.WhenRealFileSystem(FileSystem,
				() => ZipFile.Open(archiveFileName, mode),
				() => ZipUtilities.Open(FileSystem,
					archiveFileName,
					mode)));

	/// <inheritdoc cref="IZipFile.Open(string, ZipArchiveMode, Encoding?)" />
	public IZipArchive Open(string archiveFileName,
	                        ZipArchiveMode mode,
	                        Encoding? entryNameEncoding)
		=> new ZipArchiveWrapper(FileSystem,
			Execute.WhenRealFileSystem(FileSystem,
				() => ZipFile.Open(archiveFileName, mode, entryNameEncoding),
				() => ZipUtilities.Open(FileSystem,
					archiveFileName,
					mode,
					entryNameEncoding)));

	/// <inheritdoc cref="IZipFile.OpenRead(string)" />
	public IZipArchive OpenRead(string archiveFileName)
		=> new ZipArchiveWrapper(FileSystem,
			Execute.WhenRealFileSystem(FileSystem,
				() => ZipFile.OpenRead(archiveFileName),
				() => ZipUtilities.Open(FileSystem,
					archiveFileName,
					ZipArchiveMode.Read)));

	#endregion
}