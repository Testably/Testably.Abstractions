using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Testably.Abstractions.Compression.Tests.ZipFile;

public abstract class ZipFileTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	public abstract string BasePath { get; }
	public TFileSystem FileSystem { get; }
	public ITimeSystem TimeSystem { get; }

	protected ZipFileTests(
		TFileSystem fileSystem,
		ITimeSystem timeSystem)
	{
		FileSystem = fileSystem;
		TimeSystem = timeSystem;
	}

	[SkippableTheory]
	[AutoData]
	public void CreateFromDirectory_EmptySource_DoNotIncludeBaseDirectory_ShouldBeEmpty(
		CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
		   .WithSubdirectory("foo");

		FileSystem.ZipFile()
		   .CreateFromDirectory("foo", "destination.zip", compressionLevel, false);

		using IZipArchive archive =
			FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

		archive.Entries.Count.Should().Be(0);
	}

	[SkippableTheory]
	[AutoData]
	public void
		CreateFromDirectory_EmptySource_IncludeBaseDirectory_ShouldPrependDirectoryName(
			CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
		   .WithSubdirectory("foo");

		FileSystem.ZipFile()
		   .CreateFromDirectory("foo", "destination.zip", compressionLevel, true);

		using IZipArchive archive =
			FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

		archive.Entries.Count.Should().Be(1);
		archive.Entries.Should().Contain(e => e.FullName.Equals("foo/"));
	}

	[SkippableTheory]
	[MemberData(nameof(EntryNameEncoding))]
	public void CreateFromDirectory_EntryNameEncoding_ShouldUseEncoding(
		string entryName, Encoding encoding, bool encodedCorrectly)
	{
		FileSystem.Initialize()
		   .WithSubdirectory("foo").Initialized(s => s
			   .WithFile(entryName));

		FileSystem.ZipFile()
		   .CreateFromDirectory("foo", "destination.zip", CompressionLevel.NoCompression,
				false, encoding);

		using IZipArchive archive =
			FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

		archive.Entries.Count.Should().Be(1);
		if (encodedCorrectly)
		{
			archive.Entries.Should().Contain(e => e.Name == entryName);
		}
		else
		{
			archive.Entries.Should().NotContain(e => e.Name == entryName);
		}
	}

	[SkippableTheory]
	[AutoData]
	public void CreateFromDirectory_IncludeBaseDirectory_ShouldPrependDirectoryName(
		CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
		   .WithSubdirectory("foo").Initialized(s => s
			   .WithFile("test.txt"));

		FileSystem.ZipFile()
		   .CreateFromDirectory("foo", "destination.zip", compressionLevel, true);

		using IZipArchive archive =
			FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

		archive.Entries.Count.Should().Be(1);
		archive.Entries.Should().Contain(e => e.FullName.Equals("foo/test.txt"));
	}

	[SkippableFact]
	public void CreateFromDirectory_MissingDestinationDirectory_ShouldCreateDirectory()
	{
		FileSystem.Initialize()
		   .WithSubdirectory("foo").Initialized(s => s
			   .WithFile("test.txt"));

		FileSystem.ZipFile().CreateFromDirectory("foo", "destination.zip");

		FileSystem.ZipFile().ExtractToDirectory("destination.zip", "bar");

		FileSystem.File.Exists(FileSystem.Path.Combine("bar", "test.txt"))
		   .Should().BeTrue();
		FileSystem.File.ReadAllBytes(FileSystem.Path.Combine("bar", "test.txt"))
		   .Should().BeEquivalentTo(
				FileSystem.File.ReadAllBytes(FileSystem.Path.Combine("foo", "test.txt")));
	}

#if FEATURE_COMPRESSION_OVERWRITE
	[SkippableTheory]
	[AutoData]
	public void CreateFromDirectory_Overwrite_ShouldOverwriteFile(
		string contents)
	{
		FileSystem.Initialize()
		   .WithSubdirectory("bar").Initialized(s => s
			   .WithFile("test.txt"))
		   .WithSubdirectory("foo").Initialized(s => s
			   .WithFile("test.txt"));
		FileSystem.File.WriteAllText(FileSystem.Path.Combine("foo", "test.txt"),
			contents);

		FileSystem.ZipFile().CreateFromDirectory("foo", "destination.zip");

		FileSystem.ZipFile().ExtractToDirectory("destination.zip", "bar", true);

		FileSystem.File.Exists(FileSystem.Path.Combine("bar", "test.txt"))
		   .Should().BeTrue();
		FileSystem.File.ReadAllText(FileSystem.Path.Combine("bar", "test.txt"))
		   .Should().Be(contents);
	}
#endif

#if FEATURE_COMPRESSION_OVERWRITE
	[SkippableTheory]
	[AutoData]
	public void CreateFromDirectory_Overwrite_WithEncoding_ShouldOverwriteFile(
		string contents, Encoding encoding)
	{
		FileSystem.Initialize()
		   .WithSubdirectory("bar").Initialized(s => s
			   .WithFile("test.txt"))
		   .WithSubdirectory("foo").Initialized(s => s
			   .WithFile("test.txt"));
		FileSystem.File.WriteAllText(FileSystem.Path.Combine("foo", "test.txt"),
			contents);

		FileSystem.ZipFile().CreateFromDirectory("foo", "destination.zip",
			CompressionLevel.Optimal, false, encoding);

		IZipArchive archive = FileSystem.ZipFile()
		   .Open("destination.zip", ZipArchiveMode.Read, encoding);

		archive.Entries.Count.Should().Be(1);
		archive.Entries.Should().Contain(e => e.FullName.Equals("test.txt"));
	}
#endif

	[SkippableFact]
	public void CreateFromDirectory_ShouldZipDirectoryContent()
	{
		FileSystem.Initialize()
		   .WithSubdirectory("bar")
		   .WithSubdirectory("foo").Initialized(s => s
			   .WithFile("test.txt"));

		FileSystem.ZipFile().CreateFromDirectory("foo", "destination.zip");

		FileSystem.ZipFile().ExtractToDirectory("destination.zip", "bar");

		FileSystem.File.Exists(FileSystem.Path.Combine("bar", "test.txt"))
		   .Should().BeTrue();
		FileSystem.File.ReadAllBytes(FileSystem.Path.Combine("bar", "test.txt"))
		   .Should().BeEquivalentTo(
				FileSystem.File.ReadAllBytes(FileSystem.Path.Combine("foo", "test.txt")));
	}

	[SkippableTheory]
	[AutoData]
	public void CreateFromDirectory_WithEncoding_ShouldZipDirectoryContent(
		Encoding encoding)
	{
		FileSystem.Initialize()
		   .WithSubdirectory("bar")
		   .WithSubdirectory("foo").Initialized(s => s
			   .WithFile("test.txt"));

		FileSystem.ZipFile().CreateFromDirectory("foo", "destination.zip",
			CompressionLevel.Fastest, false, encoding);

		FileSystem.ZipFile().ExtractToDirectory("destination.zip", "bar", encoding);

		FileSystem.File.Exists(FileSystem.Path.Combine("bar", "test.txt"))
		   .Should().BeTrue();
		FileSystem.File.ReadAllBytes(FileSystem.Path.Combine("bar", "test.txt"))
		   .Should().BeEquivalentTo(
				FileSystem.File.ReadAllBytes(FileSystem.Path.Combine("foo", "test.txt")));
	}

	[SkippableTheory]
	[AutoData]
	public void CreateFromDirectory_WithoutOverwriteAndExistingFile_ShouldOverwriteFile(
		string contents)
	{
		FileSystem.Initialize()
		   .WithSubdirectory("bar").Initialized(s => s
			   .WithFile("test.txt"))
		   .WithSubdirectory("foo").Initialized(s => s
			   .WithFile("test.txt"));
		FileSystem.File.WriteAllText(FileSystem.Path.Combine("foo", "test.txt"),
			contents);
		string destinationPath =
			FileSystem.Path.Combine(FileSystem.Path.GetFullPath("bar"), "test.txt");

		FileSystem.ZipFile().CreateFromDirectory("foo", "destination.zip");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.ZipFile().ExtractToDirectory("destination.zip", "bar");
		});

		exception.Should().BeOfType<IOException>()
		   .Which.Message.Should().Contain($"'{destinationPath}'");
		FileSystem.File.ReadAllText(destinationPath)
		   .Should().NotBe(contents);
	}

	public static IEnumerable<object[]> EntryNameEncoding()
	{
		yield return new object[] { "Dans mes rêves.mp3", Encoding.Default, true };
		yield return new object[] { "Dans mes rêves.mp3", Encoding.ASCII, false };
	}

	[SkippableTheory]
	[AutoData]
	public void FileSystemExtension_ShouldBeSet(
		CompressionLevel compressionLevel)
	{
		IZipFile result = FileSystem.ZipFile();

		result.FileSystem.Should().Be(FileSystem);
	}
}