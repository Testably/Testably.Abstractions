#if FEATURE_COMPRESSION_ASYNC
using System.IO.Compression;
using System.Text;
#if FEATURE_COMPRESSION_STREAM
using System.IO;
using Testably.Abstractions.Compression.Tests.TestHelpers;
#endif

namespace Testably.Abstractions.Compression.Tests.ZipFile;

[FileSystemTests]
public partial class CreateFromDirectoryAsyncTests
{
	[Theory]
	[AutoData]
	public async Task
		CreateFromDirectoryAsync_EmptyDirectory_ShouldBeIncluded(
			CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo").Initialized(s => s
				.WithSubdirectory("bar"));

		await FileSystem.ZipFile()
			.CreateFromDirectoryAsync("foo", "destination.zip", compressionLevel, false,
				TestContext.Current.CancellationToken);

		using IZipArchive archive =
			FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

		await That(archive.Entries).HasSingle()
			.Which.For(x => x.FullName, f => f.IsEqualTo("bar/"));
	}

	[Theory]
	[AutoData]
	public async Task CreateFromDirectoryAsync_EmptySource_DoNotIncludeBaseDirectory_ShouldBeEmpty(
		CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");

		await FileSystem.ZipFile()
			.CreateFromDirectoryAsync("foo", "destination.zip", compressionLevel, false,
				TestContext.Current.CancellationToken);

		using IZipArchive archive =
			FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

		await That(archive.Entries).IsEmpty();
	}

	[Theory]
	[AutoData]
	public async Task
		CreateFromDirectoryAsync_EmptySource_IncludeBaseDirectory_ShouldPrependDirectoryName(
			CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");

		await FileSystem.ZipFile()
			.CreateFromDirectoryAsync("foo", "destination.zip", compressionLevel, true,
				TestContext.Current.CancellationToken);

		using IZipArchive archive =
			FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

		await That(archive.Entries).HasSingle()
			.Which.For(x => x.FullName, f => f.IsEqualTo("foo/"));
	}

	[Theory]
	[MemberData(nameof(EntryNameEncoding))]
	public async Task CreateFromDirectoryAsync_EntryNameEncoding_ShouldUseEncoding(
		string entryName, Encoding encoding, bool encodedCorrectly)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile(entryName));

		await FileSystem.ZipFile()
			.CreateFromDirectoryAsync("foo", "destination.zip", CompressionLevel.NoCompression,
				false, encoding, TestContext.Current.CancellationToken);

		using IZipArchive archive =
			FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

		IZipArchiveEntry singleEntry = await That(archive.Entries).HasSingle();
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
	public async Task CreateFromDirectoryAsync_IncludeBaseDirectory_ShouldPrependDirectoryName(
		CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile("test.txt"));

		await FileSystem.ZipFile()
			.CreateFromDirectoryAsync("foo", "destination.zip", compressionLevel, true,
				TestContext.Current.CancellationToken);

		using IZipArchive archive =
			FileSystem.ZipFile().Open("destination.zip", ZipArchiveMode.Read);

		await That(archive.Entries).HasSingle()
			.Which.For(x => x.FullName, f => f.IsEqualTo("foo/test.txt"));
	}

	[Theory]
	[AutoData]
	public async Task CreateFromDirectoryAsync_Overwrite_WithEncoding_ShouldOverwriteFile(
		string contents, Encoding encoding)
	{
		FileSystem.Initialize()
			.WithSubdirectory("bar").Initialized(s => s
				.WithFile("test.txt"))
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile("test.txt"));
		FileSystem.File.WriteAllText(FileSystem.Path.Combine("foo", "test.txt"),
			contents);

		await FileSystem.ZipFile().CreateFromDirectoryAsync("foo", "destination.zip",
			CompressionLevel.Optimal, false, encoding, TestContext.Current.CancellationToken);

		IZipArchive archive = FileSystem.ZipFile()
			.Open("destination.zip", ZipArchiveMode.Read, encoding);

		await That(archive.Entries).HasSingle()
			.Which.For(x => x.FullName, f => f.IsEqualTo("test.txt"));
	}

	[Fact]
	public async Task CreateFromDirectoryAsync_ShouldZipDirectoryContent()
	{
		FileSystem.Initialize()
			.WithSubdirectory("destination")
			.WithSubdirectory("foo").Initialized(s => s
				.WithSubdirectory("bar").Initialized(t => t
					.WithFile("test.txt")));

		await FileSystem.ZipFile().CreateFromDirectoryAsync("foo", "destination.zip",
			TestContext.Current.CancellationToken);

		FileSystem.ZipFile().ExtractToDirectory("destination.zip", "destination");

		await That(FileSystem).HasFile("destination/bar/test.txt")
			.WithContent().SameAs("foo/bar/test.txt");
	}

	[Fact]
	public async Task CreateFromDirectoryAsync_WithReadOnlyStream_ShouldThrowArgumentException()
	{
		FileSystem.Initialize()
			.WithFile("target.zip")
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile("test.txt"));
		using FileSystemStream stream = FileSystem.FileStream.New(
			"target.zip", FileMode.Open, FileAccess.Read);

		async Task Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			await FileSystem.ZipFile().CreateFromDirectoryAsync("foo", stream);
		}

		await That(Act).Throws<ArgumentException>()
			.WithMessage("*stream is unwritable*").AsWildcard().And
			.WithParamName("destination").And
			.WithHResult(-2147024809);
	}

	[Theory]
	[AutoData]
	public async Task
		CreateFromDirectoryAsync_WithStream_EmptyDirectory_ShouldBeIncluded(
			CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo").Initialized(s => s
				.WithSubdirectory("bar"));
		using MemoryStream stream = new();

		await FileSystem.ZipFile()
			.CreateFromDirectoryAsync("foo", stream, compressionLevel, false,
				TestContext.Current.CancellationToken);

		using IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		await That(archive.Entries).HasSingle()
			.Which.For(x => x.FullName, f => f.IsEqualTo("bar/"));
	}

	[Theory]
	[AutoData]
	public async Task
		CreateFromDirectoryAsync_WithStream_EmptySource_DoNotIncludeBaseDirectory_ShouldBeEmpty(
			CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		using MemoryStream stream = new();

		await FileSystem.ZipFile()
			.CreateFromDirectoryAsync("foo", stream, compressionLevel, false,
				TestContext.Current.CancellationToken);

		using IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		await That(archive.Entries).IsEmpty();
	}

	[Theory]
	[AutoData]
	public async Task
		CreateFromDirectoryAsync_WithStream_EmptySource_IncludeBaseDirectory_ShouldPrependDirectoryName(
			CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo");
		using MemoryStream stream = new();

		await FileSystem.ZipFile()
			.CreateFromDirectoryAsync("foo", stream, compressionLevel, true,
				TestContext.Current.CancellationToken);

		using IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		await That(archive.Entries).HasSingle()
			.Which.For(x => x.FullName, f => f.IsEqualTo("foo/"));
	}

	[Theory]
	[MemberData(nameof(EntryNameEncoding))]
	public async Task CreateFromDirectoryAsync_WithStream_EntryNameEncoding_ShouldUseEncoding(
		string entryName, Encoding encoding, bool encodedCorrectly)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile(entryName));
		using MemoryStream stream = new();

		await FileSystem.ZipFile()
			.CreateFromDirectoryAsync("foo", stream, CompressionLevel.NoCompression,
				false, encoding, TestContext.Current.CancellationToken);

		using IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		IZipArchiveEntry singleEntry = await That(archive.Entries).HasSingle();
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
	public async Task
		CreateFromDirectoryAsync_WithStream_IncludeBaseDirectory_ShouldPrependDirectoryName(
			CompressionLevel compressionLevel)
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile("test.txt"));
		using MemoryStream stream = new();

		await FileSystem.ZipFile()
			.CreateFromDirectoryAsync("foo", stream, compressionLevel, true,
				TestContext.Current.CancellationToken);

		using IZipArchive archive = FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read);

		await That(archive.Entries).HasSingle()
			.Which.For(x => x.FullName, f => f.IsEqualTo("foo/test.txt"));
	}

	[Fact]
	public async Task
		CreateFromDirectoryAsync_WithStream_NotWritable_ShouldThrowArgumentException()
	{
		Stream stream = new MemoryStreamMock(canWrite: false);

		async Task Act()
		{
			await FileSystem.ZipFile().CreateFromDirectoryAsync("foo", stream);
		}

		await That(Act).Throws<ArgumentException>()
			.WithMessage("The stream is unwritable*").AsWildcard().And
			.WithParamName("destination").And
			.WithHResult(-2147024809);
	}

	[Fact]
	public async Task
		CreateFromDirectoryAsync_WithStream_Null_ShouldThrowArgumentNullException()
	{
		Stream stream = null!;

		async Task Act()
		{
			await FileSystem.ZipFile().CreateFromDirectoryAsync("foo", stream);
		}

		await That(Act).Throws<ArgumentNullException>()
			.WithParamName("destination");
	}

	[Theory]
	[AutoData]
	public async Task
		CreateFromDirectoryAsync_WithStream_Overwrite_WithEncoding_ShouldOverwriteFile(
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

		await FileSystem.ZipFile().CreateFromDirectoryAsync("foo", stream,
			CompressionLevel.Optimal, false, encoding, TestContext.Current.CancellationToken);

		IZipArchive archive =
			FileSystem.ZipArchive().New(stream, ZipArchiveMode.Read, true, encoding);

		await That(archive.Entries).HasSingle()
			.Which.For(x => x.FullName, f => f.IsEqualTo("test.txt"));
	}

	[Fact]
	public async Task CreateFromDirectoryAsync_WithStream_ShouldZipDirectoryContent()
	{
		FileSystem.Initialize()
			.WithSubdirectory("destination")
			.WithSubdirectory("foo").Initialized(s => s
				.WithSubdirectory("bar").Initialized(t => t
					.WithFile("test.txt")));
		using MemoryStream stream = new();

		await FileSystem.ZipFile()
			.CreateFromDirectoryAsync("foo", stream, TestContext.Current.CancellationToken);

		FileSystem.ZipFile().ExtractToDirectory(stream, "destination");

		await That(FileSystem).HasFile("destination/bar/test.txt")
			.WithContent().SameAs("foo/bar/test.txt");
	}

	[Fact]
	public async Task ShouldNotLock()
	{
		string directory = "ToBeZipped";
		string archive = "zippedDirectory.zip";
		FileSystem.Directory.CreateDirectory(directory);
		FileSystem.File.WriteAllText(FileSystem.Path.Combine(directory, "file.txt"),
			"Some content");
		void Act() => FileSystem.Directory.Delete(directory, true);

		await FileSystem.ZipFile()
			.CreateFromDirectoryAsync(directory, archive, TestContext.Current.CancellationToken);

		await That(Act).DoesNotThrow();
	}

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
#endif
