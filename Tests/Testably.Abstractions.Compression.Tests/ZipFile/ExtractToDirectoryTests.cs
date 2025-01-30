using System.IO;
using System.IO.Compression;
using System.Text;
#if FEATURE_COMPRESSION_STREAM
using Testably.Abstractions.Compression.Tests.TestHelpers;
#endif

namespace Testably.Abstractions.Compression.Tests.ZipFile;

[FileSystemTests]
public partial class ExtractToDirectoryTests
{
	[Fact]
	public async Task ExtractToDirectory_MissingDestinationDirectory_ShouldCreateDirectory()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile("test.txt"));

		FileSystem.ZipFile().CreateFromDirectory("foo", "destination.zip");

		FileSystem.ZipFile().ExtractToDirectory("destination.zip", "bar");

		await That(FileSystem).HasFile("bar/test.txt");
		await That(FileSystem.File.ReadAllBytes("bar/test.txt"))
			.IsEqualTo(FileSystem.File.ReadAllBytes("foo/test.txt"));
	}

	[Fact]
	public async Task
		ExtractToDirectory_MissingSourceFileName_ShouldThrowArgumentNullException()
	{
		FileSystem.Initialize();
		string sourceArchiveFileName = "destination.zip";

		void Act()
		{
			FileSystem.ZipFile().ExtractToDirectory(sourceArchiveFileName, "bar");
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithMessage($"*'{FileSystem.Path.GetFullPath(sourceArchiveFileName)}*").AsWildcard();
	}

	[Fact]
	public async Task
		ExtractToDirectory_NullAsSourceFileName_ShouldThrowArgumentNullException()
	{
		FileSystem.Initialize();
		string sourceArchiveFileName = null!;

		void Act()
		{
			FileSystem.ZipFile().ExtractToDirectory(sourceArchiveFileName, "bar");
		}

		await That(Act).Throws<ArgumentNullException>()
			.WithParamName("sourceArchiveFileName");
	}

#if FEATURE_COMPRESSION_OVERWRITE
	[Theory]
	[AutoData]
	public async Task ExtractToDirectory_Overwrite_ShouldOverwriteFile(
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

		await That(FileSystem).HasFile(FileSystem.Path.Combine("bar", "test.txt"))
			.WithContent(contents);
	}
#endif

#if FEATURE_COMPRESSION_OVERWRITE
	[Theory]
	[AutoData]
	public async Task ExtractToDirectory_WithEncoding_Overwrite_ShouldOverwriteFile(
		string contents,
		Encoding encoding)
	{
		FileSystem.Initialize()
			.WithSubdirectory("bar").Initialized(s => s
				.WithFile("test.txt"))
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile("test.txt"));
		FileSystem.File.WriteAllText(FileSystem.Path.Combine("foo", "test.txt"),
			contents);

		FileSystem.ZipFile().CreateFromDirectory("foo", "destination.zip");

		FileSystem.ZipFile().ExtractToDirectory("destination.zip", "bar", encoding, true);

		await That(FileSystem).HasFile(FileSystem.Path.Combine("bar", "test.txt"))
			.WithContent(contents);
	}
#endif

	[Theory]
	[AutoData]
	public async Task ExtractToDirectory_WithEncoding_ShouldZipDirectoryContent(
		Encoding encoding)
	{
		FileSystem.Initialize()
			.WithSubdirectory("bar")
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile("test.txt"));

		FileSystem.ZipFile().CreateFromDirectory("foo", "destination.zip",
			CompressionLevel.Fastest, false, encoding);

		FileSystem.ZipFile().ExtractToDirectory("destination.zip", "bar", encoding);

		await That(FileSystem).HasFile(FileSystem.Path.Combine("bar", "test.txt"));
		await That(FileSystem.File.ReadAllBytes(FileSystem.Path.Combine("bar", "test.txt")))
			.IsEqualTo(FileSystem.File.ReadAllBytes(FileSystem.Path.Combine("foo", "test.txt")));
	}

	[Theory]
	[AutoData]
	public async Task ExtractToDirectory_WithoutOverwriteAndExistingFile_ShouldOverwriteFile(
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

		void Act()
		{
			FileSystem.ZipFile().ExtractToDirectory("destination.zip", "bar");
		}

		await That(Act).Throws<IOException>()
			.WithMessage($"*'{destinationPath}'*").AsWildcard();
		await That(FileSystem.File.ReadAllText(destinationPath))
			.IsNotEqualTo(contents);
	}

#if FEATURE_COMPRESSION_STREAM
	[Fact]
	public async Task ExtractToDirectory_WithStream_MissingDestinationDirectory_ShouldCreateDirectory()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile("test.txt"));
		using MemoryStream stream = new();

		FileSystem.ZipFile().CreateFromDirectory("foo", stream);

		FileSystem.ZipFile().ExtractToDirectory(stream, "bar");

		await That(FileSystem).HasFile("bar/test.txt");
		await That(FileSystem.File.ReadAllBytes("bar/test.txt"))
			.IsEqualTo(FileSystem.File.ReadAllBytes("foo/test.txt"));
	}
#endif

#if FEATURE_COMPRESSION_STREAM
	[Fact]
	public async Task
		ExtractToDirectory_WithStream_NotReadable_ShouldThrowArgumentNullException()
	{
		FileSystem.Initialize();
		Stream source = new MemoryStreamMock(canRead: false);

		void Act()
		{
			FileSystem.ZipFile().ExtractToDirectory(source, "bar");
		}

		await That(Act).Throws<ArgumentException>()
			.WithMessage("The stream is unreadable*").AsWildcard().And
			.WithParamName("source").And
			.WithHResult(-2147024809);
	}
#endif

#if FEATURE_COMPRESSION_STREAM
	[Fact]
	public async Task
		ExtractToDirectory_WithStream_Null_ShouldThrowArgumentNullException()
	{
		FileSystem.Initialize();
		Stream source = null!;

		void Act()
		{
			FileSystem.ZipFile().ExtractToDirectory(source, "bar");
		}

		await That(Act).Throws<ArgumentNullException>()
			.WithParamName("source");
	}
#endif

#if FEATURE_COMPRESSION_STREAM
	[Theory]
	[AutoData]
	public async Task ExtractToDirectory_WithStream_Overwrite_ShouldOverwriteFile(
		string contents)
	{
		FileSystem.Initialize()
			.WithSubdirectory("bar").Initialized(s => s
				.WithFile("test.txt"))
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile("test.txt"));
		FileSystem.File.WriteAllText(FileSystem.Path.Combine("foo", "test.txt"),
			contents);
		using MemoryStream stream = new();

		FileSystem.ZipFile().CreateFromDirectory("foo", stream);

		FileSystem.ZipFile().ExtractToDirectory(stream, "bar", true);

		await That(FileSystem).HasFile(FileSystem.Path.Combine("bar", "test.txt"))
			.WithContent(contents);
	}
#endif

#if FEATURE_COMPRESSION_STREAM
	[Theory]
	[AutoData]
	public async Task ExtractToDirectory_WithStream_WithEncoding_Overwrite_ShouldOverwriteFile(
		string contents,
		Encoding encoding)
	{
		FileSystem.Initialize()
			.WithSubdirectory("bar").Initialized(s => s
				.WithFile("test.txt"))
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile("test.txt"));
		FileSystem.File.WriteAllText(FileSystem.Path.Combine("foo", "test.txt"),
			contents);
		using MemoryStream stream = new();

		FileSystem.ZipFile().CreateFromDirectory("foo", stream);

		FileSystem.ZipFile().ExtractToDirectory(stream, "bar", encoding, true);

		await That(FileSystem).HasFile(FileSystem.Path.Combine("bar", "test.txt"))
			.WithContent(contents);
	}
#endif

#if FEATURE_COMPRESSION_STREAM
	[Theory]
	[AutoData]
	public async Task ExtractToDirectory_WithStream_WithEncoding_ShouldZipDirectoryContent(
		Encoding encoding)
	{
		FileSystem.Initialize()
			.WithSubdirectory("bar")
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile("test.txt"));
		using MemoryStream stream = new();

		FileSystem.ZipFile().CreateFromDirectory("foo", stream,
			CompressionLevel.Fastest, false, encoding);

		FileSystem.ZipFile().ExtractToDirectory(stream, "bar", encoding);

		await That(FileSystem).HasFile(FileSystem.Path.Combine("bar", "test.txt"));
		await That(FileSystem.File.ReadAllBytes(FileSystem.Path.Combine("bar", "test.txt")))
			.IsEqualTo(FileSystem.File.ReadAllBytes(FileSystem.Path.Combine("foo", "test.txt")));
	}
#endif

#if FEATURE_COMPRESSION_STREAM
	[Theory]
	[AutoData]
	public async Task ExtractToDirectory_WithStream_WithoutOverwriteAndExistingFile_ShouldOverwriteFile(
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
		using MemoryStream stream = new();

		FileSystem.ZipFile().CreateFromDirectory("foo", stream);

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			FileSystem.ZipFile().ExtractToDirectory(stream, "bar");
		}

		await That(Act).Throws<IOException>()
			.WithMessage($"*'{destinationPath}'*").AsWildcard();
		await That(FileSystem.File.ReadAllText(destinationPath))
			.IsNotEqualTo(contents);
	}
#endif

#if FEATURE_COMPRESSION_STREAM
	[Fact]
	public async Task ExtractToDirectory_WithWriteOnlyStream_ShouldThrowArgumentException()
	{
		FileSystem.Initialize()
			.WithFile("target.zip")
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile("test.txt"));
		using FileSystemStream stream = FileSystem.FileStream.New(
			"target.zip", FileMode.Open, FileAccess.Write);

		FileSystem.ZipFile().CreateFromDirectory("foo", stream);

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			FileSystem.ZipFile().ExtractToDirectory(stream, "bar");
		}

		await That(Act).Throws<ArgumentException>()
			.WithMessage("*stream is unreadable*").AsWildcard().And
			.WithParamName("source").And
			.WithHResult(-2147024809);
	}
#endif
}
