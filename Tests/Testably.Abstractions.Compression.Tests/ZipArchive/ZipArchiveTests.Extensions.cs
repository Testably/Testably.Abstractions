using System.IO;
using System.IO.Compression;
using System.Linq;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Compression.Tests.ZipArchive;

public abstract partial class ZipArchiveTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[InlineData("2000-01-01T12:14:15")]
	[InlineData("1980-01-01T00:00:00")]
	[InlineData("2107-12-31T23:59:59")]
	public void CreateEntryFromFile_LastWriteTime_ShouldBeCopiedFromFile(
		string lastWriteTimeString)
	{
		DateTime lastWriteTime = DateTime.Parse(lastWriteTimeString);
		FileSystem.Initialize()
		   .WithSubdirectory("foo")
		   .WithSubdirectory("bar");
		FileSystem.File.WriteAllText("bar/foo.txt", "FooFooFoo");
		FileSystem.File.SetLastWriteTime("bar/foo.txt", lastWriteTime);
		FileSystem.ZipFile()
		   .CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);
		using FileSystemStream stream = FileSystem.File.Open("destination.zip",
			FileMode.Open, FileAccess.ReadWrite);
		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Update);
		archive.Entries.Count.Should().Be(0);

		archive.CreateEntryFromFile("bar/foo.txt", "foo/bar.txt",
			CompressionLevel.NoCompression);

		IZipArchiveEntry entry = archive.Entries.Single();
		entry.LastWriteTime.DateTime.Should().Be(lastWriteTime);
	}

	[SkippableTheory]
	[InlineData("1930-06-21T14:15:16")]
	[InlineData("1979-12-31T00:00:00")]
	[InlineData("2108-01-01T00:00:00")]
	[InlineData("2208-01-01T00:00:00")]
	public void CreateEntryFromFile_LastWriteTimeOutOfRange_ShouldBeFirstJanuary1980(
		string lastWriteTimeString)
	{
		DateTime expectedTime = new(1980, 1, 1, 0, 0, 0);
		DateTime lastWriteTime = DateTime.Parse(lastWriteTimeString).ToUniversalTime();
		FileSystem.Initialize()
		   .WithSubdirectory("foo")
		   .WithSubdirectory("bar");
		FileSystem.File.WriteAllText("bar/foo.txt", "FooFooFoo");
		FileSystem.File.SetLastWriteTime("bar/foo.txt", lastWriteTime);
		FileSystem.ZipFile()
		   .CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);
		using FileSystemStream stream = FileSystem.File.Open("destination.zip",
			FileMode.Open, FileAccess.ReadWrite);
		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Update);
		archive.Entries.Count.Should().Be(0);

		archive.CreateEntryFromFile("bar/foo.txt", "foo/bar.txt",
			CompressionLevel.NoCompression);

		IZipArchiveEntry entry = archive.Entries.Single();
		entry.LastWriteTime.DateTime.Should().Be(expectedTime);
	}

	[SkippableFact]
	public void CreateEntryFromFile_NullEntryName_ShouldThrowArgumentNullException()
	{
		FileSystem.Initialize()
		   .WithSubdirectory("foo")
		   .WithSubdirectory("bar");
		FileSystem.File.WriteAllText("bar/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
		   .CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);
		using FileSystemStream stream = FileSystem.File.Open("destination.zip",
			FileMode.Open, FileAccess.ReadWrite);
		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Update);
		archive.Entries.Count.Should().Be(0);

		Exception? exception = Record.Exception(() =>
		{
			archive.CreateEntryFromFile("bar/foo.txt", null!);
		});

		exception.Should().BeOfType<ArgumentNullException>()
		   .Which.ParamName.Should().Be("entryName");
	}

	[SkippableFact]
	public void CreateEntryFromFile_NullSourceFileName_ShouldThrowArgumentNullException()
	{
		FileSystem.Initialize()
		   .WithSubdirectory("foo")
		   .WithSubdirectory("bar");
		FileSystem.File.WriteAllText("bar/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
		   .CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);
		using FileSystemStream stream = FileSystem.File.Open("destination.zip",
			FileMode.Open, FileAccess.ReadWrite);
		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Update);
		archive.Entries.Count.Should().Be(0);

		Exception? exception = Record.Exception(() =>
		{
			archive.CreateEntryFromFile(null!, "foo/bar.txt",
				CompressionLevel.NoCompression);
		});

		exception.Should().BeOfType<ArgumentNullException>()
		   .Which.ParamName.Should().Be("sourceFileName");
	}

	[SkippableFact]
	public void CreateEntryFromFile_ReadOnlyArchive_ShouldThrowNotSupportedException()
	{
		FileSystem.Initialize()
		   .WithSubdirectory("foo")
		   .WithSubdirectory("bar");
		FileSystem.File.WriteAllText("bar/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
		   .CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);
		using FileSystemStream stream = FileSystem.File.Open("destination.zip",
			FileMode.Open, FileAccess.ReadWrite);
		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);
		archive.Entries.Count.Should().Be(0);

		Exception? exception = Record.Exception(() =>
		{
			archive.CreateEntryFromFile("bar/foo.txt", "foo/bar.txt");
		});

		exception.Should().BeOfType<NotSupportedException>();
	}

	[SkippableFact]
	public void CreateEntryFromFile_ShouldCreateEntryWithFileContent()
	{
		FileSystem.Initialize()
		   .WithSubdirectory("foo")
		   .WithSubdirectory("bar");
		FileSystem.File.WriteAllText("bar/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
		   .CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);
		using FileSystemStream stream = FileSystem.File.Open("destination.zip",
			FileMode.Open, FileAccess.ReadWrite);
		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Update);
		archive.Entries.Count.Should().Be(0);

		archive.CreateEntryFromFile("bar/foo.txt", "foo/bar.txt",
			CompressionLevel.NoCompression);

		IZipArchiveEntry entry = archive.Entries.Single();
		entry.FullName.Should().Be("foo/bar.txt");

		entry.ExtractToFile("test.txt");
		FileSystem.File.ReadAllText("test.txt").Should().Be("FooFooFoo");
	}

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

		exception.Should().BeOfType<ArgumentNullException>()
		   .Which.ParamName.Should().Be("destinationDirectoryName");
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
	public void ExtractToDirectory_WithoutOverwrite_ShouldThrowIOException()
	{
		FileSystem.Initialize()
		   .WithSubdirectory("foo")
		   .WithSubdirectory("bar").Initialized(s => s
			   .WithFile("foo.txt"));
		FileSystem.File.WriteAllText("foo/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
		   .CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);
		using FileSystemStream stream = FileSystem.File.OpenRead("destination.zip");
		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		Exception? exception = Record.Exception(() =>
		{
			archive.ExtractToDirectory("bar");
		});

		exception.Should().BeOfType<IOException>()
		   .Which.Message.Should()
		   .Contain($"'{FileSystem.Path.GetFullPath("bar/foo.txt")}'");
		FileSystem.File.ReadAllText("bar/foo.txt")
		   .Should().NotBe("FooFooFoo");
	}

	[SkippableFact]
	public void ExtractToDirectory_ShouldExtractFilesAndDirectories()
	{
		FileSystem.Initialize()
		   .WithSubdirectory("foo").Initialized(s => s
			   .WithSubdirectory("bar")
			   .WithFile("bar.txt"));
		FileSystem.File.WriteAllText("foo/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
		   .CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);
		using FileSystemStream stream = FileSystem.File.OpenRead("destination.zip");
		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		archive.ExtractToDirectory("bar");

		FileSystem.File.ReadAllText("bar/foo.txt")
		   .Should().Be("FooFooFoo");
		FileSystem.Directory.Exists("bar/bar")
		   .Should().BeTrue();
		FileSystem.File.Exists("bar/bar.txt")
		   .Should().BeTrue();
	}

#if FEATURE_COMPRESSION_ADVANCED
	[SkippableFact]
	public void ExtractToDirectory_WithOverwrite_ShouldOverwriteExistingFile()
	{
		FileSystem.Initialize()
		   .WithSubdirectory("foo")
		   .WithSubdirectory("bar").Initialized(s => s
			   .WithFile("foo.txt"));
		FileSystem.File.WriteAllText("foo/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
		   .CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);
		using FileSystemStream stream = FileSystem.File.OpenRead("destination.zip");
		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		archive.ExtractToDirectory("bar", true);

		FileSystem.File.ReadAllText("bar/foo.txt")
		   .Should().Be("FooFooFoo");
	}
#endif
}