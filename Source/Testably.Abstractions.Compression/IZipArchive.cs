using System;
using System.Collections.ObjectModel;
using System.IO.Compression;

namespace Testably.Abstractions;

/// <inheritdoc cref="ZipArchive" />
public interface IZipArchive : IFileSystemEntity, IDisposable
{
#if FEATURE_ZIPFILE_NET7
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

	/// <inheritdoc cref="System.IO.Compression.ZipFileExtensions.ExtractToDirectory(ZipArchive, string)" />
	void ExtractToDirectory(string destinationDirectoryName);

#if FEATURE_COMPRESSION_ADVANCED
	/// <inheritdoc cref="System.IO.Compression.ZipFileExtensions.ExtractToDirectory(ZipArchive, string, bool)" />
	void ExtractToDirectory(string destinationDirectoryName,
		bool overwriteFiles);
#endif

	/// <inheritdoc cref="ZipArchive.GetEntry(string)" />
	IZipArchiveEntry? GetEntry(string entryName);
}
