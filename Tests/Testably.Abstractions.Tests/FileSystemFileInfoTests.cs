using System.IO;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileInfoTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    #region Test Setup

    public string BasePath { get; }

    public TFileSystem FileSystem { get; }
    public ITimeSystem TimeSystem { get; }

    protected FileSystemFileInfoTests(
        TFileSystem fileSystem,
        ITimeSystem timeSystem,
        string basePath)
    {
        FileSystem = fileSystem;
        TimeSystem = timeSystem;
        BasePath = basePath;
    }

    #endregion

    [Theory]
    [AutoData]
    [FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.Length))]
    public void Length(string path, byte[] bytes)
    {
        FileSystem.File.WriteAllBytes(path, bytes);
        IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(path);

        long result = sut.Length;

        result.Should().Be(bytes.Length);
    }

    [Theory]
    [AutoData]
    [FileSystemTests.FileInfo(nameof(IFileSystem.IFileInfo.Length))]
    public void Length_MissingFile_ShouldThrowFileNotFoundException(string path)
    {
        IFileSystem.IFileInfo sut = FileSystem.FileInfo.New(path);

        Exception? exception = Record.Exception(() =>
        {
            _ = sut.Length;
        });

#if NETFRAMEWORK
        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should().Contain($"'{path}'");
#else
        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
#endif
    }
}