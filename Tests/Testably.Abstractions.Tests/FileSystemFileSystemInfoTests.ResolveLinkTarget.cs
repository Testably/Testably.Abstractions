#if FEATURE_FILESYSTEM_LINK
using System.IO;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileSystemInfoTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    //TODO Remove file in link chain
    //TODO Delete target file and create it new with different case

    /// <summary>
    ///     The maximum number of symbolic links that are followed.<br />
    ///     <see href="https://learn.microsoft.com/en-us/dotnet/api/system.io.directory.resolvelinktarget?view=net-6.0#remarks" />
    /// </summary>
    private static int MaxResolveLinks =>
        Test.RunsOnWindows ? 63 : 40;

    [Theory]
    [AutoData]
    [FileSystemTests.FileSystemInfo(
        nameof(IFileSystem.IFileSystemInfo.ResolveLinkTarget))]
    public void ResolveLinkTarget_ShouldFollowSymbolicLink(
        string path, string pathToTarget)

    {
        string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
        FileSystem.File.WriteAllText(pathToTarget, null);
        IFileSystem.IFileInfo fileInfo = FileSystem.FileInfo.New(path);
        fileInfo.CreateAsSymbolicLink(pathToTarget);

        IFileSystem.IFileSystemInfo? target = fileInfo.ResolveLinkTarget(false);

        target!.FullName.Should().Be(targetFullPath);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.FileSystemInfo(
        nameof(IFileSystem.IFileSystemInfo.ResolveLinkTarget))]
    public void ResolveLinkTarget_FinalTarget_ShouldFollowSymbolicLinkToFinalTarget(
        string path, string pathToFinalTarget)

    {
        int maxLinks = MaxResolveLinks;

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
        nameof(IFileSystem.IFileSystemInfo.ResolveLinkTarget))]
    public void ResolveLinkTarget_FinalTargetWithTooManyLevels_ShouldThrowIOException(
        string path, string pathToFinalTarget)

    {
        int maxLinks = MaxResolveLinks + 1;
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
        nameof(IFileSystem.IFileSystemInfo.ResolveLinkTarget))]
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
        nameof(IFileSystem.IFileSystemInfo.ResolveLinkTarget))]
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