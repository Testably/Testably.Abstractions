namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public class ExistsTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task Exists_Directory_ShouldReturnFalse(string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		bool result = FileSystem.File.Exists(path);

		await That(result).IsFalse();
	}

	[Test]
	public async Task Exists_Empty_ShouldReturnFalse()
	{
		bool result = FileSystem.File.Exists(string.Empty);

		await That(result).IsFalse();
	}

	[Test]
	public async Task Exists_Null_ShouldReturnFalse()
	{
		bool result = FileSystem.File.Exists(null);

		await That(result).IsFalse();
	}
}
