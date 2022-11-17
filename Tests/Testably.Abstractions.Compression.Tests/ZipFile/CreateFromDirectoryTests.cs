using System.Collections.Generic;
using System.IO.Compression;
using System.Text;

namespace Testably.Abstractions.Compression.Tests.ZipFile;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class CreateFromDirectoryTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void
		CreateFromDirectory_EmptyDirectory_ShouldBeIncluded(
			CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo").Initialized(s => s
				.WithSubdirectory("bar"));

		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", compressionLevel, false);

		using IZipArchive archive =
			FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

		archive.Entries.Count.Should().Be(1);
		archive.Entries.Should().Contain(e => e.FullName.Equals("bar/"));
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
			.WithSubdirectory("destination")
			.WithSubdirectory("foo").Initialized(s => s
				.WithSubdirectory("bar").Initialized(t => t
					.WithFile("test.txt")));

		FileSystem.ZipFile().CreateFromDirectory("foo", "destination.zip");

		FileSystem.ZipFile().ExtractToDirectory("destination.zip", "destination");

		FileSystem.File.Exists("destination/bar/test.txt")
			.Should().BeTrue();
		FileSystem.File.ReadAllBytes("destination/bar/test.txt")
			.Should().BeEquivalentTo(
				FileSystem.File.ReadAllBytes("foo/bar/test.txt"));
	}

	public static IEnumerable<object[]> EntryNameEncoding()
	{
		// ReSharper disable StringLiteralTypo
		yield return new object[]
		{
			"Dans mes rêves.mp3", Encoding.Default, true
		};
		yield return new object[]
		{
			"Dans mes rêves.mp3", Encoding.ASCII, false
		};
		// ReSharper restore StringLiteralTypo
	}
}
