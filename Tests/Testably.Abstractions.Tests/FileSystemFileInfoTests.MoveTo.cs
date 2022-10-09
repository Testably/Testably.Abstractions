using System.IO;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileInfoTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.MoveTo))]
    public void MoveTo_DestinationExists_ShouldThrowIOExceptionAndNotMoveFile(
        string sourceName,
        string destinationName,
        string sourceContents,
        string destinationContents)
    {
        FileSystem.File.WriteAllText(sourceName, sourceContents);
        FileSystem.File.WriteAllText(destinationName, destinationContents);
        IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(sourceName);

        Exception? exception = Record.Exception(() =>
        {
            sut.MoveTo(destinationName);
        });

        exception.Should().BeOfType<IOException>();
        sut.Exists.Should().BeTrue();
        FileSystem.File.Exists(sourceName).Should().BeTrue();
        FileSystem.File.ReadAllText(sourceName).Should().Be(sourceContents);
        FileSystem.File.Exists(destinationName).Should().BeTrue();
        FileSystem.File.ReadAllText(destinationName).Should().Be(destinationContents);
    }

#if FEATURE_FILE_MOVETO_OVERWRITE
    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.MoveTo))]
    public void MoveTo_DestinationExists_WithOverwrite_ShouldOverwriteDestination(
        string sourceName,
        string destinationName,
        string sourceContents,
        string destinationContents)
    {
        FileSystem.File.WriteAllText(sourceName, sourceContents);
        FileSystem.File.WriteAllText(destinationName, destinationContents);
        IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(sourceName);

        sut.MoveTo(destinationName, true);

        sut.Exists.Should().BeTrue();
        sut.ToString().Should().Be(destinationName);
        sut.FullName.Should().Be(FileSystem.Path.GetFullPath(destinationName));
        FileSystem.File.Exists(sourceName).Should().BeFalse();
        FileSystem.File.Exists(destinationName).Should().BeTrue();
        FileSystem.File.ReadAllText(destinationName).Should().Be(sourceContents);
    }
#endif

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.MoveTo))]
    public void MoveTo_ReadOnly_ShouldMoveFile(
        string sourceName, string destinationName, string contents)
    {
        FileSystem.File.WriteAllText(sourceName, contents);
        IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(sourceName);
        sut.IsReadOnly = true;

        sut.MoveTo(destinationName);

        FileSystem.File.Exists(sourceName).Should().BeFalse();
        FileSystem.File.Exists(destinationName).Should().BeTrue();
        FileSystem.File.GetAttributes(destinationName)
           .Should().HaveFlag(FileAttributes.ReadOnly);
        FileSystem.File.ReadAllText(destinationName).Should().Be(contents);
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.MoveTo))]
    public void MoveTo_ShouldMoveFileWithContent(
        string sourceName, string destinationName, string contents)
    {
        FileSystem.File.WriteAllText(sourceName, contents);
        IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(sourceName);

        sut.MoveTo(destinationName);
        
        sut.FullName.Should().Be(FileSystem.Path.GetFullPath(destinationName));
        sut.Exists.Should().BeTrue();
        FileSystem.File.Exists(sourceName).Should().BeFalse();
        FileSystem.File.Exists(destinationName).Should().BeTrue();
        FileSystem.File.ReadAllText(destinationName).Should().Be(contents);
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.MoveTo))]
    public void MoveTo_SourceLocked_ShouldThrowIOException(
        string sourceName,
        string destinationName)
    {
        FileSystem.File.WriteAllText(sourceName, null);
        using FileSystemStream stream = FileSystem.File.Open(sourceName, FileMode.Open,
            FileAccess.Read, FileShare.Read);
        IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(sourceName);

        Exception? exception = Record.Exception(() =>
        {
            sut.MoveTo(destinationName);
        });

        exception.Should().BeOfType<IOException>();
        FileSystem.File.Exists(destinationName).Should().BeFalse();
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.MoveTo))]
    public void MoveTo_SourceMissing_ShouldThrowFileNotFoundException(
        string sourceName,
        string destinationName)
    {
        IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(sourceName);

        Exception? exception = Record.Exception(() =>
        {
            sut.MoveTo(destinationName);
        });

        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should()
           .Contain($"'{FileSystem.Path.GetFullPath(sourceName)}'");
        FileSystem.File.Exists(destinationName).Should().BeFalse();
    }
}