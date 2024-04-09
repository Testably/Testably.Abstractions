using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Testably.Abstractions.Compression.Tests.ZipArchiveEntry;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Tests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void Archive_ShouldBeSetToArchive()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);

		using FileSystemStream stream = FileSystem.File.OpenRead("destination.zip");

		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		IZipArchiveEntry entry = archive.Entries.Single();

		entry.Archive.Should().Be(archive);
	}

#if FEATURE_ZIPFILE_NET7
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
		IZipArchiveEntry entry = archive.Entries.Single();

		entry.Comment.Should().Be("");
	}
#endif

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
		IZipArchiveEntry entry = archive.Entries.Single();

		entry.Comment = comment;

		entry.Comment.Should().Be(comment);
	}
#endif

	[SkippableFact]
	public void CompressedLength_WithNoCompression_ShouldBeFileLength()
	{
		Skip.If(Test.IsNetFramework, "Test is brittle on .NET Framework.");

		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);

		using FileSystemStream stream = FileSystem.File.OpenRead("destination.zip");

		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		archive.Entries.Single().Length.Should().Be(9);
		archive.Entries.Single().CompressedLength.Should().Be(9);
	}

	[SkippableFact]
	public void CompressedLength_WithOptimalCompressionLevel_ShouldBeLessThanFileLength()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.Optimal,
				false);

		using FileSystemStream stream = FileSystem.File.OpenRead("destination.zip");

		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		archive.Entries.Single().Length.Should().Be(9);
		archive.Entries.Single().CompressedLength.Should().BeLessThan(9);
	}

#if FEATURE_COMPRESSION_ADVANCED
	[SkippableFact]
	public void Crc32_ShouldBeCalculatedFromTheFileContent()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo/foo1.txt", "FooFooFoo");
		FileSystem.File.WriteAllText("foo/foo2.txt", "Some other text");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);

		using FileSystemStream stream = FileSystem.File.OpenRead("destination.zip");

		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		IZipArchiveEntry entry1 = archive.Entries[0];
		IZipArchiveEntry entry2 = archive.Entries[1];

		entry1.Crc32.Should().NotBe(entry2.Crc32);
	}
#endif

	[SkippableFact]
	public void Delete_ReadMode_ShouldThrowNotSupportedException()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);

		using FileSystemStream stream =
			FileSystem.File.Open("destination.zip", FileMode.Open, FileAccess.ReadWrite);

		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);
		IZipArchiveEntry entry = archive.Entries.Single();

		Exception? exception = Record.Exception(() => entry.Delete());

		exception.Should().BeOfType<NotSupportedException>();
	}

	[SkippableFact]
	public void Delete_ShouldRemoveEntryFromArchive()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);

		using FileSystemStream stream =
			FileSystem.File.Open("destination.zip", FileMode.Open, FileAccess.ReadWrite);

		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Update);
		IZipArchiveEntry entry = archive.Entries.Single();

		entry.Delete();

		archive.Entries.Should().HaveCount(0);
	}

#if FEATURE_COMPRESSION_ADVANCED
	[SkippableTheory]
	[AutoData]
	public void ExternalAttributes_ShouldBeSettable(int externalAttributes)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo/foo1.txt", "FooFooFoo");
		FileSystem.File.WriteAllText("foo/foo2.txt", "FooFooFoo");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);

		using FileSystemStream stream = FileSystem.File.OpenRead("destination.zip");

		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		IZipArchiveEntry entry1 = archive.Entries[0];
		IZipArchiveEntry entry2 = archive.Entries[1];

		entry1.ExternalAttributes = externalAttributes;
		entry1.ExternalAttributes.Should().Be(externalAttributes);
		entry2.ExternalAttributes.Should().NotBe(externalAttributes);
	}
#endif

	[SkippableFact]
	public void FileSystemExtension_ShouldBeSet()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);

		using FileSystemStream stream = FileSystem.File.OpenRead("destination.zip");

		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		IZipArchiveEntry entry = archive.Entries.Single();

		entry.FileSystem.Should().Be(FileSystem);
	}

	[SkippableFact]
	public void FullName_ShouldIncludeDirectory()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				true);

		using FileSystemStream stream = FileSystem.File.OpenRead("destination.zip");

		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		IZipArchiveEntry entry = archive.Entries.Single();

		entry.FullName.Should().Be("foo/foo.txt");
		entry.Name.Should().Be("foo.txt");
	}

	[SkippableTheory]
	[AutoData]
	public void LastWriteTime_ReadOnlyArchive_ShouldThrowNotSupportedException(
		DateTime lastWriteTime)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo/foo1.txt", "FooFooFoo");
		FileSystem.File.WriteAllText("foo/foo2.txt", "FooFooFoo");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);

		using FileSystemStream stream = FileSystem.File.OpenRead("destination.zip");

		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		IZipArchiveEntry entry1 = archive.Entries[0];
		Exception? exception = Record.Exception(() =>
		{
			entry1.LastWriteTime = new DateTimeOffset(lastWriteTime);
		});

		exception.Should().BeOfType<NotSupportedException>();
	}

	[SkippableTheory]
	[AutoData]
	public void LastWriteTime_ShouldBeSettable(DateTime lastWriteTime)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo/foo1.txt", "FooFooFoo");
		FileSystem.File.WriteAllText("foo/foo2.txt", "FooFooFoo");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);

		using FileSystemStream stream = FileSystem.File.Open("destination.zip",
			FileMode.Open, FileAccess.ReadWrite);

		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Update);

		IZipArchiveEntry entry1 = archive.Entries[0];
		IZipArchiveEntry entry2 = archive.Entries[1];

		entry1.LastWriteTime = new DateTimeOffset(lastWriteTime);
		entry1.LastWriteTime.DateTime.Should().Be(lastWriteTime);
		entry2.LastWriteTime.DateTime.Should().NotBe(lastWriteTime);
	}

	[SkippableFact]
	public void ToString_ShouldBeSetToFileName()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false);

		using FileSystemStream stream =
			FileSystem.File.Open("destination.zip", FileMode.Open, FileAccess.ReadWrite);

		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Update);
		IZipArchiveEntry entry = archive.Entries.Single();

		string? result = entry.ToString();

		result.Should().Be("foo.txt");
	}
}
