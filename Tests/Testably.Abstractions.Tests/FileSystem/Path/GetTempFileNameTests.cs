namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public class GetTempFileNameTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	public async Task GetTempFileName_ShouldBeInTempPath()
	{
		string tempPath = FileSystem.Path.GetTempPath();

		#pragma warning disable CS0618 // Type or member is obsolete
		string result = FileSystem.Path.GetTempFileName();
		#pragma warning restore CS0618 // Type or member is obsolete

		await That(result).StartsWith(tempPath);
	}

	[Test]
	public async Task GetTempFileName_ShouldExist()
	{
		#pragma warning disable CS0618 // Type or member is obsolete
		string result = FileSystem.Path.GetTempFileName();
		#pragma warning restore CS0618 // Type or member is obsolete

		await That(FileSystem.File.Exists(result)).IsTrue();
	}
}
