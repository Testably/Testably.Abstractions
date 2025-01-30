using System.IO.Compression;
using System.Text;
#if FEATURE_COMPRESSION_STREAM
using System.IO;
using Testably.Abstractions.Compression.Tests.TestHelpers;
#endif

namespace Testably.Abstractions.Compression.Tests.ZipFile;

[FileSystemTests]
public partial class CreateFromDirectoryTests
{
	[Theory]
	[AutoData]
	public async Task
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

		await That(archive.Entries).HasSingle()
			.Which.For(x => x.FullName, f => f.IsEqualTo("bar/"));
	}

	[Theory]
	[AutoData]
	public async Task CreateFromDirectory_EmptySource_DoNotIncludeBaseDirectory_ShouldBeEmpty(
		CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");

		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", compressionLevel, false);

		using IZipArchive archive =
			FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

		await That(archive.Entries).IsEmpty();
	}

	[Theory]
	[AutoData]
	public async Task
		CreateFromDirectory_EmptySource_IncludeBaseDirectory_ShouldPrependDirectoryName(
			CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");

		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", compressionLevel, true);

		using IZipArchive archive =
			FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

		await That(archive.Entries).HasSingle()
			.Which.For(x => x.FullName, f => f.IsEqualTo("foo/"));
	}

	[Theory]
	[MemberData(nameof(EntryNameEncoding))]
	public async Task CreateFromDirectory_EntryNameEncoding_ShouldUseEncoding(
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

		var singleEntry = await That(archive.Entries).HasSingle();
		if (encodedCorrectly)
		{
			await That(singleEntry.Name).IsEqualTo(entryName);
		}
		else
		{
			await That(singleEntry.Name).IsNotEqualTo(entryName);
		}
	}

	[Theory]
	[AutoData]
	public async Task CreateFromDirectory_IncludeBaseDirectory_ShouldPrependDirectoryName(
		CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile("test.txt"));

		FileSystem.ZipFile()
			.CreateFromDirectory("foo", "destination.zip", compressionLevel, true);

		using IZipArchive archive =
			FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

		await That(archive.Entries).HasSingle()
			.Which.For(x => x.FullName, f => f.IsEqualTo("foo/test.txt"));
	}

#if FEATURE_COMPRESSION_OVERWRITE
	[Theory]
	[AutoData]
	public async Task CreateFromDirectory_Overwrite_WithEncoding_ShouldOverwriteFile(
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

		await That(archive.Entries).HasSingle()
			.Which.For(x => x.FullName, f => f.IsEqualTo("test.txt"));
	}
#endif

	[Fact]
	public async Task CreateFromDirectory_ShouldZipDirectoryContent()
	{
		FileSystem.Initialize()
			.WithSubdirectory("destination")
			.WithSubdirectory("foo").Initialized(s => s
				.WithSubdirectory("bar").Initialized(t => t
					.WithFile("test.txt")));

		FileSystem.ZipFile().CreateFromDirectory("foo", "destination.zip");

		FileSystem.ZipFile().ExtractToDirectory("destination.zip", "destination");

		await That(FileSystem).HasFile("destination/bar/test.txt");
		await That(FileSystem.File.ReadAllBytes("destination/bar/test.txt"))
			.IsEqualTo(FileSystem.File.ReadAllBytes("foo/bar/test.txt"));
	}

#if FEATURE_COMPRESSION_STREAM
	[Fact]
	public async Task CreateFromDirectory_WithReadOnlyStream_ShouldThrowArgumentException()
	{
		FileSystem.Initialize()
			.WithFile("target.zip")
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile("test.txt"));
		using FileSystemStream stream = FileSystem.FileStream.New(
			"target.zip", FileMode.Open, FileAccess.Read);

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			FileSystem.ZipFile().CreateFromDirectory("foo", stream);
		}

		await That(Act).Throws<ArgumentException>()
			.WithMessage("*stream is unwritable*").AsWildcard().And
			.WithParamName("destination").And
			.WithHResult(-2147024809);
	}
#endif

#if FEATURE_COMPRESSION_STREAM
	[Theory]
	[AutoData]
	public async Task
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

		await That(archive.Entries).HasSingle()
			.Which.For(x => x.FullName, f => f.IsEqualTo("bar/"));
	}
#endif

#if FEATURE_COMPRESSION_STREAM
	[Theory]
	[AutoData]
	public async Task CreateFromDirectory_WithStream_EmptySource_DoNotIncludeBaseDirectory_ShouldBeEmpty(
		CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		using MemoryStream stream = new();

		FileSystem.ZipFile()
			.CreateFromDirectory("foo", stream, compressionLevel, false);

		using IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		await That(archive.Entries).IsEmpty();
	}
#endif

#if FEATURE_COMPRESSION_STREAM
	[Theory]
	[AutoData]
	public async Task
		CreateFromDirectory_WithStream_EmptySource_IncludeBaseDirectory_ShouldPrependDirectoryName(
			CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		using MemoryStream stream = new();

		FileSystem.ZipFile()
			.CreateFromDirectory("foo", stream, compressionLevel, true);

		using IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		await That(archive.Entries).HasSingle()
			.Which.For(x => x.FullName, f => f.IsEqualTo("foo/"));
	}
#endif

#if FEATURE_COMPRESSION_STREAM
	[Theory]
	[MemberData(nameof(EntryNameEncoding))]
	public async Task CreateFromDirectory_WithStream_EntryNameEncoding_ShouldUseEncoding(
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

		var singleEntry = await That(archive.Entries).HasSingle();
		if (encodedCorrectly)
		{
			await That(singleEntry.Name).IsEqualTo(entryName);
		}
		else
		{
			await That(singleEntry.Name).IsNotEqualTo(entryName);
		}
	}
#endif

#if FEATURE_COMPRESSION_STREAM
	[Theory]
	[AutoData]
	public async Task CreateFromDirectory_WithStream_IncludeBaseDirectory_ShouldPrependDirectoryName(
		CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile("test.txt"));
		using MemoryStream stream = new();

		FileSystem.ZipFile()
			.CreateFromDirectory("foo", stream, compressionLevel, true);

		using IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		await That(archive.Entries).HasSingle()
			.Which.For(x => x.FullName, f => f.IsEqualTo("foo/test.txt"));
	}
#endif

#if FEATURE_COMPRESSION_STREAM
	[Fact]
	public async Task
		CreateFromDirectory_WithStream_NotWritable_ShouldThrowArgumentException()
	{
		Stream stream = new MemoryStreamMock(canWrite: false);

		void Act()
		{
			FileSystem.ZipFile().CreateFromDirectory("foo", stream);
		}

		await That(Act).Throws<ArgumentException>()
			.WithMessage("The stream is unwritable*").AsWildcard().And
			.WithParamName("destination").And
			.WithHResult(-2147024809);
	}
#endif

#if FEATURE_COMPRESSION_STREAM
	[Fact]
	public async Task
		CreateFromDirectory_WithStream_Null_ShouldThrowArgumentNullException()
	{
		Stream stream = null!;

		void Act()
		{
			FileSystem.ZipFile().CreateFromDirectory("foo", stream);
		}

		await That(Act).Throws<ArgumentNullException>()
			.WithParamName("destination");
	}
#endif

#if FEATURE_COMPRESSION_STREAM
	[Theory]
	[AutoData]
	public async Task CreateFromDirectory_WithStream_Overwrite_WithEncoding_ShouldOverwriteFile(
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

		await That(archive.Entries).HasSingle()
			.Which.For(x => x.FullName, f => f.IsEqualTo("test.txt"));
	}
#endif

#if FEATURE_COMPRESSION_STREAM
	[Fact]
	public async Task CreateFromDirectory_WithStream_ShouldZipDirectoryContent()
	{
		FileSystem.Initialize()
			.WithSubdirectory("destination")
			.WithSubdirectory("foo").Initialized(s => s
				.WithSubdirectory("bar").Initialized(t => t
					.WithFile("test.txt")));
		using MemoryStream stream = new();

		FileSystem.ZipFile().CreateFromDirectory("foo", stream);

		FileSystem.ZipFile().ExtractToDirectory(stream, "destination");

		await That(FileSystem).HasFile("destination/bar/test.txt");
		await That(FileSystem.File.ReadAllBytes("destination/bar/test.txt"))
			.IsEqualTo(FileSystem.File.ReadAllBytes("foo/bar/test.txt"));
	}
#endif

	#region Helpers

	#pragma warning disable MA0018
	public static TheoryData<string, Encoding, bool> EntryNameEncoding()
	{
		// ReSharper disable StringLiteralTypo
		TheoryData<string, Encoding, bool> theoryData = new()
		{
			{
				"Dans mes rêves.mp3", Encoding.Default, true
			},
			{
				"Dans mes rêves.mp3", Encoding.ASCII, false
			},
		};
		// ReSharper restore StringLiteralTypo
		return theoryData;
	}
	#pragma warning restore MA0018

	#endregion
}
