using System;
using System.IO;
using System.IO.Compression;

namespace Testably.Abstractions;

/// <inheritdoc cref="System.IO.Compression.ZipFileExtensions" />
public static class ZipFileExtensions
{
	/// <inheritdoc cref="System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(ZipArchive, string, string)" />
	public static IZipArchiveEntry CreateEntryFromFile(this IZipArchive destination,
	                                                   string sourceFileName,
	                                                   string entryName)
		=> DoCreateEntryFromFile(destination, sourceFileName, entryName, null);

	/// <inheritdoc
	///     cref="System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(ZipArchive, string, string, CompressionLevel)" />
	public static IZipArchiveEntry CreateEntryFromFile(this IZipArchive destination,
	                                                   string sourceFileName,
	                                                   string entryName,
	                                                   CompressionLevel compressionLevel)
		=> DoCreateEntryFromFile(destination, sourceFileName, entryName,
			compressionLevel);

	/// <inheritdoc cref="System.IO.Compression.ZipFileExtensions.ExtractToDirectory(ZipArchive, string)" />
	public static void ExtractToDirectory(this IZipArchive source,
	                                      string destinationDirectoryName) =>
		ExtractToDirectory(source, destinationDirectoryName, overwriteFiles: false);

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

	/// <inheritdoc cref="System.IO.Compression.ZipFileExtensions.ExtractToFile(ZipArchiveEntry, string)" />
	public static void ExtractToFile(this IZipArchiveEntry source,
	                                 string destinationFileName) =>
		ExtractToFile(source, destinationFileName, false);

	/// <inheritdoc cref="System.IO.Compression.ZipFileExtensions.ExtractToFile(ZipArchiveEntry, string, bool)" />
	public static void ExtractToFile(this IZipArchiveEntry source,
	                                 string destinationFileName, bool overwrite)
	{
		if (source == null)
		{
			throw new ArgumentNullException(nameof(source));
		}

		if (destinationFileName == null)
		{
			throw new ArgumentNullException(nameof(destinationFileName));
		}

		// Rely on FileStream's ctor for further checking destinationFileName parameter
		FileMode fMode = overwrite ? FileMode.Create : FileMode.CreateNew;

		using (FileSystemStream fs = source.FileSystem.FileStream.New(destinationFileName,
			fMode, FileAccess.Write, FileShare.None, bufferSize: 0x1000, useAsync: false))
		{
			using (Stream es = source.Open())
			{
				es.CopyTo(fs);
			}
		}

		try
		{
			File.SetLastWriteTime(destinationFileName, source.LastWriteTime.DateTime);
		}
		catch
		{
			// some OSes like Android (#35374) might not support setting the last write time, the extraction should not fail because of that
		}
	}

	internal static void ExtractRelativeToDirectory(this IZipArchiveEntry source,
	                                                string destinationDirectoryName,
	                                                bool overwrite)
	{
		if (source == null)
		{
			throw new ArgumentNullException(nameof(source));
		}

		if (destinationDirectoryName == null)
		{
			throw new ArgumentNullException(nameof(destinationDirectoryName));
		}

		// Note that this will give us a good DirectoryInfo even if destinationDirectoryName exists:
		IFileSystem.IDirectoryInfo di =
			source.FileSystem.Directory.CreateDirectory(destinationDirectoryName);
		string destinationDirectoryFullPath = di.FullName;
		if (!destinationDirectoryFullPath.EndsWith(source.FileSystem.Path
		   .DirectorySeparatorChar))
		{
			destinationDirectoryFullPath += source.FileSystem.Path.DirectorySeparatorChar;
		}

		string fileDestinationPath = source.FileSystem.Path.GetFullPath(
			source.FileSystem.Path.Combine(destinationDirectoryFullPath,
				source.FullName));

		//if (!fileDestinationPath.StartsWith(destinationDirectoryFullPath, PathInternal.StringComparison))
		//	throw new IOException(SR.IO_ExtractingResultsInOutside);

		if (source.FileSystem.Path.GetFileName(fileDestinationPath).Length == 0)
		{
			// If it is a directory:

			//if (source.Length != 0)
			//	throw new IOException(SR.IO_DirectoryNameWithData);

			source.FileSystem.Directory.CreateDirectory(fileDestinationPath);
		}
		else
		{
			// If it is a file:
			// Create containing directory:
			source.FileSystem.Directory.CreateDirectory(
				source.FileSystem.Path.GetDirectoryName(fileDestinationPath)!);
			source.ExtractToFile(fileDestinationPath, overwrite: overwrite);
		}
	}

	private static IZipArchiveEntry DoCreateEntryFromFile(IZipArchive destination,
	                                                      string sourceFileName,
	                                                      string entryName,
	                                                      CompressionLevel?
		                                                      compressionLevel)
	{
		if (destination == null)
		{
			throw new ArgumentNullException(nameof(destination));
		}

		if (sourceFileName == null)
		{
			throw new ArgumentNullException(nameof(sourceFileName));
		}

		if (entryName == null)
		{
			throw new ArgumentNullException(nameof(entryName));
		}

		// Checking of compressionLevel is passed down to DeflateStream and the IDeflater implementation
		// as it is a pluggable component that completely encapsulates the meaning of compressionLevel.

		// Argument checking gets passed down to FileStream's ctor and CreateEntry

		using (FileSystemStream fs = destination.FileSystem.FileStream.New(sourceFileName,
			FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 0x1000,
			useAsync: false))
		{
			IZipArchiveEntry entry = compressionLevel.HasValue
				? destination.CreateEntry(entryName, compressionLevel.Value)
				: destination.CreateEntry(entryName);

			DateTime lastWrite = File.GetLastWriteTime(sourceFileName);

			// If file to be archived has an invalid last modified time, use the first datetime representable in the Zip timestamp format
			// (midnight on January 1, 1980):
			if (lastWrite.Year < 1980 || lastWrite.Year > 2107)
			{
				lastWrite = new DateTime(1980, 1, 1, 0, 0, 0);
			}

			entry.LastWriteTime = lastWrite;

			using (Stream es = entry.Open())
			{
				fs.CopyTo(es);
			}

			return entry;
		}
	}
}