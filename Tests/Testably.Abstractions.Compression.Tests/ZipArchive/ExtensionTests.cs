using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Testably.Abstractions.Compression.Tests.ZipArchive;

[FileSystemTests]
public partial class ExtensionTests
{
	[SkippableTheory]
	[InlineData("2000-01-01T12:14:15")]
	[InlineData("1980-01-01T00:00:00")]
	[InlineData("2107-12-31T23:59:59")]
	public async Task CreateEntryFromFile_LastWriteTime_ShouldBeCopiedFromFile(
		string lastWriteTimeString)
	{
		DateTime lastWriteTime = DateTime.Parse(lastWriteTimeString, CultureInfo.InvariantCulture);
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
		await That(archive.Entries).IsEmpty();

		archive.CreateEntryFromFile("bar/foo.txt", "foo/bar.txt",
			CompressionLevel.NoCompression);

		IZipArchiveEntry entry = archive.Entries.Single();
		await That(entry.LastWriteTime.DateTime).Is(lastWriteTime);
	}

	[SkippableTheory]
	[InlineData("1930-06-21T14:15:16")]
	[InlineData("1979-12-31T00:00:00")]
	[InlineData("2108-01-01T00:00:00")]
	[InlineData("2208-01-01T00:00:00")]
	public async Task CreateEntryFromFile_LastWriteTimeOutOfRange_ShouldBeFirstJanuary1980(
		string lastWriteTimeString)
	{
		DateTime expectedTime = new(1980, 1, 1, 0, 0, 0);
		DateTime lastWriteTime = DateTime.Parse(lastWriteTimeString, CultureInfo.InvariantCulture)
			.ToUniversalTime();
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
		await That(archive.Entries).IsEmpty();

		archive.CreateEntryFromFile("bar/foo.txt", "foo/bar.txt",
			CompressionLevel.NoCompression);

		IZipArchiveEntry entry = archive.Entries.Single();
		await That(entry.LastWriteTime.DateTime).Is(expectedTime);
	}

	[SkippableFact]
	public async Task CreateEntryFromFile_NullEntryName_ShouldThrowArgumentNullException()
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
		await That(archive.Entries).IsEmpty();

		void Act()
			=> archive.CreateEntryFromFile("bar/foo.txt", null!);

		await That(Act).Throws<ArgumentNullException>()
			.WithParamName("entryName");
	}

	[SkippableFact]
	public async Task CreateEntryFromFile_NullSourceFileName_ShouldThrowArgumentNullException()
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
		await That(archive.Entries).IsEmpty();

		void Act()
			=> archive.CreateEntryFromFile(null!, "foo/bar.txt",
				CompressionLevel.NoCompression);

		await That(Act).Throws<ArgumentNullException>()
			.WithParamName("sourceFileName");
	}

	[SkippableFact]
	public async Task CreateEntryFromFile_ReadOnlyArchive_ShouldThrowNotSupportedException()
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
		await That(archive.Entries).IsEmpty();

		void Act()
			=> archive.CreateEntryFromFile("bar/foo.txt", "foo/bar.txt");

		await That(Act).Throws<NotSupportedException>();
	}

	[SkippableFact]
	public async Task CreateEntryFromFile_ShouldCreateEntryWithFileContent()
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
		await That(archive.Entries).IsEmpty();

		archive.CreateEntryFromFile("bar/foo.txt", "foo/bar.txt",
			CompressionLevel.NoCompression);

		IZipArchiveEntry entry = archive.Entries.Single();
		await That(entry.FullName).Is("foo/bar.txt");

		entry.ExtractToFile("test.txt");
		await That(FileSystem).HasFile("test.txt").WithContent("FooFooFoo");
	}

	[SkippableTheory]
	[AutoData]
	public async Task ExtractToDirectory_DestinationNull_ShouldThrowArgumentNullException(
		CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");

		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", compressionLevel, false);

		void Act()
		{
			using IZipArchive archive =
				FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

			archive.ExtractToDirectory(null!);
		}

		await That(Act).Throws<ArgumentNullException>()
			.WithParamName("destinationDirectoryName");
	}

#if FEATURE_COMPRESSION_ADVANCED
	[SkippableTheory]
	[AutoData]
	public async Task
		ExtractToDirectory_DestinationNull_WithOverwrite_ShouldThrowArgumentNullException(
			CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");

		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", compressionLevel, false);

		void Act()
		{
			using IZipArchive archive =
				FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

			archive.ExtractToDirectory(null!, true);
		}

		await That(Act).Throws<ArgumentNullException>();
	}
#endif

	[SkippableFact]
	public async Task ExtractToDirectory_ShouldExtractFilesAndDirectories()
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

		await That(FileSystem).HasFile("bar/foo.txt").WithContent("FooFooFoo");
		await That(FileSystem).HasDirectory("bar/bar");
		await That(FileSystem).HasFile("bar/bar.txt");
	}

	[SkippableFact]
	public async Task ExtractToDirectory_WithoutOverwrite_ShouldThrowIOException()
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

		void Act()
		{
			archive.ExtractToDirectory("bar");
		}

		await That(Act).Throws<IOException>()
			.WithMessage($"*'{FileSystem.Path.GetFullPath("bar/foo.txt")}'*").AsWildcard();
		await That(FileSystem).HasFile("bar/foo.txt")
			.WhichContent(c => c.IsNot("FooFooFoo"));
	}

#if FEATURE_COMPRESSION_ADVANCED
	[SkippableFact]
	public async Task ExtractToDirectory_WithOverwrite_ShouldOverwriteExistingFile()
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

		await That(FileSystem).HasFile("bar/foo.txt")
			.WithContent("FooFooFoo");
	}
#endif
}
