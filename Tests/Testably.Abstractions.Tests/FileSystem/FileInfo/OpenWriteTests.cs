using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public partial class OpenWriteTests
{
	[Theory]
	[AutoData]
	public void OpenWrite_MissingFile_ShouldCreateFile(string path)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);

		using FileSystemStream stream = sut.OpenWrite();

		FileSystem.File.Exists(path).Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void OpenWrite_ShouldUseWriteAccessAndNoneShare(string path)
	{
		FileSystem.File.WriteAllText(path, null);
		IFileInfo sut = FileSystem.FileInfo.New(path);

		using FileSystemStream stream = sut.OpenWrite();

		FileTestHelper.CheckFileAccess(stream).Should().Be(FileAccess.Write);
		FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(FileShare.None);
	}
}
