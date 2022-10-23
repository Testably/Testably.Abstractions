using System;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Linq;

namespace Testably.Abstractions;

internal class ZipArchiveWrapper : IZipArchive
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
		=> new(_instance.Entries
		   .Select(e => ZipArchiveEntryWrapper.New(FileSystem, this, e))
		   .ToList());

	/// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
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

	/// <inheritdoc cref="IZipArchive.GetEntry(string)" />
	public IZipArchiveEntry? GetEntry(string entryName)
		=> ZipArchiveEntryWrapper.New(FileSystem, this, _instance.GetEntry(entryName));

	#endregion
}