using System.IO;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileInfoTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Theory]
    [AutoData]
    [FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.OpenWrite))]
    public void OpenWrite_MissingFile_ShouldCreateFile(string path)
    {
        IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(path);

        using FileSystemStream stream = sut.OpenWrite();

        FileSystem.File.Exists(path).Should().BeTrue();
    }

    [Theory]
    [AutoData]
    [FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.OpenWrite))]
    public void OpenWrite_ShouldUseWriteAccessAndNoneShare(string path)
    {
        FileSystem.File.WriteAllText(path, null);
        IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(path);

        using FileSystemStream stream = sut.OpenWrite();

        FileTestHelper.CheckFileAccess(stream).Should().Be(FileAccess.Write);
        FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(FileShare.None);
    }
}