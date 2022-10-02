using System.IO;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileInfoTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Theory]
    [AutoData]
    [FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.Open))]
    public void Open_ExistingFileWithCreateNewMode_ShouldThrowFileNotFoundException(
        string path)
    {
        FileSystem.File.WriteAllText(path, null);
        IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(path);

        Exception? exception = Record.Exception(() =>
        {
            _ = sut.Open(FileMode.CreateNew);
        });

        exception.Should().BeOfType<IOException>()
           .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
    }

    [Theory]
    [AutoData]
    [FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.Open))]
    public void Open_MissingFileAndIncorrectMode_ShouldThrowFileNotFoundException(
        string path)
    {
        IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(path);

        Exception? exception = Record.Exception(() =>
        {
            _ = sut.Open(FileMode.Open);
        });

        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
    }

    [Theory]
    [InlineAutoData(FileAccess.Read, FileShare.Write)]
    [InlineAutoData(FileAccess.Write, FileShare.Read)]
    [FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.Open))]
    public void Open_ShouldUseGivenAccessAndShare(string path,
                                                  FileAccess access,
                                                  FileShare share)
    {
        FileSystem.File.WriteAllText(path, null);
        IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(path);

        using FileSystemStream stream = sut.Open(FileMode.Open, access, share);

        FileTestHelper.CheckFileAccess(stream).Should().Be(access);
        FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(share);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.Open))]
    public void Open_ShouldUseNoneShareAsDefault(string path,
                                                 FileAccess access)
    {
        FileSystem.File.WriteAllText(path, null);
        IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(path);

        using FileSystemStream stream = sut.Open(FileMode.Open, access);

        FileTestHelper.CheckFileAccess(stream).Should().Be(access);
        FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(FileShare.None);
    }

#if NETFRAMEWORK

    [Theory]
    [AutoData]
    [FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.Open))]
    public void Open_AppendMode_ShouldThrowArgumentException(
        string path)
    {
        FileSystem.File.WriteAllText(path, null);
        IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(path);

        Exception? exception = Record.Exception(() =>
        {
            _ = sut.Open(FileMode.Append);
        });

        exception.Should().BeOfType<ArgumentException>();
    }

    [Theory]
    [InlineAutoData(FileMode.Open, FileAccess.ReadWrite)]
    [InlineAutoData(FileMode.Create, FileAccess.ReadWrite)]
    [FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.Open))]
    public void Open_ShouldUseExpectedAccessDependingOnMode(
        FileMode mode,
        FileAccess expectedAccess,
        string path)
    {
        FileSystem.File.WriteAllText(path, null);
        IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(path);

        using FileSystemStream stream = sut.Open(mode);

        FileTestHelper.CheckFileAccess(stream).Should().Be(expectedAccess);
        FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(FileShare.None);
    }
#else
    [Theory]
    [InlineAutoData(FileMode.Append, FileAccess.Write)]
    [InlineAutoData(FileMode.Open, FileAccess.ReadWrite)]
    [InlineAutoData(FileMode.Create, FileAccess.ReadWrite)]
    [FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.Open))]
    public void Open_ShouldUseExpectedAccessDependingOnMode(
        FileMode mode,
        FileAccess expectedAccess,
        string path)
    {
        FileSystem.File.WriteAllText(path, null);
        IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(path);

        using FileSystemStream stream = sut.Open(mode);

        FileTestHelper.CheckFileAccess(stream).Should().Be(expectedAccess);
        FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(FileShare.None);
    }
#endif
}