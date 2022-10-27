using System.IO.Compression;
using System.Linq;

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

	[SkippableFact]
	public void ExtractToDirectory_Null_ShouldThrowArgumentNullException()
	{
		IZipArchive? archive = null;
		Exception? exception = Record.Exception(() =>
		{
			archive!.ExtractToDirectory("foo");
		});

		exception.Should().BeOfType<ArgumentNullException>();
	}

#if FEATURE_COMPRESSION_ADVANCED
	[SkippableFact]
	public void ExtractToDirectory_Null_WithOverwrite_ShouldThrowArgumentNullException()
	{
		IZipArchive? archive = null;
		Exception? exception = Record.Exception(() =>
		{
			archive!.ExtractToDirectory("foo", true);
		});

		exception.Should().BeOfType<ArgumentNullException>();
	}
#endif

	[SkippableTheory]
	[AutoData]
	public void ExtractToFile_DestinationNull_ShouldThrowArgumentNullException(
		CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
		   .WithSubdirectory("foo").Initialized(s => s
			   .WithAFile());

		FileSystem.ZipFile()
		   .CreateFromDirectory("foo", "destination.zip", compressionLevel, false);

		Exception? exception = Record.Exception(() =>
		{
			using IZipArchive archive =
				FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

			archive.Entries.Single().ExtractToFile(null!);
		});

		exception.Should().BeOfType<ArgumentNullException>();
	}

	[SkippableTheory]
	[AutoData]
	public void
		ExtractToFile_DestinationNull_WithOverwrite_ShouldThrowArgumentNullException(
			CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
		   .WithSubdirectory("foo").Initialized(s => s
			   .WithAFile());

		FileSystem.ZipFile()
		   .CreateFromDirectory("foo", "destination.zip", compressionLevel, false);

		Exception? exception = Record.Exception(() =>
		{
			using IZipArchive archive =
				FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

			archive.Entries.Single().ExtractToFile(null!, true);
		});

		exception.Should().BeOfType<ArgumentNullException>();
	}

	[SkippableFact]
	public void ExtractToFile_Null_ShouldThrowArgumentNullException()
	{
		IZipArchiveEntry? archiveEntry = null;
		Exception? exception = Record.Exception(() =>
		{
			archiveEntry!.ExtractToFile("foo");
		});

		exception.Should().BeOfType<ArgumentNullException>();
	}

	[SkippableFact]
	public void ExtractToFile_Null_WithOverwrite_ShouldThrowArgumentNullException()
	{
		IZipArchiveEntry? archiveEntry = null;
		Exception? exception = Record.Exception(() =>
		{
			archiveEntry!.ExtractToFile("foo", true);
		});

		exception.Should().BeOfType<ArgumentNullException>();
	}
}