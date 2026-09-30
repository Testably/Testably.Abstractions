namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public class RefreshTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	private static readonly DateTime OtherTime = new(2002, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	[Test]
	[AutoArguments]
	public async Task CreateSubdirectory_ShouldNotResetCachedState(string path, string subdirectory)
	{
		FileSystem.Directory.CreateDirectory(path);
		FileSystem.Directory.SetLastWriteTimeUtc(path, OtherTime);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		await That(sut.Exists).IsTrue();

		sut.CreateSubdirectory(subdirectory);

		await That(sut.LastWriteTimeUtc).IsEqualTo(OtherTime);
	}

	[Test]
	[AutoArguments]
	public async Task Exists_ShouldReturnCachedValueFromOtherProperty(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		_ = sut.Attributes;

		FileSystem.Directory.Delete(path);

		await That(sut.Exists).IsTrue();
	}

	[Test]
	[AutoArguments]
	public async Task LastWriteTime_Set_ShouldResetCachedState(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		await That(sut.LastWriteTimeUtc).IsNotEqualTo(OtherTime);

		sut.LastWriteTimeUtc = OtherTime;

		await That(sut.LastWriteTimeUtc).IsEqualTo(OtherTime);
	}

	[Test]
	[AutoArguments]
	public async Task LastWriteTime_ShouldReturnCachedValueUntilRefresh(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		await That(sut.Exists).IsTrue();

		FileSystem.Directory.SetLastWriteTimeUtc(path, OtherTime);

		await That(sut.LastWriteTimeUtc).IsNotEqualTo(OtherTime);
		sut.Refresh();
		await That(sut.LastWriteTimeUtc).IsEqualTo(OtherTime);
	}

	[Test]
	[AutoArguments]
	public async Task MoveTo_ShouldResetCachedState(string path, string destination)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);
		await That(sut.Exists).IsTrue();
		FileSystem.Directory.SetLastWriteTimeUtc(path, OtherTime);

		sut.MoveTo(destination);

		await That(sut.LastWriteTimeUtc).IsEqualTo(OtherTime);
	}
}
