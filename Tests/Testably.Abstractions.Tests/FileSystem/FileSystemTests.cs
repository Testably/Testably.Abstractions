namespace Testably.Abstractions.Tests.FileSystem;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class FileSystemTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void Paths_UnderWindows_ShouldUseNormalSlashAndBackslashInterchangeable()
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.Directory.CreateDirectory("foo\\bar");
		FileSystem.File.WriteAllText("foo\\bar\\file.txt", "some content");

		FileSystem.Should().HaveFile("foo/bar/file.txt");
		FileSystem.Directory.GetFiles("foo/bar").Length
			.Should().Be(1);
	}
}
