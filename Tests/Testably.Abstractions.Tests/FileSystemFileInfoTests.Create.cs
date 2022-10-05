using System.IO;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileInfoTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.Create))]
    public void Create_MissingFile_ShouldCreateFile(string path)
    {
        IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(path);

        using FileSystemStream stream = sut.Create();

        FileSystem.File.Exists(path).Should().BeTrue();
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.Create))]
    public void Create_ShouldUseReadWriteAccessAndNoneShare(string path)
    {
        FileSystem.File.WriteAllText(path, null);
        IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(path);

        using FileSystemStream stream = sut.Create();

        FileTestHelper.CheckFileAccess(stream).Should().Be(FileAccess.ReadWrite);
        FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(FileShare.None);
    }
}