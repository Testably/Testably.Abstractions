#if FEATURE_COMPRESSION_ASYNC
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Testably.Abstractions.Compression.Tests.ZipArchiveEntry;

public partial class ExtensionTests
{
	[Fact]
	public async Task
		ExtractToFileAsync_AccessLengthOnWritableStream_ShouldThrowInvalidOperationException()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo.txt", "some content");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);
		using FileSystemStream stream = FileSystem.File.Open("destination.zip",
			FileMode.Open, FileAccess.ReadWrite);
		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Update);
		archive.CreateEntryFromFile("foo.txt", "foo/");

		Task Act()
			=> archive.ExtractToDirectoryAsync("bar");

		await That(Act).Throws<InvalidOperationException>();
	}

	[Theory]
	[AutoData]
	public async Task ExtractToFileAsync_DestinationNull_ShouldThrowArgumentNullException(
		CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo").Initialized(s => s
				.WithAFile());

		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", compressionLevel, false);

		async Task Act()
		{
			using IZipArchive archive =
				FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

			await archive.Entries.Single().ExtractToFileAsync(null!, TestContext.Current.CancellationToken);
		}

		await That(Act).Throws<ArgumentNullException>()
			.WithParamName("destinationFileName");
	}

	[Theory]
	[AutoData]
	public async Task
		ExtractToFileAsync_DestinationNull_WithOverwrite_ShouldThrowArgumentNullException(
			CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo").Initialized(s => s
				.WithAFile());

		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", compressionLevel, false);

		async Task Act()
		{
			using IZipArchive archive =
				FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

			await archive.Entries.Single().ExtractToFileAsync(null!, true, TestContext.Current.CancellationToken);
		}

		await That(Act).Throws<ArgumentNullException>();
	}

	[Fact]
	public async Task ExtractToFileAsync_IncorrectEntryType_ShouldThrowIOException()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo.txt", "some content");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);
		using FileSystemStream stream = FileSystem.File.Open("destination.zip",
			FileMode.Open, FileAccess.ReadWrite);
		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Update);
		archive.CreateEntryFromFile("foo.txt", "foo/");
		archive.Dispose();

		using FileSystemStream stream2 = FileSystem.File.OpenRead("destination.zip");
		IZipArchive archive2 = FileSystem.ZipArchive().New(stream2, ZipArchiveMode.Read);

		Task Act()
			=> archive2.ExtractToDirectoryAsync("bar");

		await That(Act).Throws<IOException>();
	}

	[Theory]
	[InlineData("2000-01-01T12:14:15")]
	[InlineData("1980-01-01T00:00:00")]
	[InlineData("2107-12-31T23:59:59")]
	public async Task ExtractToFileAsync_LastWriteTime_ShouldBeCopiedFromFile(string lastWriteTimeString)
	{
		DateTime lastWriteTime = DateTime.Parse(lastWriteTimeString, CultureInfo.InvariantCulture);
		FileSystem.Initialize()
			.WithSubdirectory("foo")
			.WithSubdirectory("bar").Initialized(s => s
				.WithFile("bar.txt"));
		FileSystem.File.WriteAllText("bar/foo.txt", "FooFooFoo");
		FileSystem.File.SetLastWriteTime("bar/foo.txt", lastWriteTime);
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);
		using FileSystemStream stream = FileSystem.File.Open("destination.zip",
			FileMode.Open, FileAccess.ReadWrite);
		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Update);
		archive.CreateEntryFromFile("bar/foo.txt", "foo/bar.txt",
			CompressionLevel.NoCompression);
		IZipArchiveEntry entry = archive.Entries.Single();

		await entry.ExtractToFileAsync("bar/bar.txt", true, TestContext.Current.CancellationToken);

		await That(FileSystem).HasFile("bar/bar.txt")
			.WithContent("FooFooFoo").And
			.WithLastWriteTime(lastWriteTime);
	}

	[Fact]
	public async Task ExtractToFileAsync_WithoutOverwrite_ShouldThrowIOException()
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

		Task Act()
			=> entry.ExtractToFileAsync("bar/bar.txt");

		await That(Act).Throws<IOException>()
			.WithMessage($"*'{FileSystem.Path.GetFullPath("bar/bar.txt")}'*").AsWildcard();
		await That(FileSystem).HasFile("bar/bar.txt")
			.WhoseContent(f => f.IsNotEqualTo("FooFooFoo"));
	}

	[Fact]
	public async Task ExtractToFileAsync_WithOverwrite_ShouldOverwriteExistingFile()
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

		await entry.ExtractToFileAsync("bar/bar.txt", true, TestContext.Current.CancellationToken);

		await That(FileSystem).HasFile("bar/bar.txt")
			.WithContent("FooFooFoo");
	}
}
#endif
