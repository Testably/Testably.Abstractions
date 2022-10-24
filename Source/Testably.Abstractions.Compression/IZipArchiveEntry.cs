﻿using System;
using System.IO;
using System.IO.Compression;

namespace Testably.Abstractions;

/// <inheritdoc cref="ZipArchiveEntry" />
public interface IZipArchiveEntry : IFileSystem.IFileSystemExtensionPoint
{
	/// <inheritdoc cref="ZipArchiveEntry.Archive" />
	IZipArchive Archive { get; }

	/// <inheritdoc cref="ZipArchiveEntry.CompressedLength" />
	long CompressedLength { get; }

#if FEATURE_COMPRESSION_ADVANCED
	/// <inheritdoc cref="ZipArchiveEntry.Crc32" />
	uint Crc32 { get; }
#endif

#if FEATURE_COMPRESSION_ADVANCED
	/// <inheritdoc cref="ZipArchiveEntry.ExternalAttributes" />
	int ExternalAttributes { get; set; }
#endif

	/// <inheritdoc cref="ZipArchiveEntry.FullName" />
	string FullName { get; }

	/// <inheritdoc cref="ZipArchiveEntry.LastWriteTime" />
	DateTimeOffset LastWriteTime { get; set; }

	/// <inheritdoc cref="ZipArchiveEntry.Length" />
	long Length { get; }

	/// <inheritdoc cref="ZipArchiveEntry.Name" />
	string Name { get; }

	/// <inheritdoc cref="ZipArchiveEntry.Delete()" />
	void Delete();

	/// <inheritdoc cref="ZipArchiveEntry.Open()" />
	Stream Open();
}