using System.IO.Compression;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Compression.Tests.ZipArchive;

public abstract partial class ZipArchiveTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	public abstract string BasePath { get; }
	public TFileSystem FileSystem { get; }
	public ITimeSystem TimeSystem { get; }

	protected ZipArchiveTests(
		TFileSystem fileSystem,
		ITimeSystem timeSystem)
	{
		FileSystem = fileSystem;
		TimeSystem = timeSystem;
	}

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

	[SkippableTheory]
	[AutoData]
	public void FileSystemExtension_ShouldBeSet(
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