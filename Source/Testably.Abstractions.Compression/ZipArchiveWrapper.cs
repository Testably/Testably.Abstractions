using System;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Linq;
using Testably.Abstractions.FileSystem;
using Testably.Abstractions.Internal;

namespace Testably.Abstractions;

internal sealed class ZipArchiveWrapper : IZipArchive
{
	private readonly ZipArchive _instance;

	public ZipArchiveWrapper(IFileSystem fileSystem, ZipArchive instance)
	{
		_instance = instance;
		FileSystem = fileSystem;
	}

	#region IZipArchive Members

	/// <inheritdoc cref="IZipArchive.Entries" />
	public ReadOnlyCollection<IZipArchiveEntry> Entries
		=> MapToZipArchiveEntries(_instance.Entries);

	/// <inheritdoc cref="IFileSystemExtensionPoint.FileSystem" />
	public IFileSystem FileSystem { get; }

	/// <inheritdoc cref="IZipArchive.Mode" />
	public ZipArchiveMode Mode
		=> _instance.Mode;

	/// <inheritdoc cref="IDisposable.Dispose()" />
	public void Dispose()
		=> _instance.Dispose();

	/// <inheritdoc cref="IZipArchive.CreateEntry(string)" />
	public IZipArchiveEntry CreateEntry(string entryName)
		=> ZipArchiveEntryWrapper.New(FileSystem, this, _instance.CreateEntry(entryName));

	/// <inheritdoc cref="IZipArchive.CreateEntry(string, CompressionLevel)" />
	public IZipArchiveEntry CreateEntry(string entryName,
	                                    CompressionLevel compressionLevel)
		=> ZipArchiveEntryWrapper.New(FileSystem, this,
			_instance.CreateEntry(entryName, compressionLevel));

	/// <inheritdoc cref="IZipArchive.CreateEntryFromFile(string, string)" />
	public IZipArchiveEntry CreateEntryFromFile(string sourceFileName, string entryName)
		=> Execute.WhenRealFileSystem(FileSystem,
			() => ZipArchiveEntryWrapper.New(FileSystem, this,
				_instance.CreateEntryFromFile(
					sourceFileName,
					entryName)),
			() => ZipUtilities.CreateEntryFromFile(this,
				sourceFileName,
				entryName));

	/// <inheritdoc cref="IZipArchive.CreateEntryFromFile(string, string, CompressionLevel)" />
	public IZipArchiveEntry CreateEntryFromFile(string sourceFileName, string entryName,
	                                            CompressionLevel compressionLevel)
		=> Execute.WhenRealFileSystem(FileSystem,
			() => ZipArchiveEntryWrapper.New(FileSystem, this,
				_instance.CreateEntryFromFile(
					sourceFileName,
					entryName,
					compressionLevel)),
			() => ZipUtilities.CreateEntryFromFile(this,
				sourceFileName,
				entryName,
				compressionLevel));

	/// <inheritdoc cref="IZipArchive.ExtractToDirectory(string)" />
	public void ExtractToDirectory(string destinationDirectoryName)
	{
		if (destinationDirectoryName == null)
		{
			throw new ArgumentNullException(nameof(destinationDirectoryName));
		}

		Execute.WhenRealFileSystem(FileSystem,
			() => _instance.ExtractToDirectory(destinationDirectoryName),
			() =>
			{
				foreach (IZipArchiveEntry entry in Entries)
				{
					entry.ExtractRelativeToDirectory(destinationDirectoryName, false);
				}
			});
	}

#if FEATURE_COMPRESSION_ADVANCED
	/// <inheritdoc cref="IZipArchive.ExtractToDirectory(string, bool)" />
	public void ExtractToDirectory(string destinationDirectoryName, bool overwriteFiles)
	{
		if (destinationDirectoryName == null)
		{
			throw new ArgumentNullException(nameof(destinationDirectoryName));
		}

		Execute.WhenRealFileSystem(FileSystem,
			() => _instance.ExtractToDirectory(destinationDirectoryName, overwriteFiles),
			() =>
			{
				foreach (IZipArchiveEntry entry in Entries)
				{
					entry.ExtractRelativeToDirectory(destinationDirectoryName,
						overwriteFiles);
				}
			});
	}
#endif

	/// <inheritdoc cref="IZipArchive.GetEntry(string)" />
	public IZipArchiveEntry? GetEntry(string entryName)
		=> ZipArchiveEntryWrapper.New(FileSystem, this, _instance.GetEntry(entryName));

	#endregion

	private ReadOnlyCollection<IZipArchiveEntry> MapToZipArchiveEntries(
		ReadOnlyCollection<ZipArchiveEntry> zipArchiveEntries)
		=> new(zipArchiveEntries
		   .Select(e => ZipArchiveEntryWrapper.New(FileSystem, this, e))
		   .ToList());
}