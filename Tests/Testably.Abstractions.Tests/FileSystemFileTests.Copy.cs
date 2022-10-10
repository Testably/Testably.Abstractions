using System.IO;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [SkippableTheory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.Copy))]
    public void Copy_DestinationExists_ShouldThrowIOExceptionAndNotCopyFile(
        string sourceName,
        string destinationName,
        string sourceContents,
        string destinationContents)
    {
        FileSystem.File.WriteAllText(sourceName, sourceContents);
        FileSystem.File.WriteAllText(destinationName, destinationContents);

        Exception? exception = Record.Exception(() =>
        {
            FileSystem.File.Copy(sourceName, destinationName);
        });

        exception.Should().BeOfType<IOException>();
        FileSystem.File.Exists(sourceName).Should().BeTrue();
        FileSystem.File.ReadAllText(sourceName).Should().Be(sourceContents);
        FileSystem.File.Exists(destinationName).Should().BeTrue();
        FileSystem.File.ReadAllText(destinationName).Should().Be(destinationContents);
    }

#if FEATURE_FILE_MOVETO_OVERWRITE
    [SkippableTheory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.Copy))]
    public void Copy_DestinationExists_WithOverwrite_ShouldOverwriteDestination(
        string sourceName,
        string destinationName,
        string sourceContents,
        string destinationContents)
    {
        FileSystem.File.WriteAllText(sourceName, sourceContents);
        FileSystem.File.WriteAllText(destinationName, destinationContents);

        FileSystem.File.Copy(sourceName, destinationName, true);

        FileSystem.File.Exists(sourceName).Should().BeTrue();
        FileSystem.File.ReadAllText(sourceName).Should().Be(sourceContents);
        FileSystem.File.Exists(destinationName).Should().BeTrue();
        FileSystem.File.ReadAllText(destinationName).Should().Be(sourceContents);
    }
#endif

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.Copy))]
    public void Copy_ReadOnly_ShouldCopyFile(
        string sourceName, string destinationName, string contents)
    {
        FileSystem.File.WriteAllText(sourceName, contents);
        FileSystem.File.SetAttributes(sourceName, FileAttributes.ReadOnly);

        FileSystem.File.Copy(sourceName, destinationName);

        FileSystem.File.Exists(sourceName).Should().BeTrue();
        FileSystem.File.Exists(destinationName).Should().BeTrue();
        FileSystem.File.GetAttributes(destinationName)
           .Should().HaveFlag(FileAttributes.ReadOnly);
        FileSystem.File.ReadAllText(destinationName).Should().Be(contents);
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.Copy))]
    public void Copy_ShouldCopyFileWithContent(
        string sourceName, string destinationName, string contents)
    {
        Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

        FileSystem.File.WriteAllText(sourceName, contents);

        TimeSystem.Thread.Sleep(1000);

        FileSystem.File.Copy(sourceName, destinationName);

        FileSystem.File.GetCreationTime(destinationName)
           .Should().NotBe(FileSystem.File.GetCreationTime(sourceName));
        FileSystem.File.GetLastAccessTime(destinationName)
           .Should().Be(FileSystem.File.GetLastAccessTime(sourceName));
        FileSystem.File.GetLastWriteTime(destinationName)
           .Should().Be(FileSystem.File.GetLastWriteTime(sourceName));
        FileSystem.File.Exists(sourceName).Should().BeTrue();
        FileSystem.File.ReadAllText(sourceName).Should().Be(contents);
        FileSystem.File.Exists(destinationName).Should().BeTrue();
        FileSystem.File.ReadAllText(destinationName).Should().Be(contents);
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.Copy))]
    public void Copy_SourceLocked_ShouldThrowIOException(
        string sourceName,
        string destinationName)
    {
        FileSystem.File.WriteAllText(sourceName, null);
        using FileSystemStream stream = FileSystem.File.Open(sourceName, FileMode.Open,
            FileAccess.Read, FileShare.None);

        Exception? exception = Record.Exception(() =>
        {
            FileSystem.File.Copy(sourceName, destinationName);
        });

        if (Test.RunsOnWindows)
        {
            exception.Should().BeOfType<IOException>();
            FileSystem.File.Exists(destinationName).Should().BeFalse();
        }
        else
        {
            FileSystem.File.Exists(sourceName).Should().BeTrue();
            FileSystem.File.Exists(destinationName).Should().BeTrue();
        }
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.Copy))]
    public void Copy_SourceMissing_ShouldThrowFileNotFoundException(
        string sourceName,
        string destinationName)
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.File.Copy(sourceName, destinationName);
        });

#if NETFRAMEWORK
        exception.Should().BeOfType<FileNotFoundException>();
#else
        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should()
           .Contain($"'{FileSystem.Path.GetFullPath(sourceName)}'");
#endif
        FileSystem.File.Exists(destinationName).Should().BeFalse();
    }
}