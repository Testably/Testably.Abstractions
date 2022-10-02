using System.IO;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.Open))]
    public void Open_ExistingFileWithCreateNewMode_ShouldThrowFileNotFoundException(
        string path)
    {
        FileSystem.File.WriteAllText(path, null);

        Exception? exception = Record.Exception(() =>
        {
            _ = FileSystem.File.Open(path, FileMode.CreateNew);
        });

        exception.Should().BeOfType<IOException>()
           .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.Open))]
    public void Open_MissingFileAndIncorrectMode_ShouldThrowFileNotFoundException(
        string path)
    {
        Exception? exception = Record.Exception(() =>
        {
            _ = FileSystem.File.Open(path, FileMode.Open);
        });

        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
    }

    [Theory]
    [InlineAutoData(FileMode.Append, FileAccess.Write)]
    [InlineAutoData(FileMode.Open, FileAccess.ReadWrite)]
    [InlineAutoData(FileMode.Create, FileAccess.ReadWrite)]
    [FileSystemTests.File(nameof(IFileSystem.IFile.Open))]
    public void Open_ShouldUseExpectedAccessDependingOnMode(
        FileMode mode,
        FileAccess expectedAccess,
        string path)
    {
        FileSystem.File.WriteAllText(path, null);

        using FileSystemStream stream = FileSystem.File.Open(path, mode);

        FileTestHelper.CheckFileAccess(stream).Should().Be(expectedAccess);
        FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(FileShare.None);
    }

    [Theory]
    [InlineAutoData(FileAccess.Read, FileShare.Write)]
    [InlineAutoData(FileAccess.Write, FileShare.Read)]
    [FileSystemTests.File(nameof(IFileSystem.IFile.Open))]
    public void Open_ShouldUseGivenAccessAndShare(string path,
                                                  FileAccess access,
                                                  FileShare share)
    {
        FileSystem.File.WriteAllText(path, null);

        using FileSystemStream stream =
            FileSystem.File.Open(path, FileMode.Open, access, share);

        FileTestHelper.CheckFileAccess(stream).Should().Be(access);
        FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(share);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.Open))]
    public void Open_ShouldUseNoneShareAsDefault(string path,
                                                 FileAccess access)
    {
        FileSystem.File.WriteAllText(path, null);

        using FileSystemStream stream = FileSystem.File.Open(path, FileMode.Open, access);

        FileTestHelper.CheckFileAccess(stream).Should().Be(access);
        FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(FileShare.None);
    }
}