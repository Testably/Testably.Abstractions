#if FEATURE_FILESYSTEM_LINK
using System.IO;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileSystemInfoTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    #region Test Setup

    /// <summary>
    ///     The maximum number of symbolic links that are followed.<br />
    ///     <see href="https://learn.microsoft.com/en-us/dotnet/api/system.io.directory.resolvelinktarget?view=net-6.0#remarks" />
    /// </summary>
    private static int MaxResolveLinks =>
        Test.RunsOnWindows ? 63 : 40;

    #endregion

    [Theory]
    [AutoData]
    [FileSystemTests.FileSystemInfo(
        nameof(IFileSystem.IFileSystemInfo.ResolveLinkTarget))]
    public void ResolveLinkTarget_FileWithDifferentCase_ShouldReturnPathToMissingFile(
        string path, string pathToTarget, string contents)
    {
        string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
        FileSystem.File.WriteAllText(pathToTarget, "foo");
        IFileSystem.IFileInfo fileInfo = FileSystem.FileInfo.New(path);
        fileInfo.CreateAsSymbolicLink(pathToTarget);
        FileSystem.File.Delete(pathToTarget);
        FileSystem.File.WriteAllText(pathToTarget.ToUpper(), contents);

        IFileSystem.IFileSystemInfo? target = fileInfo.ResolveLinkTarget(false);

        target!.FullName.Should().Be(targetFullPath);
        if (Test.RunsOnWindows)
        {
            target.Exists.Should().BeTrue();
            FileSystem.File.ReadAllText(target.FullName).Should().Be(contents);
        }
        else
        {
            target.Exists.Should().BeFalse();
        }
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
    public void ResolveLinkTarget_MissingFileInLinkChain_ShouldReturnPathToMissingFile(
        string path, string pathToFinalTarget, string pathToMissingFile)
    {
        FileSystem.File.WriteAllText(pathToFinalTarget, null);
        IFileSystem.IFileInfo linkFileInfo1 = FileSystem.FileInfo.New(pathToMissingFile);
        linkFileInfo1.CreateAsSymbolicLink(pathToFinalTarget);
        IFileSystem.IFileInfo linkFileInfo2 = FileSystem.FileInfo.New(path);
        linkFileInfo2.CreateAsSymbolicLink(pathToMissingFile);
        linkFileInfo1.Delete();

        IFileSystem.IFileSystemInfo? target = linkFileInfo2.ResolveLinkTarget(true);

        target!.FullName.Should().Be(FileSystem.Path.GetFullPath(pathToMissingFile));
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
    public void ResolveLinkTarget_ShouldFollowSymbolicLink(
        string path, string pathToTarget)
    {
        string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
        FileSystem.File.WriteAllText(pathToTarget, null);
        IFileSystem.IFileInfo fileInfo = FileSystem.FileInfo.New(path);
        fileInfo.CreateAsSymbolicLink(pathToTarget);

        IFileSystem.IFileSystemInfo? target = fileInfo.ResolveLinkTarget(false);

        target!.FullName.Should().Be(targetFullPath);
        target.Exists.Should().BeTrue();
    }

    [Theory]
    [AutoData]
    [FileSystemTests.FileSystemInfo(
        nameof(IFileSystem.IFileSystemInfo.ResolveLinkTarget))]
    public void ResolveLinkTarget_TargetDeletedAfterLinkCreation_ShouldReturnNull(
        string path, string pathToTarget)
    {
        string targetFullPath = FileSystem.Path.GetFullPath(pathToTarget);
        FileSystem.File.WriteAllText(pathToTarget, null);
        IFileSystem.IFileInfo fileInfo = FileSystem.FileInfo.New(path);
        fileInfo.CreateAsSymbolicLink(pathToTarget);
        FileSystem.File.Delete(pathToTarget);

        IFileSystem.IFileSystemInfo? target = fileInfo.ResolveLinkTarget(false);

        target!.FullName.Should().Be(targetFullPath);
        target.Exists.Should().BeFalse();
    }
}
#endif