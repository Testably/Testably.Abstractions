using System.IO.Compression;
using System.Text;

namespace Testably.Abstractions;

public interface IZipFile
{
	/// <inheritdoc cref="ZipFile.CreateFromDirectory(string, string)" />
	void CreateFromDirectory(string sourceDirectoryName,
	                         string destinationArchiveFileName);

	/// <inheritdoc cref="ZipFile.CreateFromDirectory(string, string, CompressionLevel, bool)" />
	void CreateFromDirectory(string sourceDirectoryName,
	                         string destinationArchiveFileName,
	                         CompressionLevel compressionLevel,
	                         bool includeBaseDirectory);

	/// <inheritdoc cref="ZipFile.CreateFromDirectory(string, string, CompressionLevel, bool, Encoding)" />
	void CreateFromDirectory(string sourceDirectoryName,
	                         string destinationArchiveFileName,
	                         CompressionLevel compressionLevel,
	                         bool includeBaseDirectory,
	                         Encoding entryNameEncoding);

	/// <inheritdoc cref="ZipFile.ExtractToDirectory(string, string)" />
	void ExtractToDirectory(string sourceArchiveFileName,
	                        string destinationDirectoryName);

#if FEATURE_COMPRESSION_OVERWRITE
	/// <inheritdoc cref="ZipFile.ExtractToDirectory(string, string, bool)" />
	void ExtractToDirectory(string sourceArchiveFileName,
	                        string destinationDirectoryName,
	                        bool overwriteFiles);
#endif

	/// <inheritdoc cref="ZipFile.ExtractToDirectory(string, string, Encoding?)" />
	void ExtractToDirectory(string sourceArchiveFileName,
	                        string destinationDirectoryName,
	                        Encoding? entryNameEncoding);

#if FEATURE_COMPRESSION_OVERWRITE
	/// <inheritdoc cref="ZipFile.ExtractToDirectory(string, string, Encoding?, bool)" />
	void ExtractToDirectory(string sourceArchiveFileName,
	                        string destinationDirectoryName,
	                        Encoding? entryNameEncoding,
	                        bool overwriteFiles);
#endif

	/// <inheritdoc cref="ZipFile.Open(string, ZipArchiveMode)" />
	ZipArchive Open(string archiveFileName,
	                ZipArchiveMode mode);

	/// <inheritdoc cref="IZipFile.Open(string, ZipArchiveMode, Encoding?)" />
	ZipArchive Open(string archiveFileName,
	                ZipArchiveMode mode,
	                Encoding? entryNameEncoding);

	/// <inheritdoc cref="ZipFile.OpenRead(string)" />
	ZipArchive OpenRead(string archiveFileName);
}