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
    private sealed class InMemoryStorage : IStorage
    {
        private readonly ConcurrentDictionary<string, FileSystemInfoMock> _files = new();
        private readonly ConcurrentDictionary<string, DriveInfoMock> _drives = new();

        private readonly FileSystemMock _fileSystem;

        public InMemoryStorage(FileSystemMock fileSystem)
        {
            _fileSystem = fileSystem;
            DriveInfoMock mainDrive = new("".PrefixRoot(), _fileSystem);
            _drives.TryAdd(mainDrive.Name, mainDrive);
        }

        #region IStorage Members

        /// <inheritdoc cref="FileSystemMock.IStorage.CurrentDirectory" />
        public string CurrentDirectory { get; set; } = string.Empty.PrefixRoot();

        /// <inheritdoc cref="FileSystemMock.IStorage.Delete(string, bool)" />
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
                    throw ExceptionFactory.DirectoryNotEmpty(
                        _fileSystem.Path.GetFullPath(path));
                }
            }
            else if (fileSystemInfo is IStorage.IFileInfoMock fileInfoMock)
            {
                fileInfoMock.ClearBytes();
            }

            return _files.TryRemove(key, out _);
        }

        /// <inheritdoc
        ///     cref="FileSystemMock.IStorage.Enumerate{TFileSystemInfo}(string, string, EnumerationOptions, Func{Exception})" />
        public IEnumerable<TFileSystemInfo> Enumerate<TFileSystemInfo>(string path,
                                                                       string expression,
                                                                       EnumerationOptions enumerationOptions,
                                                                       Func<Exception> notFoundException)
            where TFileSystemInfo : IFileSystem.IFileSystemInfo
        {
            ValidateExpression(expression);
            string key = _fileSystem.Path.GetFullPath(path)
               .NormalizeAndTrimPath(_fileSystem);
            if (!_files.ContainsKey(key))
            {
                throw notFoundException();
            }

            foreach (FileSystemInfoMock file in _files
               .Where(x => x.Key.StartsWith(key) && x.Key != key)
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

        /// <inheritdoc cref="FileSystemMock.IStorage.Exists(string?)" />
        public bool Exists([NotNullWhen(true)] string? path)
        {
            if (path == null)
            {
                return false;
            }

            return _files.ContainsKey(_fileSystem.Path.GetFullPath(path)
               .NormalizeAndTrimPath(_fileSystem));
        }

        /// <inheritdoc cref="FileSystemMock.IStorage.GetDirectory(string)" />
        public IStorage.IDirectoryInfoMock? GetDirectory(string path)
        {
            if (_files.TryGetValue(
                _fileSystem.Path.GetFullPath(path).NormalizeAndTrimPath(_fileSystem),
                out FileSystemInfoMock? fileInfo))
            {
                return fileInfo as IStorage.IDirectoryInfoMock;
            }

            return null;
        }

        /// <inheritdoc cref="FileSystemMock.IStorage.GetDrive(string)" />
        public IDriveInfoMock? GetDrive(string? driveName)
        {
            if (string.IsNullOrEmpty(driveName))
            {
                return null;
            }

            DriveInfoMock drive = new(driveName, _fileSystem);
            if (_drives.TryGetValue(drive.Name, out DriveInfoMock? d))
            {
                return d;
            }

            return null;
        }

        /// <inheritdoc cref="FileSystemMock.IStorage.GetOrAddDrive(string)" />
        public IDriveInfoMock GetOrAddDrive(string driveName)
        {
            DriveInfoMock drive = new(driveName, _fileSystem);
            return _drives.GetOrAdd(drive.Name, _ => drive);
        }

        /// <inheritdoc cref="FileSystemMock.IStorage.GetDrives()" />
        public IEnumerable<IDriveInfoMock> GetDrives()
            => _drives.Values;

        /// <inheritdoc cref="FileSystemMock.IStorage.GetFile(string)" />
        public IStorage.IFileInfoMock? GetFile(string path)
        {
            if (_files.TryGetValue(
                _fileSystem.Path.GetFullPath(path).NormalizeAndTrimPath(_fileSystem),
                out FileSystemInfoMock? fileInfo))
            {
                return fileInfo as IStorage.IFileInfoMock;
            }

            return null;
        }

        /// <inheritdoc cref="FileSystemMock.IStorage.GetOrAddDirectory(string)" />
        public IStorage.IDirectoryInfoMock? GetOrAddDirectory(string path)
        {
            CallbackChange? fileSystemChange = null;
            IStorage.IDirectoryInfoMock? directory = _files.GetOrAdd(
                _fileSystem.Path.GetFullPath(path).NormalizeAndTrimPath(_fileSystem),
                _ =>
                {
                    FileSystemInfoMock directoryMock = CreateDirectoryInternal(path);
                    var access = directoryMock.RequestAccess(FileAccess.Write, FileShare.ReadWrite);
                    fileSystemChange = _fileSystem.Callback.InvokeChangeOccurring(
                        directoryMock.FullName,
                        CallbackChangeTypes.DirectoryCreated,
                        NotifyFilters.CreationTime);
                    access.Dispose();
                    return directoryMock;
                }) as IStorage.IDirectoryInfoMock;
            _fileSystem.Callback.InvokeChangeOccurred(fileSystemChange);
            return directory;
        }

        /// <inheritdoc cref="FileSystemMock.IStorage.GetOrAddFile(string)" />
        public IStorage.IFileInfoMock? GetOrAddFile(string path)
        {
            CallbackChange? fileSystemChange = null;
            var file = _files.GetOrAdd(
                _fileSystem.Path.GetFullPath(path).NormalizeAndTrimPath(_fileSystem),
                _ =>
                {
                    var fileMock = CreateFileInternal(path);
                    var access = fileMock.RequestAccess(FileAccess.Write, FileShare.ReadWrite);
                    fileSystemChange = _fileSystem.Callback.InvokeChangeOccurring(
                        fileMock.FullName,
                        CallbackChangeTypes.FileCreated,
                        NotifyFilters.CreationTime);
                    access.Dispose();
                    return fileMock;
                }) as IStorage.IFileInfoMock;
            _fileSystem.Callback.InvokeChangeOccurred(fileSystemChange);
            return file;
        }

        /// <inheritdoc cref="FileSystemMock.IStorage.GetSubdirectoryPath(string, string)" />
        public string GetSubdirectoryPath(string fullFilePath, string givenPath)
        {
            if (_fileSystem.Path.IsPathRooted(givenPath))
            {
                return fullFilePath;
            }

            if (CurrentDirectory == string.Empty.PrefixRoot())
            {
                return fullFilePath.Substring(CurrentDirectory.Length);
            }

            return fullFilePath.Substring(CurrentDirectory.Length + 1);
        }

        #endregion

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

            List<IDisposable> requests = new();
            foreach (string? parentPath in parents)
            {
                string key = _fileSystem.Path.GetFullPath(parentPath)
                   .NormalizeAndTrimPath(_fileSystem);
                CallbackChange? fileSystemChange = null;
                FileSystemInfoMock directory = _files.AddOrUpdate(
                    key,
                    _ =>
                    {
                        fileSystemChange = _fileSystem.Callback.InvokeChangeOccurring(
                            parentPath,
                            CallbackChangeTypes.DirectoryCreated,
                            NotifyFilters.CreationTime);
                        return DirectoryInfoMock.New(parentPath, _fileSystem);
                    },
                    (_, fileSystemInfo) =>
                        fileSystemInfo.AdjustTimes(timeAdjustments));
                _fileSystem.Callback.InvokeChangeOccurred(fileSystemChange);
                requests.Add(directory.RequestAccess(FileAccess.Write,
                    FileShare.ReadWrite));
            }

            foreach (IDisposable request in requests)
            {
                request.Dispose();
            }

            if (Framework.IsNetFramework)
            {
                return DirectoryInfoMock.New(path, _fileSystem.Path.GetFileName(path),
                    _fileSystem);
            }

            return DirectoryInfoMock.New(path, _fileSystem);
        }

        private FileSystemInfoMock CreateFileInternal(string path)
        {
            return FileInfoMock.New(path, _fileSystem);
        }

        private static void ValidateExpression(string expression)
        {
            if (expression.Contains('\0'))
            {
                throw ExceptionFactory.PathHasIllegalCharacters(expression);
            }
        }
    }
}