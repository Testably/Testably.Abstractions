namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemDirectoryTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    #region Test Setup

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

    #endregion

    [Fact]
    [FileSystemTests.Directory(nameof(IFileSystem.IDirectory.GetLogicalDrives))]
    public void GetLogicalDrives_ShouldNotBeEmpty()
    {
        string[] result = FileSystem.Directory.GetLogicalDrives();

        result.Should().NotBeEmpty();
        result.Should().Contain("".PrefixRoot());
    }
}