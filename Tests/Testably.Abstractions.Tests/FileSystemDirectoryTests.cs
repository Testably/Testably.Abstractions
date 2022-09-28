using System.IO;
using Testably.Abstractions.Tests.TestHelpers.Attributes;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemDirectoryTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    public string BasePath { get; }

    public TFileSystem FileSystem { get; }
    public ITimeSystem TimeSystem { get; }

    protected FileSystemDirectoryTests(
        TFileSystem fileSystem,
        ITimeSystem timeSystem,
        string basePath)
    {
        FileSystem = fileSystem;
        TimeSystem = timeSystem;
        BasePath = basePath;
    }

    [Fact]
    public void GetDirectoryRoot_ShouldReturnDefaultRoot()
    {
        string expectedRoot = "".PrefixRoot();

        string result = FileSystem.Directory.GetDirectoryRoot("foo");

        result.Should().Be(expectedRoot);
    }

    [Fact]
    public void GetDirectoryRoot_Empty_ShouldThrowArgumentException()
    {
        Exception? exception = Record.Exception(() =>
        {
            FileSystem.Directory.GetDirectoryRoot("");
        });

        exception.Should().BeOfType<ArgumentException>();
    }

    [Theory]
    [InlineData('A')]
    [InlineData('C')]
    [InlineData('X')]
    [WindowsOnly]
    public void GetDirectoryRoot_SpecificDrive_ShouldReturnRootWithCorrectDrive(
        char drive)
    {
        string expectedRoot = "".PrefixRoot(drive);
        string path = Path.Combine($"{drive}:\\foo", "bar");

        string result = FileSystem.Directory.GetDirectoryRoot(path);

        result.Should().Be(expectedRoot);
    }
}