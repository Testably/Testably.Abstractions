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
		   .Should()
		   .BeEquivalentTo(
				FileSystem.File.ReadAllBytes(
					FileSystem.Path.Combine("foo", "test.txt")));
	}
}