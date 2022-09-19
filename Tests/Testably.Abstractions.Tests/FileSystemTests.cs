using FluentAssertions;
using Xunit;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemTests
{
    #region Test Setup

    public IFileSystem FileSystem { get; }

    protected FileSystemTests(IFileSystem fileSystem)
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