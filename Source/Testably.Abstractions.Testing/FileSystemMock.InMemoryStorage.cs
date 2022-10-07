using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Testably.Abstractions.Testing.Internal;
using static Testably.Abstractions.Testing.FileSystemMock.IStorage;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    internal sealed class InMemoryStorage : IStorage
    {
        private readonly ConcurrentDictionary<InMemoryLocation, InMemoryContainer>
            _containers = new();

        private readonly ConcurrentDictionary<string, FileSystemInfoMock> _files = new(
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? StringComparer.Ordinal
                : StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<string, DriveInfoMock> _drives =
            new(StringComparer.OrdinalIgnoreCase);

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
            else if (fileSystemInfo is IFileInfoMock fileInfoMock)
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
                        _fileSystem.Path.GetDirectoryName(
                            file.FullName.TrimEnd(_fileSystem.Path
                               .DirectorySeparatorChar));
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
        public IDirectoryInfoMock? GetDirectory(string path)
        {
            if (_files.TryGetValue(
                _fileSystem.Path.GetFullPath(path).NormalizeAndTrimPath(_fileSystem),
                out FileSystemInfoMock? fileInfo))
            {
                return fileInfo as IDirectoryInfoMock;
            }

            return null;
        }

        /// <inheritdoc cref="FileSystemMock.IStorage.GetFileSystemInfo(string)" />
        public IFileSystemInfoMock? GetFileSystemInfo(string path)
        {
            if (_files.TryGetValue(
                _fileSystem.Path.GetFullPath(path).NormalizeAndTrimPath(_fileSystem),
                out FileSystemInfoMock? fileInfo))
            {
                return fileInfo as IFileSystemInfoMock;
            }

            return null;
        }

        /// <inheritdoc cref="FileSystemMock.IStorage.TryAddFile(string, out FileSystemMock.IStorage.IFileInfoMock?)" />
        public bool TryAddFile(string path,
                               [NotNullWhen(true)] out IFileInfoMock? createdFile)
        {
            ChangeDescription? fileSystemChange = null;

            var location = InMemoryLocation.New(_fileSystem, path);
            _containers.GetOrAdd(
                location,
                InMemoryContainer.NewFile(location, _fileSystem));

            createdFile = _files.GetOrAdd(
                _fileSystem.Path.GetFullPath(path).NormalizeAndTrimPath(_fileSystem),
                _ =>
                {
                    FileSystemInfoMock fileMock = CreateFileInternal(path);
                    IDisposable access =
                        fileMock.RequestAccess(FileAccess.Write, FileShare.ReadWrite);
                    fileSystemChange = _fileSystem.ChangeHandler.NotifyPendingChange(
                        fileMock.FullName,
                        ChangeTypes.FileCreated,
                        NotifyFilters.CreationTime);
                    access.Dispose();
                    return fileMock;
                }) as IFileInfoMock;
            if (fileSystemChange != null && createdFile != null)
            {
                _fileSystem.ChangeHandler.NotifyCompletedChange(fileSystemChange);
                return true;
            }

            createdFile = null;
            return false;
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
        public IFileInfoMock? GetFile(string path)
        {
            if (_files.TryGetValue(
                _fileSystem.Path.GetFullPath(path).NormalizeAndTrimPath(_fileSystem),
                out FileSystemInfoMock? fileInfo))
            {
                return fileInfo as IFileInfoMock;
            }

            return null;
        }

        /// <inheritdoc cref="FileSystemMock.IStorage.GetOrAddDirectory(string)" />
        public IDirectoryInfoMock? GetOrAddDirectory(string path)
        {
            ChangeDescription? fileSystemChange = null;

            var location = InMemoryLocation.New(_fileSystem, path);
            _containers.GetOrAdd(
                location,
                InMemoryContainer.NewDirectory(location, _fileSystem));

            IDirectoryInfoMock? directory = _files.GetOrAdd(
                _fileSystem.Path.GetFullPath(path).NormalizeAndTrimPath(_fileSystem),
                _ =>
                {
                    FileSystemInfoMock directoryMock = CreateDirectoryInternal(path);
                    IDisposable access =
                        directoryMock.RequestAccess(FileAccess.Write,
                            FileShare.ReadWrite);
                    fileSystemChange = _fileSystem.ChangeHandler.NotifyPendingChange(
                        directoryMock.FullName,
                        ChangeTypes.DirectoryCreated,
                        NotifyFilters.CreationTime);
                    access.Dispose();
                    return directoryMock;
                }) as IDirectoryInfoMock;
            _fileSystem.ChangeHandler.NotifyCompletedChange(fileSystemChange);
            return directory;
        }

        /// <inheritdoc cref="FileSystemMock.IStorage.GetOrAddFile(string)" />
        public IFileInfoMock? GetOrAddFile(string path)
        {
            ChangeDescription? fileSystemChange = null;

            var location = InMemoryLocation.New(_fileSystem, path);
            _containers.GetOrAdd(
                location,
                InMemoryContainer.NewFile(location, _fileSystem));

            IFileInfoMock? file = _files.GetOrAdd(
                _fileSystem.Path.GetFullPath(path).NormalizeAndTrimPath(_fileSystem),
                _ =>
                {
                    FileSystemInfoMock fileMock = CreateFileInternal(path);
                    IDisposable access =
                        fileMock.RequestAccess(FileAccess.Write, FileShare.ReadWrite);
                    fileSystemChange = _fileSystem.ChangeHandler.NotifyPendingChange(
                        fileMock.FullName,
                        ChangeTypes.FileCreated,
                        NotifyFilters.CreationTime);
                    access.Dispose();
                    return fileMock;
                }) as IFileInfoMock;
            _fileSystem.ChangeHandler.NotifyCompletedChange(fileSystemChange);
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
            TimeAdjustments timeAdjustments =
                TimeAdjustments.LastWriteTime;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                timeAdjustments |= TimeAdjustments.LastAccessTime;
            }

            List<IDisposable> requests = new();
            foreach (string? parentPath in parents)
            {
                string key = _fileSystem.Path.GetFullPath(parentPath)
                   .NormalizeAndTrimPath(_fileSystem);
                ChangeDescription? fileSystemChange = null;
                FileSystemInfoMock directory = _files.AddOrUpdate(
                    key,
                    _ =>
                    {
                        fileSystemChange = _fileSystem.ChangeHandler.NotifyPendingChange(
                            parentPath,
                            ChangeTypes.DirectoryCreated,
                            NotifyFilters.CreationTime);
                        return DirectoryInfoMock.New(parentPath, _fileSystem);
                    },
                    (_, fileSystemInfo) =>
                        fileSystemInfo.AdjustTimes(timeAdjustments));
                _fileSystem.ChangeHandler.NotifyCompletedChange(fileSystemChange);
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

        public IStorageContainer GetContainer(InMemoryLocation location)
        {
            if (_containers.TryGetValue(location, out var container))
            {
                return container;
            }

            return NullContainer.Instance;
        }

        private class NullContainer : IStorageContainer
        {
            /// <summary>
            ///     The default time returned by the file system if no time has been set.
            ///     <seealso href="https://learn.microsoft.com/en-us/windows/win32/sysinfo/file-times" />:
            ///     A file time is a 64-bit value that represents the number of 100-nanosecond intervals that have elapsed
            ///     since 12:00 A.M. January 1, 1601 Coordinated Universal Time (UTC).
            /// </summary>
            private static readonly DateTime NullTime =
                new DateTime(1601, 01, 01, 00, 00, 00, DateTimeKind.Utc);

            public static IStorageContainer Instance => new NullContainer();

            /// <inheritdoc cref="IStorageContainer.LinkTarget" />
            public string? LinkTarget
            {
                get => null;
                set => _ = value;
            }

            /// <inheritdoc cref="IStorageContainer.Attributes" />
            public FileAttributes Attributes
            {
                get => (FileAttributes) (-1);
                set => _ = value;
            }

            /// <inheritdoc cref="IStorageContainer.CreationTime" />
            public DateTime CreationTime
            {
                get => NullTime;
                set => _ = value;
            }

            /// <inheritdoc cref="IStorageContainer.LastAccessTime" />
            public DateTime LastAccessTime
            {
                get => NullTime;
                set => _ = value;
            }

            /// <inheritdoc cref="IStorageContainer.LastWriteTime" />
            public DateTime LastWriteTime
            {
                get => NullTime;
                set => _ = value;
            }

            /// <inheritdoc cref="IStorageContainer.AdjustTimes(TimeAdjustments)" />
            public void AdjustTimes(TimeAdjustments timeAdjustments)
            {
                // Ignore in NullContainer
            }
        }
    }
}