namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public partial class ExistsTests
{
	[Theory]
	[AutoData]
	public async Task Exists_Directory_ShouldReturnFalse(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		await That(sut.Exists).IsFalse();
	}

	[Theory]
	[InlineData("foo", "foo.")]
	[InlineData("foo.", "foo")]
	[InlineData("foo", "foo..")]
	[InlineData("foo..", "foo")]
	public async Task Exists_ShouldIgnoreTrailingDot_OnWindows(string path1, string path2)
	{
		FileSystem.File.WriteAllText(path1, "some text");
		IFileInfo sut = FileSystem.FileInfo.New(path2);

		await That(sut.Exists).IsEqualTo(Test.RunsOnWindows);
	}

	[Theory]
	[AutoData]
	public async Task Exists_ShouldReturnCachedValueUntilRefresh(string path)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);
		await That(sut.Exists).IsFalse();

		FileSystem.File.WriteAllText(path, "some content");

		await That(sut.Exists).IsFalse();

		sut.Refresh();

		await That(sut.Exists).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task Exists_ShouldUpdateAfterFileRecreation(string path)
	{
		// Create initial file
		FileSystem.File.WriteAllText(path, "initial content");
		IFileInfo sut = FileSystem.FileInfo.New(path);
		await That(sut.Exists).IsTrue();

		// Delete the file
		FileSystem.File.Delete(path);
		sut.Refresh();
		await That(sut.Exists).IsFalse();

		// Recreate the file
		FileSystem.File.WriteAllText(path, "new content");
		// Before refresh, should still show cached false value
		await That(sut.Exists).IsFalse();

		// After refresh, should detect the recreated file
		sut.Refresh();
		await That(sut.Exists).IsTrue();
	}
}
