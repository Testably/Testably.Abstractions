namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ExistsTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void Exists_Directory_ShouldReturnFalse(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		sut.Should().NotExist();
	}

	[SkippableTheory]
	[AutoData]
	public void Exists_ShouldReturnCachedValueUntilRefresh(string path)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);
		sut.Should().NotExist();

		FileSystem.File.WriteAllText(path, "some content");

		sut.Should().NotExist();

		sut.Refresh();

		sut.Should().Exist();
	}

	[SkippableTheory]
	[InlineData("foo", "foo.")]
	[InlineData("foo.", "foo")]
	public void Exists_ShouldIgnoreTrailingDot_OnWindows(string path1, string path2)
	{
		FileSystem.File.WriteAllText(path1, "some text");
		IFileInfo sut = FileSystem.FileInfo.New(path2);

		sut.Exists.Should().Be(Test.RunsOnWindows);
	}
}
