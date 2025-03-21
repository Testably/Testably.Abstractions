namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public partial class ExistsTests
{
	[Theory]
	[AutoData]
	public void Exists_Directory_ShouldReturnFalse(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		sut.Exists.Should().BeFalse();
	}

	[Theory]
	[InlineData("foo", "foo.")]
	[InlineData("foo.", "foo")]
	[InlineData("foo", "foo..")]
	[InlineData("foo..", "foo")]
	public void Exists_ShouldIgnoreTrailingDot_OnWindows(string path1, string path2)
	{
		FileSystem.File.WriteAllText(path1, "some text");
		IFileInfo sut = FileSystem.FileInfo.New(path2);

		sut.Exists.Should().Be(Test.RunsOnWindows);
	}

	[Theory]
	[AutoData]
	public void Exists_ShouldReturnCachedValueUntilRefresh(string path)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);
		sut.Exists.Should().BeFalse();

		FileSystem.File.WriteAllText(path, "some content");

		sut.Exists.Should().BeFalse();

		sut.Refresh();

		sut.Exists.Should().BeTrue();
	}
}
