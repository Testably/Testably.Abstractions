using System;
using System.IO.Compression;
using Testably.Abstractions.Internal;

namespace Testably.Abstractions;

/// <inheritdoc cref="System.IO.Compression.ZipFileExtensions" />
public static class ZipFileExtensions
{
	/// <inheritdoc cref="System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(ZipArchive, string, string)" />
	public static IZipArchiveEntry CreateEntryFromFile(this IZipArchive destination,
	                                                   string sourceFileName,
	                                                   string entryName)
		=> ZipUtilities.CreateEntryFromFile(destination, sourceFileName, entryName, null);

	/// <inheritdoc
	///     cref="System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(ZipArchive, string, string, CompressionLevel)" />
	public static IZipArchiveEntry CreateEntryFromFile(this IZipArchive destination,
	                                                   string sourceFileName,
	                                                   string entryName,
	                                                   CompressionLevel compressionLevel)
		=> ZipUtilities.CreateEntryFromFile(destination,
			sourceFileName,
			entryName,
			compressionLevel);

	/// <inheritdoc cref="System.IO.Compression.ZipFileExtensions.ExtractToDirectory(ZipArchive, string)" />
	public static void ExtractToDirectory(this IZipArchive source,
	                                      string destinationDirectoryName)
	{
		if (source == null)
		{
			throw new ArgumentNullException(nameof(source));
		}

		if (destinationDirectoryName == null)
		{
			throw new ArgumentNullException(nameof(destinationDirectoryName));
		}

		foreach (IZipArchiveEntry entry in source.Entries)
		{
			entry.ExtractRelativeToDirectory(destinationDirectoryName, false);
		}
	}

#if FEATURE_COMPRESSION_ADVANCED
	/// <inheritdoc cref="System.IO.Compression.ZipFileExtensions.ExtractToDirectory(ZipArchive, string, bool)" />
	public static void ExtractToDirectory(this IZipArchive source,
	                                      string destinationDirectoryName,
	                                      bool overwriteFiles)
	{
		if (source == null)
		{
			throw new ArgumentNullException(nameof(source));
		}

		if (destinationDirectoryName == null)
		{
			throw new ArgumentNullException(nameof(destinationDirectoryName));
		}

		foreach (IZipArchiveEntry entry in source.Entries)
		{
			entry.ExtractRelativeToDirectory(destinationDirectoryName, overwriteFiles);
		}
	}
#endif

	/// <inheritdoc cref="System.IO.Compression.ZipFileExtensions.ExtractToFile(ZipArchiveEntry, string)" />
	public static void ExtractToFile(this IZipArchiveEntry source,
	                                 string destinationFileName) =>
		ExtractToFile(source, destinationFileName, false);

	/// <inheritdoc cref="System.IO.Compression.ZipFileExtensions.ExtractToFile(ZipArchiveEntry, string, bool)" />
	public static void ExtractToFile(this IZipArchiveEntry source,
	                                 string destinationFileName,
	                                 bool overwrite)
	{
		if (source == null)
		{
			throw new ArgumentNullException(nameof(source));
		}

		if (destinationFileName == null)
		{
			throw new ArgumentNullException(nameof(destinationFileName));
		}

		ZipUtilities.ExtractToFile(source, destinationFileName, overwrite);
	}
}