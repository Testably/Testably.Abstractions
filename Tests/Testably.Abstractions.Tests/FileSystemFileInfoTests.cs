namespace Testably.Abstractions.Tests;

public abstract class FileSystemFileSystemInfoTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    public string BasePath { get; }

    public TFileSystem FileSystem { get; }
    public ITimeSystem TimeSystem { get; }

    protected FileSystemFileSystemInfoTests(
        TFileSystem fileSystem,
        ITimeSystem timeSystem,
        string basePath)
    {
        FileSystem = fileSystem;
        TimeSystem = timeSystem;
        BasePath = basePath;
    }
}