using FluentAssertions;
using Xunit;

namespace Testably.Abstractions.Tests.File;

public class FileSystemTests
{
    [Fact]
    public void Directory_ShouldSetExtensionPoint()
    {
        FileSystem fileSystem = new();

        IFileSystem result = fileSystem.Directory.FileSystem;

        result.Should().Be(fileSystem);
    }

    [Fact]
    public void File_ShouldSetExtensionPoint()
    {
        FileSystem fileSystem = new();

        IFileSystem result = fileSystem.File.FileSystem;

        result.Should().Be(fileSystem);
    }

    [Fact]
    public void Path_ShouldSetExtensionPoint()
    {
        FileSystem fileSystem = new();

        IFileSystem result = fileSystem.Path.FileSystem;

        result.Should().Be(fileSystem);
    }
}