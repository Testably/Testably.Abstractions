using System.IO;
using System.IO.Compression;

namespace Testably.Abstractions.Compression.Tests.ZipArchive;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Tests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
#if FEATURE_ZIPFILE_NET7
	[SkippableTheory]
	[AutoData]
	public void Comment_ShouldBeSettable(string comment)
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

		archive.Comment.Should().Be(comment);
	}

	[SkippableFact]
	public void Comment_ShouldBeInitializedEmpty()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);

		using FileSystemStream stream = FileSystem.File.OpenRead("destination.zip");

		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		archive.Comment.Should().Be("");
	}
#endif

	[SkippableFact]
	public void Entries_CreateMode_ShouldThrowNotSupportedException()
	{
		using FileSystemStream stream =
			FileSystem.File.Open("destination.zip", FileMode.Create, FileAccess.ReadWrite);

		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Create);

		Exception? exception = Record.Exception(() => archive.Entries);

		exception.Should().BeOfType<NotSupportedException>();
	}

	[SkippableTheory]
	[AutoData]
	public void FileSystem_ShouldBeSet(
		CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");

		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", compressionLevel, false);

		using IZipArchive archive =
			FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

		archive.FileSystem.Should().Be(FileSystem);
	}

	[SkippableTheory]
	[AutoData]
	public void Mode_ShouldBeSetCorrectly(ZipArchiveMode mode)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");

		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.Fastest, false);

		using IZipArchive archive =
			FileSystem.ZipFile().Open("destination.zip", mode);

		archive.Mode.Should().Be(mode);
	}

	[SkippableFact]
	public void GetEntry_WhenNameIsNotFound_ShouldReturnNull()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				true);

		using IZipArchive archive =
			FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

		archive.GetEntry("bar.txt").Should().BeNull();
		archive.GetEntry("foo.txt").Should().BeNull();
		archive.GetEntry("foo/foo.txt").Should().NotBeNull();
	}
}
