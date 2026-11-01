using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
#if FEATURE_COMPRESSION_ASYNC
using System.Threading.Tasks;
#endif

namespace Testably.Abstractions;

/// <inheritdoc cref="ZipArchiveEntry" />
public interface IZipArchiveEntry : IFileSystemEntity
{
	/// <inheritdoc cref="ZipArchiveEntry.Archive" />
	IZipArchive Archive { get; }

#if FEATURE_FILESYSTEM_COMMENT_ENCRYPTED
	/// <inheritdoc cref="ZipArchiveEntry.Comment" />
	string Comment { get; set; }
#endif

	/// <inheritdoc cref="ZipArchiveEntry.CompressedLength" />
	long CompressedLength { get; }

#if FEATURE_COMPRESSION_ENTRY_ACCESS
	/// <inheritdoc cref="ZipArchiveEntry.CompressionMethod" />
	ZipCompressionMethod CompressionMethod { get; }
#endif

#if FEATURE_COMPRESSION_ADVANCED
	/// <inheritdoc cref="ZipArchiveEntry.Crc32" />
	uint Crc32 { get; }
#endif

#if FEATURE_COMPRESSION_ADVANCED
	/// <inheritdoc cref="ZipArchiveEntry.ExternalAttributes" />
	int ExternalAttributes { get; set; }
#endif

#if FEATURE_COMPRESSION_ENCRYPTION
	/// <inheritdoc cref="ZipArchiveEntry.EncryptionMethod" />
	ZipEncryptionMethod EncryptionMethod { get; }
#endif

	/// <inheritdoc cref="ZipArchiveEntry.FullName" />
	string FullName { get; }

#if FEATURE_FILESYSTEM_COMMENT_ENCRYPTED
	/// <inheritdoc cref="ZipArchiveEntry.IsEncrypted" />
	bool IsEncrypted { get; }
#endif

	/// <inheritdoc cref="ZipArchiveEntry.LastWriteTime" />
	DateTimeOffset LastWriteTime { get; set; }

	/// <inheritdoc cref="ZipArchiveEntry.Length" />
	long Length { get; }

	/// <inheritdoc cref="ZipArchiveEntry.Name" />
	string Name { get; }

#if FEATURE_COMPRESSION_ENTRY_ACCESS
	/// <inheritdoc cref="ZipArchiveEntry.VersionMadeBy" />
	ushort VersionMadeBy { get; }
#endif

	/// <inheritdoc cref="ZipArchiveEntry.Delete()" />
	void Delete();

	/// <inheritdoc cref="System.IO.Compression.ZipFileExtensions.ExtractToFile(ZipArchiveEntry, string)" />
	void ExtractToFile(string destinationFileName);

	/// <inheritdoc cref="System.IO.Compression.ZipFileExtensions.ExtractToFile(ZipArchiveEntry, string, bool)" />
	void ExtractToFile(string destinationFileName,
		bool overwrite);

#if FEATURE_COMPRESSION_ENCRYPTION
	/// <inheritdoc cref="System.IO.Compression.ZipFileExtensions.ExtractToFile(ZipArchiveEntry, string, ZipExtractionOptions)" />
	void ExtractToFile(string destinationFileName, ZipExtractionOptions options);
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="System.IO.Compression.ZipFileExtensions.ExtractToFileAsync(ZipArchiveEntry, string, CancellationToken)" />
	Task ExtractToFileAsync(string destinationFileName, CancellationToken cancellationToken = default);
#endif
	
#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="System.IO.Compression.ZipFileExtensions.ExtractToFileAsync(ZipArchiveEntry, string, bool, CancellationToken)" />
	Task ExtractToFileAsync(string destinationFileName, bool overwrite, CancellationToken cancellationToken = default);
#endif

#if FEATURE_COMPRESSION_ENCRYPTION
	/// <inheritdoc cref="System.IO.Compression.ZipFileExtensions.ExtractToFileAsync(ZipArchiveEntry, string, ZipExtractionOptions, CancellationToken)" />
	Task ExtractToFileAsync(string destinationFileName, ZipExtractionOptions options,
		CancellationToken cancellationToken = default);
#endif

	/// <inheritdoc cref="ZipArchiveEntry.Open()" />
	Stream Open();

#if FEATURE_COMPRESSION_ENTRY_ACCESS
	/// <inheritdoc cref="ZipArchiveEntry.Open(FileAccess)" />
	Stream Open(FileAccess access);
#endif

#if FEATURE_COMPRESSION_ENCRYPTION
	/// <inheritdoc cref="ZipArchiveEntry.Open(ReadOnlySpan{char})" />
	Stream Open(ReadOnlySpan<char> password);
#endif

#if FEATURE_COMPRESSION_ENCRYPTION
	/// <inheritdoc cref="ZipArchiveEntry.Open(FileAccess, ReadOnlySpan{char})" />
	Stream Open(FileAccess access, ReadOnlySpan<char> password);
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="ZipArchiveEntry.OpenAsync(CancellationToken)" />
	Task<Stream> OpenAsync(CancellationToken cancellationToken = default);
#endif

#if FEATURE_COMPRESSION_ENTRY_ACCESS
	/// <inheritdoc cref="ZipArchiveEntry.OpenAsync(FileAccess, CancellationToken)" />
	Task<Stream> OpenAsync(FileAccess access, CancellationToken cancellationToken = default);
#endif

#if FEATURE_COMPRESSION_ENCRYPTION
	/// <inheritdoc cref="ZipArchiveEntry.OpenAsync(ReadOnlySpan{char}, CancellationToken)" />
	Task<Stream> OpenAsync(ReadOnlySpan<char> password,
		CancellationToken cancellationToken = default);
#endif

#if FEATURE_COMPRESSION_ENCRYPTION
	/// <inheritdoc cref="ZipArchiveEntry.OpenAsync(FileAccess, ReadOnlySpan{char}, CancellationToken)" />
	Task<Stream> OpenAsync(FileAccess access, ReadOnlySpan<char> password,
		CancellationToken cancellationToken = default);
#endif
}
