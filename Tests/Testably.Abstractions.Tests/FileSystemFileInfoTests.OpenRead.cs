using System.IO;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileInfoTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.OpenRead))]
    public void OpenRead_MissingFile_ShouldThrowFileNotFoundException(string path)
    {
        IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(path);

        Exception? exception = Record.Exception(() =>
        {
            _ = sut.OpenRead();
        });

        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.OpenRead))]
    public void OpenRead_ShouldUseReadAccessAndReadShare(string path)
    {
        FileSystem.File.WriteAllText(path, null);
        IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(path);

        using FileSystemStream stream = sut.OpenRead();

        FileTestHelper.CheckFileAccess(stream).Should().Be(FileAccess.Read);
        FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(
            Test.RunsOnWindows ? FileShare.Read : FileShare.ReadWrite);
    }
}