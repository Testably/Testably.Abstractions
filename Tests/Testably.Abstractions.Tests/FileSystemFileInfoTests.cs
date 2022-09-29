using System.IO;

namespace Testably.Abstractions.Tests;

public abstract class FileSystemFileSystemInfoTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    #region Test Setup

    public string BasePath { get; }
    public TFileSystem FileSystem { get; }
    public ITimeSystem TimeSystem { get; }

    protected FileSystemFileSystemInfoTests(
        TFileSystem fileSystem,
        ITimeSystem timeSystem,
        string basePath)
    {
        FileSystem = fileSystem;
        TimeSystem = timeSystem;
        BasePath = basePath;
    }

    #endregion

    [Fact]
    [Trait(nameof(FileSystem), nameof(FileInfo))]
    public void New_Null_ShouldThrowArgumentNullException()
    {
        Exception? exception = Record.Exception(() =>
        {
            _ = FileSystem.FileInfo.New(null!);
        });

        exception.Should().BeOfType<ArgumentNullException>();
    }

    [Theory]
    [AutoData]
    [Trait(nameof(FileSystem), nameof(FileInfo))]
    public void New_ShouldCreateNewFileInfoFromPath(string path)
    {
        IFileSystem.IFileInfo result = FileSystem.FileInfo.New(path);

        result.ToString().Should().Be(path);
        result.Exists.Should().BeFalse();
    }

    [Theory]
    [AutoData]
    [Trait(nameof(FileSystem), nameof(FileInfo))]
    public void Wrap_ShouldWrapFromFileInfo(string path)
    {
        FileInfo fileInfo = new(path);

        IFileSystem.IFileInfo result = FileSystem.FileInfo.Wrap(fileInfo);

        result.FullName.Should().Be(fileInfo.FullName);
        result.Exists.Should().Be(fileInfo.Exists);
    }
}