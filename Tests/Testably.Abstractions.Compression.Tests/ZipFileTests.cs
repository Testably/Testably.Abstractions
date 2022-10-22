using System.IO;

namespace Testably.Abstractions.Compression.Tests;

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
}