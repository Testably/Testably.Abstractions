using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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

    public string CurrentDirectory { get; set; } = "".PrefixRoot();

    /// <inheritdoc />
    public IFileSystem.IDirectoryInfo? GetOrAddDirectory(string path)
    {
        return _files.GetOrAdd(_fileSystem.Path.GetFullPath(path).NormalizeAndTrimPath(_fileSystem),
                p => new DirectoryInfoMock(path, _fileSystem)) as
            IFileSystem.IDirectoryInfo;
    }

    /// <inheritdoc />
    public bool Exists([NotNullWhen(true)] string? path)
    {
        if (path == null)
        {
            return false;
        }

        return _files.ContainsKey(_fileSystem.Path.GetFullPath(path).NormalizeAndTrimPath(_fileSystem));
    }

    /// <inheritdoc />
    public bool Delete(string path)
    {
        return _files.TryRemove(_fileSystem.Path.GetFullPath(path).NormalizeAndTrimPath(_fileSystem), out _);
    }

    #endregion

}