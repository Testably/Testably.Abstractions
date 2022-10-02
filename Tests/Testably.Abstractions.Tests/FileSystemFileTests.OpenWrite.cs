using System.IO;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.OpenWrite))]
    public void OpenWrite_MissingFile_ShouldCreateFile(string path)
    {
        using FileSystemStream stream = FileSystem.File.OpenWrite(path);

        FileSystem.File.Exists(path).Should().BeTrue();
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.OpenWrite))]
    public void OpenWrite_ShouldUseWriteAccessAndNoneShare(string path)
    {
        FileSystem.File.WriteAllText(path, null);

        using FileSystemStream stream = FileSystem.File.OpenWrite(path);

        FileTestHelper.CheckFileAccess(stream).Should().Be(FileAccess.Write);
        FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(FileShare.None);
    }
}