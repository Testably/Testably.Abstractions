namespace Testably.Abstractions.Tests;

public abstract class FileSystemTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    #region Test Setup

    public TFileSystem FileSystem { get; }

    protected FileSystemTests(TFileSystem fileSystem)
    {
        FileSystem = fileSystem;
    }

    #endregion

    [Fact]
    public void Directory_ShouldSetExtensionPoint()
    {
        IFileSystem result = FileSystem.Directory.FileSystem;

        result.Should().Be(FileSystem);
    }

    [Fact]
    public void DirectoryInfo_ShouldSetExtensionPoint()
    {
        IFileSystem result = FileSystem.DirectoryInfo.FileSystem;

        result.Should().Be(FileSystem);
    }

    [Fact]
    public void File_ShouldSetExtensionPoint()
    {
        IFileSystem result = FileSystem.File.FileSystem;

        result.Should().Be(FileSystem);
    }

    [Fact]
    public void FileInfo_ShouldSetExtensionPoint()
    {
        IFileSystem result = FileSystem.FileInfo.FileSystem;

        result.Should().Be(FileSystem);
    }

    [Fact]
    public void Path_ShouldSetExtensionPoint()
    {
        IFileSystem result = FileSystem.Path.FileSystem;

        result.Should().Be(FileSystem);
    }
}