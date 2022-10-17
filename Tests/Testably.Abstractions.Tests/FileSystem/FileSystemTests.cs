namespace Testably.Abstractions.Tests.FileSystem;

public abstract partial class FileSystemTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	#region Test Setup

	public abstract string BasePath { get; }
	public TFileSystem FileSystem { get; }
	public ITimeSystem TimeSystem { get; }

	protected FileSystemTests(TFileSystem fileSystem,
	                          ITimeSystem timeSystem)
	{
		FileSystem = fileSystem;
		TimeSystem = timeSystem;

		Test.SkipIfTestsOnRealFileSystemShouldBeSkipped(FileSystem);
	}

	#endregion

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