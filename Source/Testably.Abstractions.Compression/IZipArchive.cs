using System;
using System.Collections.ObjectModel;
using System.IO.Compression;

namespace Testably.Abstractions;

/// <inheritdoc cref="ZipArchive" />
public interface IZipArchive : IFileSystem.IFileSystemExtensionPoint, IDisposable
{
	/// <inheritdoc cref="ZipArchive.Entries" />
	ReadOnlyCollection<IZipArchiveEntry> Entries { get; }

	/// <inheritdoc cref="ZipArchive.Mode" />
	ZipArchiveMode Mode { get; }

	/// <inheritdoc cref="ZipArchive.CreateEntry(string)" />
	IZipArchiveEntry CreateEntry(string entryName);

	/// <inheritdoc cref="ZipArchive.CreateEntry(string, CompressionLevel)" />
	IZipArchiveEntry CreateEntry(string entryName, CompressionLevel compressionLevel);

	/// <inheritdoc cref="ZipArchive.GetEntry(string)" />
	IZipArchiveEntry? GetEntry(string entryName);
}