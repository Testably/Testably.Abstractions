using FluentAssertions;
using Xunit;

namespace Testably.Abstractions.Testing.Tests.File;

public partial class FileSystemMockTests
{
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