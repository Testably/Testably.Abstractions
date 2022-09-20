using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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

    public string CurrentDirectory { get; set; } = string.Empty.PrefixRoot();

    /// <inheritdoc />
    public bool Delete(string path, bool recursive)
    {
        string key = _fileSystem.Path.GetFullPath(path).NormalizeAndTrimPath(_fileSystem);
        if (!_files.TryGetValue(key, out FileSystemInfoMock? fileSystemInfo))
        {
            return false;
        }

        if (fileSystemInfo is IFileSystem.IDirectoryInfo)
        {
            string start = key + FileSystem.Path.DirectorySeparatorChar;
            if (recursive)
            {
                foreach (KeyValuePair<string, FileSystemInfoMock> file in _files.Where(x
                    => x.Key.StartsWith(start)))
                {
                    _files.TryRemove(file.Key, out _);
                }
            }
            else if (_files.Any(x => x.Key.StartsWith(start)))
            {
                throw new IOException($"The directory is not empty. : '{path}'");
            }
        }

        return _files.TryRemove(key, out _);
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