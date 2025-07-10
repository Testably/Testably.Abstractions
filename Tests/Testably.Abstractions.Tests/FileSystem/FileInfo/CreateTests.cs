using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public partial class CreateTests
{
	[Theory]
	[AutoData]
	public async Task Create_MissingFile_ShouldCreateFile(string path)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);
		await That(FileSystem.File.Exists(path)).IsFalse();

		using FileSystemStream stream = sut.Create();

		await That(FileSystem.File.Exists(path)).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task Create_ShouldRefreshExistsCacheForCurrentItem_ExceptOnNetFramework(
		string path)
	{
		IFileInfo sut1 = FileSystem.FileInfo.New(path);
		IFileInfo sut2 = FileSystem.FileInfo.New(path);
		IFileInfo sut3 = FileSystem.FileInfo.New(path);
		await That(sut1.Exists).IsFalse();
		await That(sut2.Exists).IsFalse();
		// Do not call Exists for `sut3`

		using FileSystemStream stream = sut1.Create();

		if (Test.IsNetFramework)
		{
			await That(sut1.Exists).IsFalse();
			await That(sut2.Exists).IsFalse();
			await That(sut3.Exists).IsTrue();
		}
		else
		{
			await That(sut1.Exists).IsTrue();
			await That(sut2.Exists).IsFalse();
			await That(sut3.Exists).IsTrue();
		}

		await That(FileSystem.File.Exists(path)).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task Create_ShouldUseReadWriteAccessAndNoneShare(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		using FileSystemStream stream = sut.Create();

		await That(FileTestHelper.CheckFileAccess(stream)).IsEqualTo(FileAccess.ReadWrite);
		await That(FileTestHelper.CheckFileShare(FileSystem, path)).IsEqualTo(FileShare.None);
	}
}
