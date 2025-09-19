#if FEATURE_COMPRESSION_ASYNC
using System.IO;
using System.IO.Compression;
using System.Text;
#if FEATURE_COMPRESSION_STREAM
using Testably.Abstractions.Compression.Tests.TestHelpers;
#endif

namespace Testably.Abstractions.Compression.Tests.ZipFile;

[FileSystemTests]
public partial class ExtractToDirectoryAsyncAsyncTests
{
	[Fact]
	public async Task ExtractToDirectoryAsync_MissingDestinationDirectory_ShouldCreateDirectory()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile("test.txt"));

		FileSystem.ZipFile().CreateFromDirectory("foo", "destination.zip");

		await FileSystem.ZipFile().ExtractToDirectoryAsync("destination.zip", "bar",
			TestContext.Current.CancellationToken);

		await That(FileSystem).HasFile("bar/test.txt")
			.WithContent().SameAs("foo/test.txt");
	}

	[Fact]
	public async Task
		ExtractToDirectoryAsync_MissingSourceFileName_ShouldThrowArgumentNullException()
	{
		FileSystem.Initialize();
		string sourceArchiveFileName = "destination.zip";

		async Task Act()
		{
			await FileSystem.ZipFile().ExtractToDirectoryAsync(sourceArchiveFileName, "bar",
				TestContext.Current.CancellationToken);
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithMessage($"*'{FileSystem.Path.GetFullPath(sourceArchiveFileName)}*").AsWildcard();
	}

	[Fact]
	public async Task
		ExtractToDirectoryAsync_NullAsSourceFileName_ShouldThrowArgumentNullException()
	{
		FileSystem.Initialize();
		string sourceArchiveFileName = null!;

		async Task Act()
		{
			await FileSystem.ZipFile().ExtractToDirectoryAsync(sourceArchiveFileName, "bar", TestContext.Current.CancellationToken);
		}

		await That(Act).Throws<ArgumentNullException>()
			.WithParamName("sourceArchiveFileName");
	}

	[Theory]
	[AutoData]
	public async Task ExtractToDirectoryAsync_Overwrite_ShouldOverwriteFile(
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

		await FileSystem.ZipFile().ExtractToDirectoryAsync("destination.zip", "bar", true,
			TestContext.Current.CancellationToken);

		await That(FileSystem).HasFile(FileSystem.Path.Combine("bar", "test.txt"))
			.WithContent(contents);
	}

	[Theory]
	[AutoData]
	public async Task ExtractToDirectoryAsync_WithEncoding_Overwrite_ShouldOverwriteFile(
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

		await FileSystem.ZipFile().ExtractToDirectoryAsync("destination.zip", "bar", encoding, true,
			TestContext.Current.CancellationToken);

		await That(FileSystem).HasFile(FileSystem.Path.Combine("bar", "test.txt"))
			.WithContent(contents);
	}

	[Theory]
	[AutoData]
	public async Task ExtractToDirectoryAsync_WithEncoding_ShouldZipDirectoryContent(
		Encoding encoding)
	{
		FileSystem.Initialize()
			.WithSubdirectory("bar")
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile("test.txt"));

		FileSystem.ZipFile().CreateFromDirectory("foo", "destination.zip",
			CompressionLevel.Fastest, false, encoding);

		await FileSystem.ZipFile().ExtractToDirectoryAsync("destination.zip", "bar", encoding,
			TestContext.Current.CancellationToken);

		await That(FileSystem).HasFile(FileSystem.Path.Combine("bar", "test.txt"))
			.WithContent().SameAs(FileSystem.Path.Combine("foo", "test.txt"));
	}

	[Theory]
	[AutoData]
	public async Task ExtractToDirectoryAsync_WithoutOverwriteAndExistingFile_ShouldOverwriteFile(
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

		async Task Act()
		{
			await FileSystem.ZipFile().ExtractToDirectoryAsync("destination.zip", "bar", TestContext.Current.CancellationToken);
		}

		await That(Act).Throws<IOException>()
			.WithMessage($"*'{destinationPath}'*").AsWildcard();
		await That(FileSystem.File.ReadAllText(destinationPath))
			.IsNotEqualTo(contents);
	}

	[Fact]
	public async Task
		ExtractToDirectoryAsync_WithStream_MissingDestinationDirectory_ShouldCreateDirectory()
	{
		FileSystem.Initialize()
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile("test.txt"));
		using MemoryStream stream = new();

		FileSystem.ZipFile().CreateFromDirectory("foo", stream);

		await FileSystem.ZipFile()
			.ExtractToDirectoryAsync(stream, "bar", TestContext.Current.CancellationToken);

		await That(FileSystem).HasFile("bar/test.txt")
			.WithContent().SameAs("foo/test.txt");
	}

	[Fact]
	public async Task
		ExtractToDirectoryAsync_WithStream_NotReadable_ShouldThrowArgumentNullException()
	{
		FileSystem.Initialize();
		Stream source = new MemoryStreamMock(canRead: false);

		async Task Act()
		{
			await FileSystem.ZipFile().ExtractToDirectoryAsync(source, "bar", TestContext.Current.CancellationToken);
		}

		await That(Act).Throws<ArgumentException>()
			.WithMessage("The stream is unreadable*").AsWildcard().And
			.WithParamName("source").And
			.WithHResult(-2147024809);
	}

	[Fact]
	public async Task
		ExtractToDirectoryAsync_WithStream_Null_ShouldThrowArgumentNullException()
	{
		FileSystem.Initialize();
		Stream source = null!;

		async Task Act()
		{
			await FileSystem.ZipFile().ExtractToDirectoryAsync(source, "bar", TestContext.Current.CancellationToken);
		}

		await That(Act).Throws<ArgumentNullException>()
			.WithParamName("source");
	}

	[Theory]
	[AutoData]
	public async Task ExtractToDirectoryAsync_WithStream_Overwrite_ShouldOverwriteFile(
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

		await FileSystem.ZipFile()
			.ExtractToDirectoryAsync(stream, "bar", true, TestContext.Current.CancellationToken);

		await That(FileSystem).HasFile(FileSystem.Path.Combine("bar", "test.txt"))
			.WithContent(contents);
	}

	[Theory]
	[AutoData]
	public async Task ExtractToDirectoryAsync_WithStream_WithEncoding_Overwrite_ShouldOverwriteFile(
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

		await FileSystem.ZipFile().ExtractToDirectoryAsync(stream, "bar", encoding, true,
			TestContext.Current.CancellationToken);

		await That(FileSystem).HasFile(FileSystem.Path.Combine("bar", "test.txt"))
			.WithContent(contents);
	}

	[Theory]
	[AutoData]
	public async Task ExtractToDirectoryAsync_WithStream_WithEncoding_ShouldZipDirectoryContent(
		Encoding encoding)
	{
		FileSystem.Initialize()
			.WithSubdirectory("bar")
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile("test.txt"));
		using MemoryStream stream = new();

		FileSystem.ZipFile().CreateFromDirectory("foo", stream,
			CompressionLevel.Fastest, false, encoding);

		await FileSystem.ZipFile().ExtractToDirectoryAsync(stream, "bar", encoding,
			TestContext.Current.CancellationToken);

		await That(FileSystem).HasFile(FileSystem.Path.Combine("bar", "test.txt"))
			.WithContent().SameAs(FileSystem.Path.Combine("foo", "test.txt"));
	}

	[Theory]
	[AutoData]
	public async Task
		ExtractToDirectoryAsync_WithStream_WithoutOverwriteAndExistingFile_ShouldOverwriteFile(
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

		async Task Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			await FileSystem.ZipFile().ExtractToDirectoryAsync(stream, "bar", TestContext.Current.CancellationToken);
		}

		await That(Act).Throws<IOException>()
			.WithMessage($"*'{destinationPath}'*").AsWildcard();
		await That(FileSystem.File.ReadAllText(destinationPath))
			.IsNotEqualTo(contents);
	}

	[Fact]
	public async Task ExtractToDirectoryAsync_WithWriteOnlyStream_ShouldThrowArgumentException()
	{
		FileSystem.Initialize()
			.WithFile("target.zip")
			.WithSubdirectory("foo").Initialized(s => s
				.WithFile("test.txt"));
		using FileSystemStream stream = FileSystem.FileStream.New(
			"target.zip", FileMode.Open, FileAccess.Write);

		FileSystem.ZipFile().CreateFromDirectory("foo", stream);

		async Task Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			await FileSystem.ZipFile().ExtractToDirectoryAsync(stream, "bar", TestContext.Current.CancellationToken);
		}

		await That(Act).Throws<ArgumentException>()
			.WithMessage("*stream is unreadable*").AsWildcard().And
			.WithParamName("source").And
			.WithHResult(-2147024809);
	}
}
#endif
