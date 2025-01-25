using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;

namespace Testably.Abstractions.Compression.Tests.ZipArchive;

[FileSystemTests]
public partial class Tests
{
#if FEATURE_FILESYSTEM_COMMENT_ENCRYPTED
	[SkippableFact]
	public async Task Comment_ShouldBeInitializedEmpty()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);

		using FileSystemStream stream = FileSystem.File.OpenRead("destination.zip");

		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		await That(archive.Comment).Is("");
	}
#endif
#if FEATURE_FILESYSTEM_COMMENT_ENCRYPTED
	[SkippableTheory]
	[AutoData]
	public async Task Comment_ShouldBeSettable(string comment)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);

		using FileSystemStream stream = FileSystem.File.OpenRead("destination.zip");

		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);
		archive.Comment = comment;

		await That(archive.Comment).Is(comment);
	}
#endif

	[SkippableFact]
	public async Task Entries_CreateMode_ShouldThrowNotSupportedException()
	{
		using FileSystemStream stream =
			FileSystem.File.Open("destination.zip", FileMode.Create, FileAccess.ReadWrite);

		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Create);

		ReadOnlyCollection<IZipArchiveEntry> Act() => archive.Entries;

		await That(Act).Throws<NotSupportedException>();
	}

	[SkippableTheory]
	[AutoData]
	public async Task FileSystem_ShouldBeSet(
		CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");

		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", compressionLevel, false);

		using IZipArchive archive =
			FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

		await That(archive.FileSystem).Is(FileSystem);
	}

	[SkippableFact]
	public async Task GetEntry_WhenNameIsNotFound_ShouldReturnNull()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				true);

		using IZipArchive archive =
			FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

		await That(archive.GetEntry("bar.txt")).IsNull();
		await That(archive.GetEntry("foo.txt")).IsNull();
		await That(archive.GetEntry("foo/foo.txt")).IsNotNull();
	}

	[SkippableTheory]
	[AutoData]
	public async Task Mode_ShouldBeSetCorrectly(ZipArchiveMode mode)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");

		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.Fastest, false);

		using IZipArchive archive =
			FileSystem.ZipFile().Open("destination.zip", mode);

		await That(archive.Mode).Is(mode);
	}
}
