﻿using System.IO.Compression;

namespace Testably.Abstractions.Compression.Tests.ZipArchive;

public abstract partial class ZipArchiveTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void ExtractToDirectory_DestinationNull_ShouldThrowArgumentNullException(
		CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
		   .WithSubdirectory("foo");

		FileSystem.ZipFile()
		   .CreateFromDirectory("foo", "destination.zip", compressionLevel, false);

		Exception? exception = Record.Exception(() =>
		{
			using IZipArchive archive =
				FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

			archive.ExtractToDirectory(null!);
		});

		exception.Should().BeOfType<ArgumentNullException>();
	}

#if FEATURE_COMPRESSION_ADVANCED
	[SkippableTheory]
	[AutoData]
	public void
		ExtractToDirectory_DestinationNull_WithOverwrite_ShouldThrowArgumentNullException(
			CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
		   .WithSubdirectory("foo");

		FileSystem.ZipFile()
		   .CreateFromDirectory("foo", "destination.zip", compressionLevel, false);

		Exception? exception = Record.Exception(() =>
		{
			using IZipArchive archive =
				FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

			archive.ExtractToDirectory(null!, true);
		});

		exception.Should().BeOfType<ArgumentNullException>();
	}
#endif
}