using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        return _files.GetOrAdd(
            _fileSystem.Path.GetFullPath(path).NormalizeAndTrimPath(_fileSystem),
                _ => CreateDirectoryInternal(path)) as IFileSystem.IDirectoryInfo;
    }

    private FileSystemInfoMock CreateDirectoryInternal(string path)
    {
        List<string> parents = new();
        var parent = FileSystem.Path.GetDirectoryName(
            path.TrimEnd(FileSystem.Path.DirectorySeparatorChar, FileSystem.Path.AltDirectorySeparatorChar));
        while (parent != null)
        {
            parents.Add(parent);
            parent = FileSystem.Path.GetDirectoryName(parent);
        }
        parents.Reverse();
        foreach (var parentPath in parents)
        {
            _files.TryAdd(_fileSystem.Path.GetFullPath(parentPath).NormalizeAndTrimPath(_fileSystem),
                new DirectoryInfoMock(parentPath, _fileSystem));
        }
        return new DirectoryInfoMock(path, _fileSystem);
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