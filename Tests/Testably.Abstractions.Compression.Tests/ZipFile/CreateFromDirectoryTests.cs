using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
#if FEATURE_COMPRESSION_STREAM
using System.IO;
using Testably.Abstractions.Compression.Tests.TestHelpers;
#endif

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

#if FEATURE_COMPRESSION_STREAM
	[SkippableTheory]
	[AutoData]
	public void
		CreateFromDirectory_WithStream_EmptyDirectory_ShouldBeIncluded(
			CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo").Initialized(s => s
				.WithSubdirectory("bar"));
		using MemoryStream stream = new();

		FileSystem.ZipFile()
			.CreateFromDirectory("foo", stream, compressionLevel, false);

		using IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		archive.Entries.Count.Should().Be(1);
		archive.Entries.Should().Contain(e => e.FullName.Equals("bar/"));
	}

	[SkippableTheory]
	[AutoData]
	public void CreateFromDirectory_WithStream_EmptySource_DoNotIncludeBaseDirectory_ShouldBeEmpty(
		CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		using MemoryStream stream = new();

		FileSystem.ZipFile()
			.CreateFromDirectory("foo", stream, compressionLevel, false);

		using IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		archive.Entries.Count.Should().Be(0);
	}

	[SkippableTheory]
	[AutoData]
	public void
		CreateFromDirectory_WithStream_EmptySource_IncludeBaseDirectory_ShouldPrependDirectoryName(
			CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		using MemoryStream stream = new();

		FileSystem.ZipFile()
			.CreateFromDirectory("foo", stream, compressionLevel, true);

		using IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		archive.Entries.Count.Should().Be(1);
		archive.Entries.Should().Contain(e => e.FullName.Equals("foo/"));
	}

	[SkippableTheory]
	[MemberData(nameof(EntryNameEncoding))]
	public void CreateFromDirectory_WithStream_EntryNameEncoding_ShouldUseEncoding(
		string entryName, Encoding encoding, bool encodedCorrectly)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile(entryName));
		using MemoryStream stream = new();

		FileSystem.ZipFile()
			.CreateFromDirectory("foo", stream, CompressionLevel.NoCompression,
				false, encoding);

		using IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

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
	public void CreateFromDirectory_WithStream_IncludeBaseDirectory_ShouldPrependDirectoryName(
		CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile("test.txt"));
		using MemoryStream stream = new();

		FileSystem.ZipFile()
			.CreateFromDirectory("foo", stream, compressionLevel, true);

		using IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		archive.Entries.Count.Should().Be(1);
		archive.Entries.Should().Contain(e => e.FullName.Equals("foo/test.txt"));
	}

	[SkippableFact]
	public void
		CreateFromDirectory_WithStream_Null_ShouldThrowArgumentNullException()
	{
		Stream stream = null!;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.ZipFile().CreateFromDirectory("foo", stream);
		});

		exception.Should().BeOfType<ArgumentNullException>()
			.Which.ParamName.Should().Be("destination");
	}

	[SkippableFact]
	public void
		CreateFromDirectory_WithStream_NotWritable_ShouldThrowArgumentException()
	{
		Stream stream = new MemoryStreamMock(canWrite: false);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.ZipFile().CreateFromDirectory("foo", stream);
		});

		exception.Should().BeException<ArgumentException>("The stream is unwritable",
			paramName: "destination", hResult: -2147024809);
	}

	[SkippableTheory]
	[AutoData]
	public void CreateFromDirectory_WithStream_Overwrite_WithEncoding_ShouldOverwriteFile(
		string contents, Encoding encoding)
	{
		FileSystem.Initialize()
			.WithSubdirectory("bar").Initialized(s => s
				.WithFile("test.txt"))
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile("test.txt"));
		FileSystem.File.WriteAllText(FileSystem.Path.Combine("foo", "test.txt"),
			contents);
		using MemoryStream stream = new();

		FileSystem.ZipFile().CreateFromDirectory("foo", stream,
			CompressionLevel.Optimal, false, encoding);

		IZipArchive archive =
			FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read, true, encoding);

		archive.Entries.Count.Should().Be(1);
		archive.Entries.Should().Contain(e => e.FullName.Equals("test.txt"));
	}

	[SkippableFact]
	public void CreateFromDirectory_WithStream_ShouldZipDirectoryContent()
	{
		FileSystem.Initialize()
			.WithSubdirectory("destination")
			.WithSubdirectory("foo").Initialized(s => s
				.WithSubdirectory("bar").Initialized(t => t
					.WithFile("test.txt")));
		using MemoryStream stream = new();

		FileSystem.ZipFile().CreateFromDirectory("foo", stream);

		FileSystem.ZipFile().ExtractToDirectory(stream, "destination");

		FileSystem.File.Exists("destination/bar/test.txt")
			.Should().BeTrue();
		FileSystem.File.ReadAllBytes("destination/bar/test.txt")
			.Should().BeEquivalentTo(
				FileSystem.File.ReadAllBytes("foo/bar/test.txt"));
	}
#endif

	#region Helpers

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

	#endregion
}
