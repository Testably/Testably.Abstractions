using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Testably.Abstractions.Tests;

public abstract class FileSystemFileSystemInfoTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    #region Test Setup

    public TFileSystem FileSystem { get; }
    public ITimeSystem TimeSystem { get; }
    public string BasePath { get; }

    protected FileSystemFileSystemInfoTests(
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
    public void X()
    {
        var sut = FileSystem.FileInfo.New("foo");
    }
}