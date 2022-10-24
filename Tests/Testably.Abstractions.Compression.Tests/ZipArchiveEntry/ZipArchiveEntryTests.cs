using System.IO;
using System.IO.Compression;
using System.Linq;
using Testably.Abstractions.Compression.Tests.TestHelpers;

namespace Testably.Abstractions.Compression.Tests.ZipArchiveEntry;

public abstract class ZipArchiveEntryTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	public abstract string BasePath { get; }
	public TFileSystem FileSystem { get; }
	public ITimeSystem TimeSystem { get; }

	protected ZipArchiveEntryTests(
		TFileSystem fileSystem,
		ITimeSystem timeSystem)
	{
		FileSystem = fileSystem;
		TimeSystem = timeSystem;
	}

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
			entry1.LastWriteTime = lastWriteTime;
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

		entry1.LastWriteTime = lastWriteTime;
		entry1.LastWriteTime.Should().Be(lastWriteTime);
		entry2.LastWriteTime.Should().NotBe(lastWriteTime);
	}
}