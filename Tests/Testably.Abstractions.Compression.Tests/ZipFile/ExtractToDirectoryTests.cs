using System.IO;
using System.IO.Compression;
using System.Text;
#if FEATURE_COMPRESSION_STREAM
using Testably.Abstractions.Compression.Tests.TestHelpers;
#endif

namespace Testably.Abstractions.Compression.Tests.ZipFile;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ExtractToDirectoryTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void ExtractToDirectory_MissingDestinationDirectory_ShouldCreateDirectory()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile("test.txt"));

		FileSystem.ZipFile().CreateFromDirectory("foo", "destination.zip");

		FileSystem.ZipFile().ExtractToDirectory("destination.zip", "bar");

		FileSystem.File.Exists("bar/test.txt")
			.Should().BeTrue();
		FileSystem.File.ReadAllBytes("bar/test.txt")
			.Should().BeEquivalentTo(
				FileSystem.File.ReadAllBytes("foo/test.txt"));
	}

	[SkippableFact]
	public void
		ExtractToDirectory_MissingSourceFileName_ShouldThrowArgumentNullException()
	{
		FileSystem.Initialize();
		string sourceArchiveFileName = "destination.zip";

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.ZipFile().ExtractToDirectory(sourceArchiveFileName, "bar");
		});

		exception.Should().BeOfType<FileNotFoundException>()
			.Which.Message.Should()
			.Contain($"'{FileSystem.Path.GetFullPath(sourceArchiveFileName)}");
	}

	[SkippableFact]
	public void
		ExtractToDirectory_NullAsSourceFileName_ShouldThrowArgumentNullException()
	{
		FileSystem.Initialize();
		string sourceArchiveFileName = null!;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.ZipFile().ExtractToDirectory(sourceArchiveFileName, "bar");
		});

		exception.Should().BeOfType<ArgumentNullException>()
			.Which.ParamName.Should().Be("sourceArchiveFileName");
	}

#if FEATURE_COMPRESSION_OVERWRITE
	[SkippableTheory]
	[AutoData]
	public void ExtractToDirectory_Overwrite_ShouldOverwriteFile(
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

	[SkippableTheory]
	[AutoData]
	public void ExtractToDirectory_WithEncoding_ShouldZipDirectoryContent(
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
	public void ExtractToDirectory_WithoutOverwriteAndExistingFile_ShouldOverwriteFile(
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

#if FEATURE_COMPRESSION_STREAM
	[SkippableFact]
	public void ExtractToDirectory_WithStream_MissingDestinationDirectory_ShouldCreateDirectory()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile("test.txt"));
		using MemoryStream stream = new();

		FileSystem.ZipFile().CreateFromDirectory("foo", stream);

		FileSystem.ZipFile().ExtractToDirectory(stream, "bar");

		FileSystem.File.Exists("bar/test.txt")
			.Should().BeTrue();
		FileSystem.File.ReadAllBytes("bar/test.txt")
			.Should().BeEquivalentTo(
				FileSystem.File.ReadAllBytes("foo/test.txt"));
	}

	[SkippableFact]
	public void
		ExtractToDirectory_WithStream_Null_ShouldThrowArgumentNullException()
	{
		FileSystem.Initialize();
		Stream source = null!;

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.ZipFile().ExtractToDirectory(source, "bar");
		});

		exception.Should().BeOfType<ArgumentNullException>()
			.Which.ParamName.Should().Be("source");
	}

	[SkippableFact]
	public void
		ExtractToDirectory_WithStream_NotReadable_ShouldThrowArgumentNullException()
	{
		FileSystem.Initialize();
		Stream source = new MemoryStreamMock(canRead: false);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.ZipFile().ExtractToDirectory(source, "bar");
		});

		exception.Should().BeException<ArgumentException>(
			"The stream is unreadable", paramName: "source", hResult: -2147024809);
	}

	[SkippableTheory]
	[AutoData]
	public void ExtractToDirectory_WithStream_Overwrite_ShouldOverwriteFile(
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

		FileSystem.File.Exists(FileSystem.Path.Combine("bar", "test.txt"))
			.Should().BeTrue();
		FileSystem.File.ReadAllText(FileSystem.Path.Combine("bar", "test.txt"))
			.Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void ExtractToDirectory_WithStream_WithEncoding_ShouldZipDirectoryContent(
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

		FileSystem.File.Exists(FileSystem.Path.Combine("bar", "test.txt"))
			.Should().BeTrue();
		FileSystem.File.ReadAllBytes(FileSystem.Path.Combine("bar", "test.txt"))
			.Should().BeEquivalentTo(
				FileSystem.File.ReadAllBytes(FileSystem.Path.Combine("foo", "test.txt")));
	}

	[SkippableTheory]
	[AutoData]
	public void ExtractToDirectory_WithStream_WithoutOverwriteAndExistingFile_ShouldOverwriteFile(
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

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.ZipFile().ExtractToDirectory(stream, "bar");
		});

		exception.Should().BeOfType<IOException>()
			.Which.Message.Should().Contain($"'{destinationPath}'");
		FileSystem.File.ReadAllText(destinationPath)
			.Should().NotBe(contents);
	}
#endif
}
