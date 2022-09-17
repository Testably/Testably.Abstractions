namespace Testably.Abstractions.Testing.Internal.Models;

/// <summary>
///     A mocked directory in the <see cref="InMemoryFileSystem" />.
/// </summary>
public class MockDirectoryInfo : MockFileData, IFileSystem.IDirectoryInfo
{
    private readonly FileSystemMock _fileSystemMock;
    private readonly string _path;

    internal MockDirectoryInfo(FileSystemMock fileSystemMock, string path)
    {
        _fileSystemMock = fileSystemMock;
        _path = path;
    }
}