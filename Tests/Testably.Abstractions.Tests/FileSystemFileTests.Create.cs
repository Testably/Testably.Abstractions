using System.IO;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.Create))]
    public void Create_MissingFile_ShouldCreateFile(string path)
    {
        using FileSystemStream stream = FileSystem.File.Create(path);

        FileSystem.File.Exists(path).Should().BeTrue();
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.Create))]
    public void Create_ShouldUseReadWriteAccessAndNoneShare(string path)
    {
        FileSystem.File.WriteAllText(path, null);

        using FileSystemStream stream = FileSystem.File.Create(path);

        FileTestHelper.CheckFileAccess(stream).Should().Be(FileAccess.ReadWrite);
        FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(FileShare.None);
    }
}