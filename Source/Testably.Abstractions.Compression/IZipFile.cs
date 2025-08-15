using System.IO.Compression;
using System.Text;
#if FEATURE_COMPRESSION_STREAM
using System.IO;
#endif
#if FEATURE_COMPRESSION_ASYNC
using System.Threading;
using System.Threading.Tasks;
#endif

namespace Testably.Abstractions;

/// <inheritdoc cref="ZipFile" />
public interface IZipFile : IFileSystemEntity
{
#if FEATURE_COMPRESSION_STREAM
	/// <inheritdoc cref="ZipFile.CreateFromDirectory(string, Stream)" />
	void CreateFromDirectory(
		string sourceDirectoryName,
		Stream destination);
#endif

#if FEATURE_COMPRESSION_STREAM
	/// <inheritdoc cref="ZipFile.CreateFromDirectory(string, Stream, CompressionLevel, bool)" />
	void CreateFromDirectory(
		string sourceDirectoryName,
		Stream destination,
		CompressionLevel compressionLevel,
		bool includeBaseDirectory);
#endif

#if FEATURE_COMPRESSION_STREAM
	/// <inheritdoc cref="ZipFile.CreateFromDirectory(string, Stream, CompressionLevel, bool, Encoding)" />
	void CreateFromDirectory(
		string sourceDirectoryName,
		Stream destination,
		CompressionLevel compressionLevel,
		bool includeBaseDirectory,
		Encoding entryNameEncoding);
#endif

	/// <inheritdoc cref="ZipFile.CreateFromDirectory(string, string)" />
	void CreateFromDirectory(
		string sourceDirectoryName,
		string destinationArchiveFileName);

	/// <inheritdoc cref="ZipFile.CreateFromDirectory(string, string, CompressionLevel, bool)" />
	void CreateFromDirectory(
		string sourceDirectoryName,
		string destinationArchiveFileName,
		CompressionLevel compressionLevel,
		bool includeBaseDirectory);

	/// <inheritdoc cref="ZipFile.CreateFromDirectory(string, string, CompressionLevel, bool, Encoding)" />
	void CreateFromDirectory(
		string sourceDirectoryName,
		string destinationArchiveFileName,
		CompressionLevel compressionLevel,
		bool includeBaseDirectory,
		Encoding entryNameEncoding);

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="ZipFile.CreateFromDirectoryAsync(string, Stream, CancellationToken)" />
	Task CreateFromDirectoryAsync(
		string sourceDirectoryName,
		Stream destination,
		CancellationToken cancellationToken = default);
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="ZipFile.CreateFromDirectoryAsync(string, Stream, CompressionLevel, bool, CancellationToken)" />
	Task CreateFromDirectoryAsync(
		string sourceDirectoryName,
		Stream destination,
		CompressionLevel compressionLevel,
		bool includeBaseDirectory,
		CancellationToken cancellationToken = default);
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="ZipFile.CreateFromDirectoryAsync(string, Stream, CompressionLevel, bool, Encoding, CancellationToken)" />
	Task CreateFromDirectoryAsync(
		string sourceDirectoryName,
		Stream destination,
		CompressionLevel compressionLevel,
		bool includeBaseDirectory,
		Encoding entryNameEncoding,
		CancellationToken cancellationToken = default);
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="ZipFile.CreateFromDirectoryAsync(string, string, CancellationToken)" />
	Task CreateFromDirectoryAsync(
		string sourceDirectoryName,
		string destinationArchiveFileName,
		CancellationToken cancellationToken = default);
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="ZipFile.CreateFromDirectoryAsync(string, string, CompressionLevel, bool, CancellationToken)" />
	Task CreateFromDirectoryAsync(
		string sourceDirectoryName,
		string destinationArchiveFileName,
		CompressionLevel compressionLevel,
		bool includeBaseDirectory,
		CancellationToken cancellationToken = default);
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="ZipFile.CreateFromDirectoryAsync(string, string, CompressionLevel, bool, Encoding, CancellationToken)" />
	Task CreateFromDirectoryAsync(
		string sourceDirectoryName,
		string destinationArchiveFileName,
		CompressionLevel compressionLevel,
		bool includeBaseDirectory,
		Encoding entryNameEncoding,
		CancellationToken cancellationToken = default);
#endif

#if FEATURE_COMPRESSION_STREAM
	/// <inheritdoc cref="ZipFile.ExtractToDirectory(Stream, string)" />
	void ExtractToDirectory(
		Stream source,
		string destinationDirectoryName);
#endif

#if FEATURE_COMPRESSION_STREAM
	/// <inheritdoc cref="ZipFile.ExtractToDirectory(Stream, string, bool)" />
	void ExtractToDirectory(
		Stream source,
		string destinationDirectoryName,
		bool overwriteFiles);
#endif

#if FEATURE_COMPRESSION_STREAM
	/// <inheritdoc cref="ZipFile.ExtractToDirectory(Stream, string, Encoding)" />
	void ExtractToDirectory(
		Stream source,
		string destinationDirectoryName,
		Encoding entryNameEncoding);
#endif

#if FEATURE_COMPRESSION_STREAM
	/// <inheritdoc cref="ZipFile.ExtractToDirectory(Stream, string, Encoding, bool)" />
	void ExtractToDirectory(
		Stream source,
		string destinationDirectoryName,
		Encoding entryNameEncoding,
		bool overwriteFiles);
#endif

	/// <inheritdoc cref="ZipFile.ExtractToDirectory(string, string)" />
	void ExtractToDirectory(
		string sourceArchiveFileName,
		string destinationDirectoryName);

#if FEATURE_COMPRESSION_OVERWRITE
	/// <inheritdoc cref="ZipFile.ExtractToDirectory(string, string, bool)" />
	void ExtractToDirectory(
		string sourceArchiveFileName,
		string destinationDirectoryName,
		bool overwriteFiles);
#endif

	/// <inheritdoc cref="ZipFile.ExtractToDirectory(string, string, Encoding?)" />
	void ExtractToDirectory(
		string sourceArchiveFileName,
		string destinationDirectoryName,
		Encoding? entryNameEncoding);

#if FEATURE_COMPRESSION_OVERWRITE
	/// <inheritdoc cref="ZipFile.ExtractToDirectory(string, string, Encoding?, bool)" />
	void ExtractToDirectory(
		string sourceArchiveFileName,
		string destinationDirectoryName,
		Encoding? entryNameEncoding,
		bool overwriteFiles);
#endif
	

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="ZipFile.ExtractToDirectoryAsync(Stream, string, CancellationToken)" />
	Task ExtractToDirectoryAsync(
		Stream source,
		string destinationDirectoryName,
		CancellationToken cancellationToken = default);
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="ZipFile.ExtractToDirectoryAsync(Stream, string, bool, CancellationToken)" />
	Task ExtractToDirectoryAsync(
		Stream source,
		string destinationDirectoryName,
		bool overwriteFiles,
		CancellationToken cancellationToken = default);
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="ZipFile.ExtractToDirectoryAsync(Stream, string, Encoding, CancellationToken)" />
	Task ExtractToDirectoryAsync(
		Stream source,
		string destinationDirectoryName,
		Encoding entryNameEncoding,
		CancellationToken cancellationToken = default);
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="ZipFile.ExtractToDirectoryAsync(Stream, string, Encoding, bool, CancellationToken)" />
	Task ExtractToDirectoryAsync(
		Stream source,
		string destinationDirectoryName,
		Encoding entryNameEncoding,
		bool overwriteFiles,
		CancellationToken cancellationToken = default);
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="ZipFile.ExtractToDirectoryAsync(string, string, CancellationToken)" />
	Task ExtractToDirectoryAsync(
		string sourceArchiveFileName,
		string destinationDirectoryName,
		CancellationToken cancellationToken = default);
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="ZipFile.ExtractToDirectoryAsync(string, string, bool, CancellationToken)" />
	Task ExtractToDirectoryAsync(
		string sourceArchiveFileName,
		string destinationDirectoryName,
		bool overwriteFiles,
		CancellationToken cancellationToken = default);
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="ZipFile.ExtractToDirectoryAsync(string, string, Encoding?, CancellationToken)" />
	Task ExtractToDirectoryAsync(
		string sourceArchiveFileName,
		string destinationDirectoryName,
		Encoding? entryNameEncoding,
		CancellationToken cancellationToken = default);
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="ZipFile.ExtractToDirectoryAsync(string, string, Encoding?, bool, CancellationToken)" />
	Task ExtractToDirectoryAsync(
		string sourceArchiveFileName,
		string destinationDirectoryName,
		Encoding? entryNameEncoding,
		bool overwriteFiles,
		CancellationToken cancellationToken = default);
#endif

	/// <inheritdoc cref="ZipFile.Open(string, ZipArchiveMode)" />
	IZipArchive Open(
		string archiveFileName,
		ZipArchiveMode mode);

	/// <inheritdoc cref="IZipFile.Open(string, ZipArchiveMode, Encoding?)" />
	IZipArchive Open(
		string archiveFileName,
		ZipArchiveMode mode,
		Encoding? entryNameEncoding);

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="ZipFile.OpenAsync(string, ZipArchiveMode, CancellationToken)" />
	Task<IZipArchive> OpenAsync(
		string archiveFileName,
		ZipArchiveMode mode,
		CancellationToken cancellationToken = default);
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="IZipFile.OpenAsync(string, ZipArchiveMode, Encoding?, CancellationToken)" />
	Task<IZipArchive> OpenAsync(
		string archiveFileName,
		ZipArchiveMode mode,
		Encoding? entryNameEncoding,
		CancellationToken cancellationToken = default);
#endif

	/// <inheritdoc cref="ZipFile.OpenRead(string)" />
	IZipArchive OpenRead(
		string archiveFileName);

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="ZipFile.OpenReadAsync(string, CancellationToken)" />
	Task<IZipArchive> OpenReadAsync(
		string archiveFileName,
		CancellationToken cancellationToken = default);
#endif
}
