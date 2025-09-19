using System;
using System.Collections.ObjectModel;
using System.IO.Compression;
#if FEATURE_COMPRESSION_ASYNC
using System.Threading;
using System.Threading.Tasks;
#endif

namespace Testably.Abstractions;

/// <inheritdoc cref="ZipArchive" />
public interface IZipArchive : IFileSystemEntity, IDisposable
#if FEATURE_COMPRESSION_ASYNC
, IAsyncDisposable
#endif
{
#if FEATURE_FILESYSTEM_COMMENT_ENCRYPTED
	/// <inheritdoc cref="ZipArchiveEntry.Comment" />
	string Comment { get; set; }
#endif

	/// <inheritdoc cref="ZipArchive.Entries" />
	ReadOnlyCollection<IZipArchiveEntry> Entries { get; }

	/// <inheritdoc cref="ZipArchive.Mode" />
	ZipArchiveMode Mode { get; }

	/// <inheritdoc cref="ZipArchive.CreateEntry(string)" />
	IZipArchiveEntry CreateEntry(string entryName);

	/// <inheritdoc cref="ZipArchive.CreateEntry(string, CompressionLevel)" />
	IZipArchiveEntry CreateEntry(string entryName, CompressionLevel compressionLevel);

	/// <inheritdoc cref="System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(ZipArchive, string, string)" />
	IZipArchiveEntry CreateEntryFromFile(string sourceFileName,
		string entryName);

	/// <inheritdoc
	///     cref="System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(ZipArchive, string, string, CompressionLevel)" />
	IZipArchiveEntry CreateEntryFromFile(string sourceFileName,
		string entryName,
		CompressionLevel compressionLevel);
	
#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="System.IO.Compression.ZipFileExtensions.CreateEntryFromFileAsync(ZipArchive, string, string, CancellationToken)" />
	Task<IZipArchiveEntry> CreateEntryFromFileAsync(string sourceFileName,
		string entryName,
		CancellationToken cancellationToken = default);
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc
	///     cref="System.IO.Compression.ZipFileExtensions.CreateEntryFromFileAsync(ZipArchive, string, string, CompressionLevel, CancellationToken)" />
	Task<IZipArchiveEntry> CreateEntryFromFileAsync(string sourceFileName,
		string entryName,
		CompressionLevel compressionLevel,
		CancellationToken cancellationToken = default);
#endif

	/// <inheritdoc cref="System.IO.Compression.ZipFileExtensions.ExtractToDirectory(ZipArchive, string)" />
	void ExtractToDirectory(string destinationDirectoryName);

#if FEATURE_COMPRESSION_ADVANCED
	/// <inheritdoc cref="System.IO.Compression.ZipFileExtensions.ExtractToDirectory(ZipArchive, string, bool)" />
	void ExtractToDirectory(string destinationDirectoryName,
		bool overwriteFiles);
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="System.IO.Compression.ZipFileExtensions.ExtractToDirectoryAsync(ZipArchive, string, CancellationToken)" />
	Task ExtractToDirectoryAsync(string destinationDirectoryName,
		CancellationToken cancellationToken = default);
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="System.IO.Compression.ZipFileExtensions.ExtractToDirectoryAsync(ZipArchive, string, bool, CancellationToken)" />
	Task ExtractToDirectoryAsync(string destinationDirectoryName,
		bool overwriteFiles,
		CancellationToken cancellationToken = default);
#endif

	/// <inheritdoc cref="ZipArchive.GetEntry(string)" />
	IZipArchiveEntry? GetEntry(string entryName);
}
