using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public partial class CreateTests
{
	[Theory]
	[AutoData]
	public void Create_MissingFile_ShouldCreateFile(string path)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);
		FileSystem.File.Exists(path).Should().BeFalse();

		using FileSystemStream stream = sut.Create();

		FileSystem.File.Exists(path).Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public async Task Create_ShouldRefreshExistsCacheForCurrentItem_ExceptOnNetFramework(string path)
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

		FileSystem.File.Exists(path).Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void Create_ShouldUseReadWriteAccessAndNoneShare(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		using FileSystemStream stream = sut.Create();

		FileTestHelper.CheckFileAccess(stream).Should().Be(FileAccess.ReadWrite);
		FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(FileShare.None);
	}
}
