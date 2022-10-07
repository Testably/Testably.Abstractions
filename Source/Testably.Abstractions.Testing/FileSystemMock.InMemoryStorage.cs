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
        private readonly ConcurrentDictionary<InMemoryLocation, IStorageContainer>
            _containers = new();

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

        public string CurrentDirectory { get; set; } = string.Empty.PrefixRoot();
        

        public IEnumerable<InMemoryLocation> EnumerateLocations(
            InMemoryLocation location,
            InMemoryContainer.ContainerType type,
            string expression,
            EnumerationOptions enumerationOptions,
            Func<Exception> notFoundException)
        {
            ValidateExpression(expression);
            if (!_containers.ContainsKey(location))
            {
                throw notFoundException();
            }

            foreach (KeyValuePair<InMemoryLocation, IStorageContainer> item in _containers
               .Where(x => x.Key.FullPath.StartsWith(location.FullPath) &&
                           !x.Key.Equals(location)))
            {
                string? parentPath =
                    _fileSystem.Path.GetDirectoryName(
                        item.Key.FullPath.TrimEnd(_fileSystem.Path
                           .DirectorySeparatorChar));
                if (!enumerationOptions.RecurseSubdirectories &&
                    parentPath != location.FullPath)
                {
                    continue;
                }

                if (!EnumerationOptionsHelper.MatchesPattern(enumerationOptions,
                    _fileSystem.Path.GetFileName(item.Key.FullPath), expression))
                {
                    continue;
                }

                if (type.HasFlag(item.Value.Type))
                {
                    yield return item.Key;
                }
            }
        }

#if FEATURE_FILESYSTEM_LINK
        public InMemoryLocation? ResolveLinkTarget(InMemoryLocation location,
                                                   bool returnFinalTarget = false)
        {
            if (_containers.TryGetValue(location,
                    out IStorageContainer? initialContainer) &&
                initialContainer.LinkTarget != null)
            {
                InMemoryLocation nextLocation =
                    InMemoryLocation.New(_fileSystem, initialContainer.LinkTarget);
                if (_containers.TryGetValue(nextLocation,
                    out IStorageContainer? container))
                {
                    if (returnFinalTarget)
                    {
                        int maxResolveLinks =
                            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                                ? 63
                                : 40;
                        for (int i = 1; i < maxResolveLinks; i++)
                        {
                            if (container.LinkTarget != null)
                            {
                                nextLocation = InMemoryLocation.New(_fileSystem,
                                    container.LinkTarget);
                                if (!_containers.TryGetValue(nextLocation,
                                    out IStorageContainer? nextContainer))
                                {
                                    return nextLocation;
                                }

                                container = nextContainer;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (container.LinkTarget != null)
                        {
                            throw ExceptionFactory.FileNameCannotBeResolved(
                                location.FullPath);
                        }

                        ;
                    }

                    return nextLocation;
                }

                return nextLocation;
            }

            return null;
        }
#endif

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

        public IDriveInfoMock GetOrAddDrive(string driveName)
        {
            DriveInfoMock drive = new(driveName, _fileSystem);
            return _drives.GetOrAdd(drive.Name, _ => drive);
        }

        public IEnumerable<IDriveInfoMock> GetDrives()
            => _drives.Values;
        
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

        private FileSystemInfoMock CreateFileInternal(string path)
        {
            return FileInfoMock.New(InMemoryLocation.New(_fileSystem, path), _fileSystem);
        }

        private static void ValidateExpression(string expression)
        {
            if (expression.Contains('\0'))
            {
                throw ExceptionFactory.PathHasIllegalCharacters(expression);
            }
        }

        [return: NotNullIfNotNull("location")]
        public IStorageContainer? GetContainer(InMemoryLocation? location)
        {
            if (location == null)
            {
                return null;
            }

            if (_containers.TryGetValue(location, out IStorageContainer? container))
            {
                return container;
            }

            return InMemoryContainer.Null;
        }

        public IStorageContainer GetOrCreateContainer(
            InMemoryLocation location,
            Func<InMemoryLocation, FileSystemMock, IStorageContainer> containerGenerator)
        {
            ChangeDescription? fileSystemChange = null;
            IStorageContainer container = _containers.GetOrAdd(location,
                loc =>
                {
                    IStorageContainer container =
                        containerGenerator.Invoke(loc, _fileSystem);
                    if (container.Type == InMemoryContainer.ContainerType.Directory)
                    {
                        CreateParents(_fileSystem, loc);
                        IStorageAccessHandle access =
                            container.RequestAccess(FileAccess.Write,
                                FileShare.ReadWrite);
                        fileSystemChange = _fileSystem.ChangeHandler.NotifyPendingChange(
                            location.FullPath,
                            ChangeTypes.DirectoryCreated,
                            NotifyFilters.CreationTime);
                        access.Dispose();
                    }
                    else
                    {
                        IStorageAccessHandle access =
                            container.RequestAccess(FileAccess.Write,
                                FileShare.ReadWrite);
                        fileSystemChange = _fileSystem.ChangeHandler.NotifyPendingChange(
                            location.FullPath,
                            ChangeTypes.FileCreated,
                            NotifyFilters.CreationTime);
                        access.Dispose();
                    }

                    return container;
                });
            _fileSystem.ChangeHandler.NotifyCompletedChange(fileSystemChange);
            return container;
        }

        private void CreateParents(FileSystemMock fileSystem, InMemoryLocation location)
        {
            List<string> parents = new();
            string? parent = fileSystem.Path.GetDirectoryName(
                location.FullPath.TrimEnd(fileSystem.Path.DirectorySeparatorChar,
                    fileSystem.Path.AltDirectorySeparatorChar));
            while (!string.IsNullOrEmpty(parent))
            {
                parents.Add(parent);
                parent = fileSystem.Path.GetDirectoryName(parent);
            }

            parents.Reverse();
            IStorageContainer.TimeAdjustments timeAdjustments =
                IStorageContainer.TimeAdjustments.LastWriteTime;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                timeAdjustments |= IStorageContainer.TimeAdjustments.LastAccessTime;
            }

            List<IStorageAccessHandle> requests = new();
            try
            {
                foreach (string? parentPath in parents)
                {
                    ChangeDescription? fileSystemChange = null;
                    InMemoryLocation parentLocation =
                        InMemoryLocation.New(_fileSystem, parentPath);
                    IStorageContainer parentContainer = _containers.AddOrUpdate(
                        parentLocation,
                        loc =>
                        {
                            IStorageContainer container =
                                InMemoryContainer.NewDirectory(loc, _fileSystem);

                            requests.Add(container.RequestAccess(FileAccess.Write,
                                FileShare.ReadWrite));
                            fileSystemChange =
                                fileSystem.ChangeHandler.NotifyPendingChange(
                                    parentPath,
                                    ChangeTypes.DirectoryCreated,
                                    NotifyFilters.CreationTime);
                            return container;
                        },
                        (_, f) =>
                        {
                            f.AdjustTimes(timeAdjustments);
                            return f;
                        });
                    fileSystem.ChangeHandler.NotifyCompletedChange(fileSystemChange);
                }
            }
            finally
            {
                foreach (IStorageAccessHandle request in requests)
                {
                    request.Dispose();
                }
            }
        }

        public bool DeleteContainer(InMemoryLocation location, bool recursive = false)
        {
            if (!_containers.TryGetValue(location, out IStorageContainer? container))
            {
                return false;
            }

            if (container.Type == InMemoryContainer.ContainerType.Directory)
            {
                string start = location.FullPath +
                               _fileSystem.Path.DirectorySeparatorChar;
                if (recursive)
                {
                    foreach (InMemoryLocation key in _containers.Keys.Where(x
                        => x.FullPath.StartsWith(start)))
                    {
                        if (_containers.TryRemove(key,
                            out IStorageContainer? removedChild))
                        {
                            removedChild.ClearBytes();
                        }
                    }
                }
                else if (_containers.Keys.Any(x => x.FullPath.StartsWith(start)))
                {
                    throw ExceptionFactory.DirectoryNotEmpty(location.FullPath);
                }
            }

            if (_containers.TryRemove(location, out IStorageContainer? removed))
            {
                removed.ClearBytes();
                return true;
            }

            return false;
        }

        public bool TryAddContainer(InMemoryLocation location,
                                    InMemoryContainer.ContainerType containerType,
                                    [NotNullWhen(true)] out IStorageContainer? container)
        {
            ChangeDescription? fileSystemChange = null;

            container = _containers.GetOrAdd(
                location,
                _ =>
                {
                    IStorageContainer container = InMemoryContainer.New(
                        containerType,
                        location, _fileSystem);
                    IDisposable access =
                        container.RequestAccess(FileAccess.Write, FileShare.ReadWrite);
                    fileSystemChange = _fileSystem.ChangeHandler.NotifyPendingChange(
                        location.FullPath,
                        ChangeTypes.FileCreated,
                        NotifyFilters.CreationTime);
                    access.Dispose();
                    return container;
                });

            if (fileSystemChange != null)
            {
                _fileSystem.ChangeHandler.NotifyCompletedChange(fileSystemChange);
                return true;
            }

            container = null;
            return false;
        }
    }
}