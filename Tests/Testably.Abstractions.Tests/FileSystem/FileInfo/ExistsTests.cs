namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public class ExistsTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task Exists_Directory_ShouldReturnFalse(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		await That(sut.Exists).IsFalse();
	}

	[Test]
	[Arguments("foo", "foo.")]
	[Arguments("foo.", "foo")]
	[Arguments("foo", "foo..")]
	[Arguments("foo..", "foo")]
	public async Task Exists_ShouldIgnoreTrailingDot_OnWindows(string path1, string path2)
	{
		FileSystem.File.WriteAllText(path1, "some text");
		IFileInfo sut = FileSystem.FileInfo.New(path2);

		await That(sut.Exists).IsEqualTo(Test.RunsOnWindows);
	}

	[Test]
	[AutoArguments]
	public async Task Exists_ShouldReturnCachedValueUntilRefresh(string path)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);
		await That(sut.Exists).IsFalse();

		FileSystem.File.WriteAllText(path, "some content");

		await That(sut.Exists).IsFalse();

		sut.Refresh();

		await That(sut.Exists).IsTrue();
	}
}
