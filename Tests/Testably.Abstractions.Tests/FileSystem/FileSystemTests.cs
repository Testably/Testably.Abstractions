namespace Testably.Abstractions.Tests.FileSystem;

[FileSystemTests]
public partial class FileSystemTests
{
	[Fact]
	public void Paths_UnderWindows_ShouldUseNormalSlashAndBackslashInterchangeable()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.Directory.CreateDirectory("foo\\bar");
		FileSystem.File.WriteAllText("foo\\bar\\file.txt", "some content");

		FileSystem.File.Exists("foo/bar/file.txt").Should().BeTrue();
		FileSystem.Directory.GetFiles("foo/bar").Length
			.Should().Be(1);
	}
}
