namespace Testably.Abstractions.Tests.FileSystem;

[FileSystemTests]
public partial class FileSystemTests
{
	[Fact]
	public async Task Paths_UnderWindows_ShouldUseNormalSlashAndBackslashInterchangeable()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.Directory.CreateDirectory("foo\\bar");
		FileSystem.File.WriteAllText("foo\\bar\\file.txt", "some content");

		await That(FileSystem.File.Exists("foo/bar/file.txt")).IsTrue();
		await That(FileSystem.Directory.GetFiles("foo/bar")).HasCount(1);
	}
}
