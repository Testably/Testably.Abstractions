namespace Testably.Abstractions.Testing.Internal;

internal class FileGenerator : FileSystemMock.IGenerator
{
    private readonly FileSystemMock _fileSystem;

    public FileGenerator(FileSystemMock fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public IFileSystem FileSystem => _fileSystem;
}