using System.IO.Compression;
using System.Linq;

namespace Testably.Abstractions.Compression.Tests.ZipArchiveEntry;

public abstract partial class ZipArchiveEntryTests<TFileSystem>
	where TFileSystem : IFileSystem
{
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
}