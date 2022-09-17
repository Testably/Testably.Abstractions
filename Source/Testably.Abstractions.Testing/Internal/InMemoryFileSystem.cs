using System;
using System.Collections.Concurrent;
using Testably.Abstractions.Testing.Internal.Models;

namespace Testably.Abstractions.Testing.Internal;

internal class InMemoryFileSystem : FileSystemMock.IInMemoryFileSystem
{
    public IFileSystem FileSystem => _fileSystem;
    private readonly FileSystemMock _fileSystem;

    private readonly ConcurrentDictionary<string, MockFileData> _files = new();

    public InMemoryFileSystem(FileSystemMock fileSystem)
    {
        _fileSystem = fileSystem;
    }

    #region IInMemoryFileSystem Members

    public IFileSystem.IDirectoryInfo? GetOrAddDirectory(string path,
                                                         Func<string, MockDirectoryInfo>
                                                             func)
    {
        return _files.GetOrAdd(path, func) as IFileSystem.IDirectoryInfo;
    }

    #endregion
}