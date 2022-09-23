using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    private sealed class InMemoryFileSystem : IInMemoryFileSystem
    {
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
            string key = _fileSystem.Path.GetFullPath(path)
               .NormalizeAndTrimPath(_fileSystem);
            if (!_files.TryGetValue(key, out FileSystemInfoMock? fileSystemInfo))
            {
                return false;
            }

            if (fileSystemInfo is IFileSystem.IDirectoryInfo)
            {
                string start = key + _fileSystem.Path.DirectorySeparatorChar;
                if (recursive)
                {
                    foreach (KeyValuePair<string, FileSystemInfoMock> file in
                        _files.Where(x
                            => x.Key.StartsWith(start)))
                    {
                        _files.TryRemove(file.Key, out _);
                    }
                }
                else if (_files.Any(x => x.Key.StartsWith(start)))
                {
                    throw new IOException(
                        $"Directory not empty : '{_fileSystem.Path.GetFullPath(path)}'");
                }
            }

            return _files.TryRemove(key, out _);
        }

        /// <inheritdoc cref="FileSystemMock.IInMemoryFileSystem.Enumerate{TFileSystemInfo}(string, string, EnumerationOptions, Func{Exception})" />
        public IEnumerable<TFileSystemInfo> Enumerate<TFileSystemInfo>(string path,
            string expression,
            EnumerationOptions enumerationOptions,
            Func<Exception> notFoundException)
            where TFileSystemInfo : IFileSystem.IFileSystemInfo
        {
            ValidateExpression(expression);
            string key = _fileSystem.Path.GetFullPath(path)
               .NormalizeAndTrimPath(_fileSystem);
            string start = key + _fileSystem.Path.DirectorySeparatorChar;
            if (!_files.ContainsKey(key))
            {
                throw notFoundException();
            }
            foreach (FileSystemInfoMock file in _files
               .Where(x => x.Key.StartsWith(start))
               .Select(x => x.Value))
            {
                if (file is TFileSystemInfo matchingType)
                {
                    string? parentPath =
                        _fileSystem.Path.GetDirectoryName(file.FullName);
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

        /// <inheritdoc cref="FileSystemMock.IInMemoryFileSystem.GetOrAddFile(string)" />
        public IInMemoryFileSystem.IWritableFileInfo? GetOrAddFile(string path)
        {
            return _files.GetOrAdd(
                _fileSystem.Path.GetFullPath(path).NormalizeAndTrimPath(_fileSystem),
                _ => CreateFileInternal(path)) as IInMemoryFileSystem.IWritableFileInfo;
        }

        /// <inheritdoc cref="FileSystemMock.IInMemoryFileSystem.GetFile(string)" />
        public IInMemoryFileSystem.IWritableFileInfo? GetFile(string path)
        {
            if (_files.TryGetValue(
                _fileSystem.Path.GetFullPath(path).NormalizeAndTrimPath(_fileSystem),
                out var fileInfo))
            {
                return fileInfo as IInMemoryFileSystem.IWritableFileInfo;
            }

            return null;
        }


        /// <inheritdoc cref="FileSystemMock.IInMemoryFileSystem.GetSubdirectoryPath(string, string)" />
        public string GetSubdirectoryPath(string fullFilePath, string givenPath)
        {
            if (_fileSystem.Path.IsPathRooted(givenPath))
            {
                return fullFilePath;
            }

            return fullFilePath.Substring(CurrentDirectory.Length + 1);
        }

        #endregion

        private FileSystemInfoMock CreateFileInternal(string path)
        {
            return FileInfoMock.New(path, _fileSystem);
        }
        private FileSystemInfoMock CreateDirectoryInternal(string path)
        {
            List<string> parents = new();
            string? parent = _fileSystem.Path.GetDirectoryName(
                path.TrimEnd(_fileSystem.Path.DirectorySeparatorChar,
                    _fileSystem.Path.AltDirectorySeparatorChar));
            while (!string.IsNullOrEmpty(parent))
            {
                parents.Add(parent);
                parent = _fileSystem.Path.GetDirectoryName(parent);
            }

            parents.Reverse();
            FileSystemInfoMock.TimeAdjustments timeAdjustments =
                FileSystemInfoMock.TimeAdjustments.LastWriteTime;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                timeAdjustments |= FileSystemInfoMock.TimeAdjustments.LastAccessTime;
            }

            foreach (string? parentPath in parents)
            {
                string key = _fileSystem.Path.GetFullPath(parentPath)
                   .NormalizeAndTrimPath(_fileSystem);
                _files.AddOrUpdate(
                    key,
                    _ => DirectoryInfoMock.New(parentPath, _fileSystem),
                    (_, fileSystemInfo) =>
                        fileSystemInfo.AdjustTimes(timeAdjustments));
            }

#if NETFRAMEWORK
            return DirectoryInfoMock.New(path, _fileSystem.Path.GetFileName(path), _fileSystem);
#else
            return DirectoryInfoMock.New(path, _fileSystem);
#endif
        }

        private static void ValidateExpression(string expression)
        {
            if (expression.Contains('\0'))
            {
                throw new ArgumentException(
                    $"Illegal characters in path '{{0}}'. (Parameter '{expression}')",
                    nameof(expression));
            }
        }
    }
}