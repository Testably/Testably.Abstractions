using System.IO;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemDirectoryInfoTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    #region Test Setup

    public TFileSystem FileSystem { get; }
    public ITimeSystem TimeSystem { get; }
    public string BasePath { get; }

    protected FileSystemDirectoryInfoTests(
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
    public void New_ShouldCreateNewDirectoryInfoFromPath(string path)
    {
        IFileSystem.IDirectoryInfo result = FileSystem.DirectoryInfo.New(path);

        result.ToString().Should().Be(path);
        result.Exists.Should().BeFalse();
    }

    [Theory]
    [AutoData]
    public void Parent_ArbitraryPaths_ShouldNotBeNull(string path1,
                                                      string path2,
                                                      string path3)
    {
        string path = FileSystem.Path.Combine(path1, path2, path3);

        IFileSystem.IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

        sut.Parent.Should().NotBeNull();
        sut.Parent!.Exists.Should().BeFalse();
        sut.Parent.Parent.Should().NotBeNull();
        sut.Parent.Parent!.Exists.Should().BeFalse();
    }

    [Theory]
    [AutoData]
    public void Root_ShouldExist(string path)
    {
        string expectedRoot = string.Empty.PrefixRoot();
        IFileSystem.IDirectoryInfo result = FileSystem.DirectoryInfo.New(path);

        result.Root.Exists.Should().BeTrue();
        result.Root.FullName.Should().Be(expectedRoot);
    }

    [Theory]
    [AutoData]
    public void Wrap_ShouldWrapFromDirectoryInfo(string path)
    {
        DirectoryInfo directoryInfo = new(path);

        IFileSystem.IDirectoryInfo result = FileSystem.DirectoryInfo.Wrap(directoryInfo);

        result.FullName.Should().Be(directoryInfo.FullName);
        result.Exists.Should().Be(directoryInfo.Exists);
    }
}