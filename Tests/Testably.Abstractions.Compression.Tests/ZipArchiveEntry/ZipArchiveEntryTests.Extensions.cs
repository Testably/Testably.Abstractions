using System.IO;
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

	[SkippableFact]
	public void ExtractToFile_WithoutOverwrite_ShouldThrowIOException()
	{
		FileSystem.Initialize()
		   .WithSubdirectory("foo")
		   .WithSubdirectory("bar").Initialized(s => s
			   .WithFile("bar.txt"));
		FileSystem.File.WriteAllText("foo/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
		   .CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);
		using FileSystemStream stream = FileSystem.File.OpenRead("destination.zip");
		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);
		IZipArchiveEntry entry = archive.Entries.Single();

		Exception? exception = Record.Exception(() =>
		{
			entry.ExtractToFile("bar/bar.txt");
		});

		exception.Should().BeOfType<IOException>()
		   .Which.Message.Should()
		   .Contain($"'{FileSystem.Path.GetFullPath("bar/bar.txt")}'");
		FileSystem.File.ReadAllText("bar/bar.txt")
		   .Should().NotBe("FooFooFoo");
	}

	[SkippableFact]
	public void ExtractToFile_WithOverwrite_ShouldOverwriteExistingFile()
	{
		FileSystem.Initialize()
		   .WithSubdirectory("foo")
		   .WithSubdirectory("bar").Initialized(s => s
			   .WithFile("bar.txt"));
		FileSystem.File.WriteAllText("foo/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
		   .CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);
		using FileSystemStream stream = FileSystem.File.OpenRead("destination.zip");
		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);
		IZipArchiveEntry entry = archive.Entries.Single();

		entry.ExtractToFile("bar/bar.txt", true);

		FileSystem.File.ReadAllText("bar/bar.txt")
		   .Should().Be("FooFooFoo");
	}
}