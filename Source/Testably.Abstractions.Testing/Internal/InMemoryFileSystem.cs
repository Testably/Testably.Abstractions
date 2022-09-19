﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Testably.Abstractions.Testing.Internal.Models;

namespace Testably.Abstractions.Testing.Internal;

internal class InMemoryFileSystem : FileSystemMock.IInMemoryFileSystem
{
    public IFileSystem FileSystem
        => _fileSystem;

    private readonly ConcurrentDictionary<string, FileSystemInfoMock> _files = new();

    private readonly FileSystemMock _fileSystem;

    public InMemoryFileSystem(FileSystemMock fileSystem)
    {
        _fileSystem = fileSystem;
    }

    #region IInMemoryFileSystem Members

    public string CurrentDirectory { get; set; } = "".PrefixRoot();

    /// <inheritdoc />
    public bool Delete(string path)
    {
        return _files.TryRemove(
            _fileSystem.Path.GetFullPath(path).NormalizeAndTrimPath(_fileSystem), out _);
    }

    /// <inheritdoc />
    public bool Exists([NotNullWhen(true)] string? path)
    {
        if (path == null)
        {
            return false;
        }

        return _files.ContainsKey(_fileSystem.Path.GetFullPath(path)
           .NormalizeAndTrimPath(_fileSystem));
    }

    /// <inheritdoc />
    public IFileSystem.IDirectoryInfo? GetOrAddDirectory(string path)
    {
        return _files.GetOrAdd(
            _fileSystem.Path.GetFullPath(path).NormalizeAndTrimPath(_fileSystem),
            _ => CreateDirectoryInternal(path)) as IFileSystem.IDirectoryInfo;
    }

    #endregion

    private FileSystemInfoMock CreateDirectoryInternal(string path)
    {
        List<string> parents = new();
        string? parent = FileSystem.Path.GetDirectoryName(
            path.TrimEnd(FileSystem.Path.DirectorySeparatorChar,
                FileSystem.Path.AltDirectorySeparatorChar));
        while (!string.IsNullOrEmpty(parent))
        {
            parents.Add(parent!);
            parent = FileSystem.Path.GetDirectoryName(parent);
        }

        parents.Reverse();
        TimeAdjustments timeAdjustments = TimeAdjustments.LastWriteTime;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            timeAdjustments |= TimeAdjustments.LastAccessTime;
        }

        foreach (string? parentPath in parents)
        {
            string key = _fileSystem.Path.GetFullPath(parentPath)
               .NormalizeAndTrimPath(_fileSystem);
            _files.AddOrUpdate(
                key,
                _ => new DirectoryInfoMock(parentPath, _fileSystem),
                (_, fileSystemInfo) =>
                    fileSystemInfo.AdjustTimes(timeAdjustments));
        }

        return new DirectoryInfoMock(path, _fileSystem);
    }
}