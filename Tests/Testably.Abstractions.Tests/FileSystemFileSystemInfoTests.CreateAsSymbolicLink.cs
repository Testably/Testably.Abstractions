#if FEATURE_FILESYSTEM_LINK
using System.IO;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileSystemInfoTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Theory]
    [AutoData]
    [FileSystemTests.FileSystemInfo(
        nameof(IFileSystem.IFileSystemInfo.CreateAsSymbolicLink))]
    public void CreateAsSymbolicLink_ShouldCreateSymbolicLink(
        string path, string pathToTarget)

    {
        FileSystem.File.WriteAllText(pathToTarget, null);
        IFileSystem.IFileInfo fileInfo = FileSystem.FileInfo.New(path);

        fileInfo.CreateAsSymbolicLink(pathToTarget);

        FileSystem.File.GetAttributes(path)
           .HasFlag(FileAttributes.ReparsePoint)
           .Should().BeTrue();
    }

    [Theory]
    [AutoData]
    [FileSystemTests.FileSystemInfo(
        nameof(IFileSystem.IFileSystemInfo.CreateAsSymbolicLink))]
    public void CreateAsSymbolicLink_SourceFileAlreadyExists_ShouldCreateSymbolicLink(
        string path, string pathToTarget)

    {
        FileSystem.File.WriteAllText(pathToTarget, null);
        FileSystem.File.WriteAllText(path, "foo");
        IFileSystem.IFileInfo fileInfo = FileSystem.FileInfo.New(path);

        Exception? exception = Record.Exception(() =>
        {
            fileInfo.CreateAsSymbolicLink(pathToTarget);
        });

        exception.Should().BeOfType<IOException>()
           .Which.Message.Should().Contain($"'{path}'");
    }
}
#endif