using System.IO;

namespace Testably.Abstractions.Tests;

public abstract class FileSystemDirectoryInfoFactoryTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    #region Test Setup

    public abstract string BasePath { get; }
    public TFileSystem FileSystem { get; }
    public ITimeSystem TimeSystem { get; }

    protected FileSystemDirectoryInfoFactoryTests(
        TFileSystem fileSystem,
        ITimeSystem timeSystem)
    {
        FileSystem = fileSystem;
        TimeSystem = timeSystem;
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

    [Theory]
    [AutoData]
    [FileSystemTests.DirectoryInfoFactory(nameof(IFileSystem.IDirectoryInfoFactory.Wrap))]
    public void Wrap_ShouldWrapFromDirectoryInfo(string path)
    {
        DirectoryInfo directoryInfo = new("S:\\" + path);

        IFileSystem.IDirectoryInfo result = FileSystem.DirectoryInfo.Wrap(directoryInfo);

        result.FullName.Should().Be(directoryInfo.FullName);
        result.Exists.Should().Be(directoryInfo.Exists);
    }
}