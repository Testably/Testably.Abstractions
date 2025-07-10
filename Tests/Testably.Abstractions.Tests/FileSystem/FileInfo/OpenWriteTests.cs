using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public partial class OpenWriteTests
{
	[Theory]
	[AutoData]
	public async Task OpenWrite_MissingFile_ShouldCreateFile(string path)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);

		using FileSystemStream stream = sut.OpenWrite();

		await That(FileSystem.File.Exists(path)).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task OpenWrite_ShouldUseWriteAccessAndNoneShare(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		using FileSystemStream stream = sut.OpenWrite();

		await That(FileTestHelper.CheckFileAccess(stream)).IsEqualTo(FileAccess.Write);
		await That(FileTestHelper.CheckFileShare(FileSystem, path)).IsEqualTo(FileShare.None);
	}
}
