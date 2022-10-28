using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

public abstract partial class FileSystemFileInfoTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void Create_MissingFile_ShouldCreateFile(string path)
	{
		IFileInfo sut = FileSystem.FileInfo.New(path);

		using FileSystemStream stream = sut.Create();

		FileSystem.File.Exists(path).Should().BeTrue();
	}

	[SkippableTheory]
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