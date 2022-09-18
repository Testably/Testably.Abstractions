using FluentAssertions;
using Xunit;

namespace Testably.Abstractions.Testing.Tests.File;

public abstract partial class FileSystemMockDirectoryTests
{
    public IFileSystem FileSystem { get; }
    public string BasePath { get; }

    protected FileSystemMockDirectoryTests(IFileSystem fileSystem, string basePath)
    {
        FileSystem = fileSystem;
        BasePath = basePath;
    }

    [Fact]
    public void Test_Disposing()
    {
        var result = FileSystem.Directory.CreateDirectory("foo");

        result.Should().NotBeNull();
    }
}