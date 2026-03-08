using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public class OpenWriteTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task OpenWrite_MissingFile_ShouldCreateFile(string path)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);

		using FileSystemStream stream = sut.OpenWrite();

		await That(FileSystem.File.Exists(path)).IsTrue();
	}

	[Test]
	[AutoArguments]
	public async Task OpenWrite_ShouldUseWriteAccessAndNoneShare(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		using FileSystemStream stream = sut.OpenWrite();

		await That(FileTestHelper.CheckFileAccess(stream)).IsEqualTo(FileAccess.Write);
		await That(FileTestHelper.CheckFileShare(FileSystem, path)).IsEqualTo(FileShare.None);
	}
}
