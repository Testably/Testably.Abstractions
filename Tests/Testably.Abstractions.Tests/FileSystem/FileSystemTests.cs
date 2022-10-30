namespace Testably.Abstractions.Tests.FileSystem;

public abstract partial class FileSystemTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	protected FileSystemTests(TFileSystem fileSystem, ITimeSystem timeSystem)
		: base(fileSystem, timeSystem)
	{
	}

	[SkippableFact]
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