namespace Testably.Abstractions.Tests;

public abstract class FileSystemDirectoryInfoFactoryTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    #region Test Setup

    public string BasePath { get; }

    public TFileSystem FileSystem { get; }
    public ITimeSystem TimeSystem { get; }

    protected FileSystemDirectoryInfoFactoryTests(
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
    [FileSystemTests.DirectoryInfoFactory(nameof(IFileSystem.IDirectoryInfoFactory.New))]
    public void New_EmptyPath_ShouldThrowArgumentException()
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.DirectoryInfo.New(string.Empty);
        });

#if NETFRAMEWORK
        exception.Should().BeOfType<ArgumentException>()
           .Which.Message.Should().Be("The path is not of a legal form.");
#else
        exception.Should().BeOfType<ArgumentException>()
           .Which.ParamName.Should().Be("path");
        exception.Should().BeOfType<ArgumentException>()
           .Which.Message.Should()
           .Be("The path is empty. (Parameter 'path')");
#endif
    }

    [Fact]
    [FileSystemTests.DirectoryInfoFactory(nameof(IFileSystem.IDirectoryInfoFactory.New))]
    public void New_Null_ShouldThrowArgumentNullException()
    {
        Exception? exception = Record.Exception(() =>
        {
            _ = FileSystem.DirectoryInfo.New(null!);
        });

        exception.Should().BeOfType<ArgumentNullException>();
    }

    [Theory]
    [AutoData]
    [FileSystemTests.DirectoryInfoFactory(nameof(IFileSystem.IDirectoryInfoFactory.New))]
    public void New_ShouldCreateNewDirectoryInfoFromPath(string path)
    {
        IFileSystem.IDirectoryInfo result = FileSystem.DirectoryInfo.New(path);

        result.ToString().Should().Be(path);
        result.Exists.Should().BeFalse();
    }
}