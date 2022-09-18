using System;
using System.Collections.Concurrent;
using Testably.Abstractions.Testing.Internal.Models;

namespace Testably.Abstractions.Testing.Internal;

internal class InMemoryFileSystem : FileSystemMock.IInMemoryFileSystem
{
    public IFileSystem FileSystem => _fileSystem;
    private readonly FileSystemMock _fileSystem;

    private readonly ConcurrentDictionary<string, FileSystemInfoMock> _files = new();

    public InMemoryFileSystem(FileSystemMock fileSystem)
    {
        _fileSystem = fileSystem;
    }

    #region IInMemoryFileSystem Members

    public IFileSystem.IDirectoryInfo? GetOrAddDirectory(string path,
                                                         Func<string, DirectoryInfoMock>
                                                             func)
    {
        return _files.GetOrAdd(path, func) as IFileSystem.IDirectoryInfo;
    }

    #endregion

    /// <inheritdoc />
    public IFileSystem.IDirectoryInfo? GetOrAddDirectory(string path)
    {
        return _files.GetOrAdd(path, p => new DirectoryInfoMock(path, _fileSystem)) as IFileSystem.IDirectoryInfo;
    }

    /// <inheritdoc />
    public bool Exists(string path)
    {
        return _files.ContainsKey(path);
    }

    /// <inheritdoc />
    public bool Delete(string path)
    {
        return _files.TryRemove(path, out _);
    }
}