using FluentAssertions;
using Xunit;

namespace Testably.Abstractions.Testing.Tests.File;

public class FileSystemMockTests
{
    [Fact]
    public void Directory_ShouldSetExtensionPoint()
    {
        FileSystemMock fileSystem = new();

        IFileSystem result = fileSystem.Directory.FileSystem;

        result.Should().Be(fileSystem);
    }

    [Fact]
    public void DirectoryInfo_ShouldSetExtensionPoint()
    {
        FileSystemMock fileSystem = new();

        IFileSystem result = fileSystem.DirectoryInfo.FileSystem;

        result.Should().Be(fileSystem);
    }

    [Fact]
    public void File_ShouldSetExtensionPoint()
    {
        FileSystemMock fileSystem = new();

        IFileSystem result = fileSystem.File.FileSystem;

        result.Should().Be(fileSystem);
    }

    [Fact]
    public void Generate_ShouldSetExtensionPoint()
    {
        FileSystemMock fileSystem = new();

        IFileSystem result = fileSystem.Generate.FileSystem;

        result.Should().Be(fileSystem);
    }

    [Fact]
    public void Path_ShouldSetExtensionPoint()
    {
        FileSystemMock fileSystem = new();

        IFileSystem result = fileSystem.Path.FileSystem;

        result.Should().Be(fileSystem);
    }
}