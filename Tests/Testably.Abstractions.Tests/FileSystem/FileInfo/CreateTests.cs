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
		FileSystem.Should().NotHaveFile(path);

		using FileSystemStream stream = sut.Create();

		FileSystem.Should().HaveFile(path);
	}

	[Theory]
	[AutoData]
	public void Create_ShouldRefreshExistsCacheForCurrentItem_ExceptOnNetFramework(string path)
	{
		IFileInfo sut1 = FileSystem.FileInfo.New(path);
		IFileInfo sut2 = FileSystem.FileInfo.New(path);
		IFileInfo sut3 = FileSystem.FileInfo.New(path);
		sut1.Should().NotExist();
		sut2.Should().NotExist();
		// Do not call Exists for `sut3`

		using FileSystemStream stream = sut1.Create();

		if (Test.IsNetFramework)
		{
			sut1.Should().NotExist();
			sut2.Should().NotExist();
			sut3.Should().Exist();
		}
		else
		{
			sut1.Should().Exist();
			sut2.Should().NotExist();
			sut3.Should().Exist();
		}

		FileSystem.Should().HaveFile(path);
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
