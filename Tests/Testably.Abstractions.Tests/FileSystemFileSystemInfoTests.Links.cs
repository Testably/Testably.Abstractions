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
    public void ResolveLinkTarget_ShouldFollowSymbolicLink(
        string path, string pathToTarget)

    {
        FileSystem.File.WriteAllText(pathToTarget, null);
        IFileSystem.IFileInfo fileInfo = FileSystem.FileInfo.New(path);
        fileInfo.CreateAsSymbolicLink(pathToTarget);
        string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);

        IFileSystem.IFileSystemInfo? target = fileInfo.ResolveLinkTarget(false);

        target!.FullName.Should().Be(targetFullPath);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.FileSystemInfo(
        nameof(IFileSystem.IFileSystemInfo.CreateAsSymbolicLink))]
    public void ResolveLinkTarget_FinalTarget_ShouldFollowSymbolicLinkToFinalTarget(
        string path, string pathToFinalTarget)

    {
        int maxLinks = 40;
        FileSystem.File.WriteAllText(pathToFinalTarget, null);
        string previousPath = pathToFinalTarget;
        for (int i = 0; i < maxLinks; i++)
        {
            string newPath = $"{path}-{i}";
            IFileSystem.IFileInfo linkFileInfo = FileSystem.FileInfo.New(newPath);
            linkFileInfo.CreateAsSymbolicLink(previousPath);
            previousPath = newPath;
        }

        IFileSystem.IFileInfo fileInfo = FileSystem.FileInfo.New(previousPath);

        IFileSystem.IFileSystemInfo? target = fileInfo.ResolveLinkTarget(true);

        target!.FullName.Should().Be(FileSystem.Path.GetFullPath(pathToFinalTarget));
    }

    [Theory]
    [AutoData]
    [FileSystemTests.FileSystemInfo(
        nameof(IFileSystem.IFileSystemInfo.CreateAsSymbolicLink))]
    public void ResolveLinkTarget_FinalTargetWithTooManyLevels_ShouldThrowIOException(
        string path, string pathToFinalTarget)

    {
        int maxLinks = 41;
        FileSystem.File.WriteAllText(pathToFinalTarget, null);
        string previousPath = pathToFinalTarget;
        for (int i = 0; i < maxLinks; i++)
        {
            string newPath = $"{path}-{i}";
            IFileSystem.IFileInfo linkFileInfo = FileSystem.FileInfo.New(newPath);
            linkFileInfo.CreateAsSymbolicLink(previousPath);
            previousPath = newPath;
        }

        IFileSystem.IFileInfo fileInfo = FileSystem.FileInfo.New(previousPath);

        Exception? exception = Record.Exception(() =>
        {
            _ = fileInfo.ResolveLinkTarget(true);
        });

        exception.Should().BeOfType<IOException>()
           .Which.Message.Should().Contain($"'{fileInfo.FullName}'");
    }

    [Theory]
    [AutoData]
    [FileSystemTests.FileSystemInfo(
        nameof(IFileSystem.IFileSystemInfo.CreateAsSymbolicLink))]
    public void ResolveLinkTarget_NormalFile_ShouldReturnNull(
        string path)

    {
        FileSystem.File.WriteAllText(path, null);
        IFileSystem.IFileInfo fileInfo = FileSystem.FileInfo.New(path);

        IFileSystem.IFileSystemInfo? target = fileInfo.ResolveLinkTarget(false);

        target.Should().BeNull();
    }

    [Theory]
    [AutoData]
    [FileSystemTests.FileSystemInfo(
        nameof(IFileSystem.IFileSystemInfo.CreateAsSymbolicLink))]
    public void ResolveLinkTarget_NormalDirectory_ShouldReturnNull(
        string path)

    {
        FileSystem.Directory.CreateDirectory(path);
        IFileSystem.IDirectoryInfo fileInfo = FileSystem.DirectoryInfo.New(path);

        IFileSystem.IFileSystemInfo? target = fileInfo.ResolveLinkTarget(false);

        target.Should().BeNull();
    }
}
#endif