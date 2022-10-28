namespace Testably.Abstractions.Testing.Tests.FileSystem;

public class FileMockTests
{
	[Theory]
	[AutoData]
	public void SetCreationTime(string path, DateTime creationTime)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText(path, "some content");

		fileSystem.File.SetCreationTime(path, creationTime);

		fileSystem.File.GetCreationTime(path).Should().Be(creationTime);
	}

	[Theory]
	[AutoData]
	public void SetCreationTimeUtc(string path, DateTime creationTime)
	{
		MockFileSystem fileSystem = new();
		fileSystem.File.WriteAllText(path, "some content");

		fileSystem.File.SetCreationTimeUtc(path, creationTime);

		fileSystem.File.GetCreationTimeUtc(path).Should().Be(creationTime);
	}
}