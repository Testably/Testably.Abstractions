using System;
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

    /// <inheritdoc cref="FileSystemMock.IInMemoryFileSystem.CurrentDirectory" />
    public string CurrentDirectory { get; set; } = string.Empty.PrefixRoot();

    /// <inheritdoc cref="FileSystemMock.IInMemoryFileSystem.Delete(string, bool)" />
    public bool Delete(string path, bool recursive = false)
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
                throw new IOException($"Directory not empty : '{path}'");
            }
        }

        return _files.TryRemove(key, out _);
    }

    /// <inheritdoc cref="FileSystemMock.IInMemoryFileSystem.Enumerate{TFileSystemInfo}(string, string, EnumerationOptions)" />
    public IEnumerable<TFileSystemInfo> Enumerate<TFileSystemInfo>(string path,
        string expression,
        EnumerationOptions enumerationOptions)
        where TFileSystemInfo : IFileSystem.IFileSystemInfo
    {
        if (expression.Contains('\0'))
        {
            throw new ArgumentException("Argument_InvalidPathChars", nameof(expression));
        }

        if (path.Contains('\0'))
        {
            throw new ArgumentException("Argument_InvalidPathChars", nameof(path));
        }

        string key = _fileSystem.Path.GetFullPath(path).NormalizeAndTrimPath(_fileSystem);
        string start = key + FileSystem.Path.DirectorySeparatorChar;
        foreach (FileSystemInfoMock file in _files
           .Where(x => x.Key.StartsWith(start))
           .Select(x => x.Value))
        {
            if (file is TFileSystemInfo matchingType)
            {
                string? parentPath =
                    FileSystem.Path.GetDirectoryName(file.FullName);
                if (!enumerationOptions.RecurseSubdirectories && parentPath != key)
                {
                    continue;
                }

                if (!EnumerationOptionsHelper.MatchesPattern(enumerationOptions,
                    matchingType.Name, expression))
                {
                    continue;
                }

                yield return matchingType;
            }
        }
    }

    /// <inheritdoc cref="FileSystemMock.IInMemoryFileSystem.Exists(string?)" />
    public bool Exists([NotNullWhen(true)] string? path)
    {
        if (path == null)
        {
            return false;
        }

        return _files.ContainsKey(_fileSystem.Path.GetFullPath(path)
           .NormalizeAndTrimPath(_fileSystem));
    }

    /// <inheritdoc cref="FileSystemMock.IInMemoryFileSystem.GetOrAddDirectory(string)" />
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
            parents.Add(parent);
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