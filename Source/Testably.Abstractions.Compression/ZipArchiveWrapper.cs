using System;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Linq;
using Testably.Abstractions.Internal;
#if FEATURE_COMPRESSION_ASYNC
using System.Threading;
using System.Threading.Tasks;
#endif

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

#if FEATURE_FILESYSTEM_COMMENT_ENCRYPTED
	/// <inheritdoc cref="IZipArchiveEntry.Comment" />
	public string Comment
	{
		get => _instance.Comment;
		set => _instance.Comment = value;
	}
#endif

	/// <inheritdoc cref="IZipArchive.Entries" />
	public ReadOnlyCollection<IZipArchiveEntry> Entries
		=> MapToZipArchiveEntries(_instance.Entries);

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem { get; }

	/// <inheritdoc cref="IZipArchive.Mode" />
	public ZipArchiveMode Mode
		=> _instance.Mode;

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

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="IZipArchive.CreateEntryFromFileAsync(string, string, CancellationToken)" />
	public async Task<IZipArchiveEntry> CreateEntryFromFileAsync(string sourceFileName,
		string entryName,
		CancellationToken cancellationToken = default)
		=> await Execute.WhenRealFileSystemAsync(FileSystem,
			async () => ZipArchiveEntryWrapper.New(FileSystem, this,
				await _instance.CreateEntryFromFileAsync(
					sourceFileName,
					entryName,
					cancellationToken)),
			() => ZipUtilities.CreateEntryFromFile(this,
				sourceFileName,
				entryName));
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc
	///     cref="IZipArchive.CreateEntryFromFileAsync(string, string, CompressionLevel, CancellationToken)" />
	public async Task<IZipArchiveEntry> CreateEntryFromFileAsync(string sourceFileName,
		string entryName,
		CompressionLevel compressionLevel,
		CancellationToken cancellationToken = default)
		=> await Execute.WhenRealFileSystemAsync(FileSystem,
			async () => ZipArchiveEntryWrapper.New(FileSystem, this,
				await _instance.CreateEntryFromFileAsync(
					sourceFileName,
					entryName,
					compressionLevel,
					cancellationToken)),
			() => ZipUtilities.CreateEntryFromFile(this,
				sourceFileName,
				entryName,
				compressionLevel));
#endif

	/// <inheritdoc cref="IDisposable.Dispose()" />
	public void Dispose()
		=> _instance.Dispose();

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="IAsyncDisposable.DisposeAsync()" />
	public ValueTask DisposeAsync()
		=> _instance.DisposeAsync();
#endif

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
					entry.ExtractRelativeToDirectory(destinationDirectoryName, overwrite: false);
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

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="IZipArchive.ExtractToDirectoryAsync(string, CancellationToken)" />
	public async Task ExtractToDirectoryAsync(string destinationDirectoryName,
		CancellationToken cancellationToken = default)
	{
		if (destinationDirectoryName == null)
		{
			throw new ArgumentNullException(nameof(destinationDirectoryName));
		}

		await Execute.WhenRealFileSystemAsync(FileSystem,
			async () => await _instance.ExtractToDirectoryAsync(destinationDirectoryName,
				cancellationToken),
			() =>
			{
				foreach (IZipArchiveEntry entry in Entries)
				{
					entry.ExtractRelativeToDirectory(destinationDirectoryName, overwrite: false);
				}
			});
	}
#endif

#if FEATURE_COMPRESSION_ASYNC
	/// <inheritdoc cref="IZipArchive.ExtractToDirectoryAsync(string, bool, CancellationToken)" />
	public async Task ExtractToDirectoryAsync(string destinationDirectoryName,
		bool overwriteFiles,
		CancellationToken cancellationToken = default)
	{
		if (destinationDirectoryName == null)
		{
			throw new ArgumentNullException(nameof(destinationDirectoryName));
		}

		await Execute.WhenRealFileSystemAsync(FileSystem,
			async () => await _instance.ExtractToDirectoryAsync(destinationDirectoryName,
				overwriteFiles, cancellationToken),
			() =>
			{
				foreach (IZipArchiveEntry entry in Entries)
				{
					entry.ExtractRelativeToDirectory(destinationDirectoryName,
						overwrite: overwriteFiles);
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
