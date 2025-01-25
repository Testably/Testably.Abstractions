using System.IO;
using System.IO.Compression;
using System.Linq;
using Skip = Xunit.Skip;

namespace Testably.Abstractions.Compression.Tests.ZipArchiveEntry;

[FileSystemTests]
public partial class Tests
{
	[SkippableFact]
	public async Task Archive_ShouldBeSetToArchive()
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

		await That(entry.Archive).Is(archive);
	}

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
		IZipArchiveEntry entry = archive.Entries.Single();

		await That(entry.Comment).Is("");
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
		IZipArchiveEntry entry = archive.Entries.Single();

		entry.Comment = comment;

		await That(entry.Comment).Is(comment);
	}
#endif

	[SkippableFact]
	public async Task CompressedLength_WithNoCompression_ShouldBeFileLength()
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

		await That(archive.Entries.Single())
			.For(x => x.Length, l => l.Is(9)).And
			.For(x => x.CompressedLength, l => l.Is(9));
	}

	[SkippableFact]
	public async Task CompressedLength_WithOptimalCompressionLevel_ShouldBeLessThanFileLength()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		FileSystem.File.WriteAllText("foo/foo.txt", "FooFooFoo");
		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", CompressionLevel.Optimal,
				false);

		using FileSystemStream stream = FileSystem.File.OpenRead("destination.zip");

		IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		await That(archive.Entries.Single())
			.For(x => x.Length, l => l.Is(9)).And
			.For(x => x.CompressedLength, l => l.IsLessThan(9));
	}

#if FEATURE_COMPRESSION_ADVANCED
	[SkippableFact]
	public async Task Crc32_ShouldBeCalculatedFromTheFileContent()
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

		await That(entry1.Crc32).IsNot(entry2.Crc32);
	}
#endif

	[SkippableFact]
	public async Task Delete_ReadMode_ShouldThrowNotSupportedException()
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

		void Act() => entry.Delete();

		await That(Act).Throws<NotSupportedException>();
	}

	[SkippableFact]
	public async Task Delete_ShouldRemoveEntryFromArchive()
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

		await That(archive.Entries).IsEmpty();
	}

#if FEATURE_COMPRESSION_ADVANCED
	[SkippableTheory]
	[AutoData]
	public async Task ExternalAttributes_ShouldBeSettable(int externalAttributes)
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
		await That(entry1.ExternalAttributes).Is(externalAttributes);
		await That(entry2.ExternalAttributes).IsNot(externalAttributes);
	}
#endif

	[SkippableFact]
	public async Task FileSystemExtension_ShouldBeSet()
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

		await That(entry.FileSystem).Is(FileSystem);
	}

	[SkippableFact]
	public async Task FullName_ShouldIncludeDirectory()
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

		await That(entry.FullName).Is("foo/foo.txt");
		await That(entry.Name).Is("foo.txt");
	}

	[SkippableTheory]
	[AutoData]
	public async Task LastWriteTime_ReadOnlyArchive_ShouldThrowNotSupportedException(
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
		void Act()
		{
			entry1.LastWriteTime = new DateTimeOffset(lastWriteTime);
		}

		await That(Act).Throws<NotSupportedException>();
	}

	[SkippableTheory]
	[AutoData]
	public async Task LastWriteTime_ShouldBeSettable(DateTime lastWriteTime)
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
		await That(entry1.LastWriteTime.DateTime).Is(lastWriteTime);
		await That(entry2.LastWriteTime.DateTime).IsNot(lastWriteTime);
	}

	[SkippableFact]
	public async Task ToString_ShouldBeSetToFileName()
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

		await That(result).Is("foo.txt");
	}
}
