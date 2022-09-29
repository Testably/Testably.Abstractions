using System.IO;

namespace Testably.Abstractions.Tests;

public abstract class FileSystemFileInfoFactoryTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    #region Test Setup

    public string BasePath { get; }

    public TFileSystem FileSystem { get; }
    public ITimeSystem TimeSystem { get; }

    protected FileSystemFileInfoFactoryTests(
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
    [FileSystemTests.FileInfoFactory(nameof(IFileSystem.IFileInfoFactory.New))]
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
    [FileSystemTests.FileInfoFactory(nameof(IFileSystem.IFileInfoFactory.New))]
    public void New_ShouldCreateNewFileInfoFromPath(string path)
    {
        IFileSystem.IFileInfo result = FileSystem.FileInfo.New(path);

        result.ToString().Should().Be(path);
        result.Exists.Should().BeFalse();
    }

    [Theory]
    [AutoData]
    [FileSystemTests.FileInfoFactory(nameof(IFileSystem.IFileInfoFactory.New))]
    public void Wrap_ShouldWrapFromFileInfo(string path)
    {
        FileInfo fileInfo = new(path);

        IFileSystem.IFileInfo result = FileSystem.FileInfo.Wrap(fileInfo);

        result.FullName.Should().Be(fileInfo.FullName);
        result.Exists.Should().Be(fileInfo.Exists);
    }
}