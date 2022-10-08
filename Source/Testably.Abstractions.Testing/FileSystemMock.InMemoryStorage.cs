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
    /// <summary>
    ///     The container storing the current data of the <see cref="IFileSystem" /> in memory.
    /// </summary>
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

        /// <inheritdoc cref="IStorage.CurrentDirectory" />
        public string CurrentDirectory { get; set; } = string.Empty.PrefixRoot();

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

        public IEnumerable<InMemoryLocation> EnumerateLocations(InMemoryLocation location,
            InMemoryContainer.ContainerType type,
            string expression = "*",
            EnumerationOptions? enumerationOptions = null)
        {
            ValidateExpression(expression);
            if (!_containers.ContainsKey(location))
            {
                throw ExceptionFactory.DirectoryNotFound(location.FullPath);
            }

            enumerationOptions ??= EnumerationOptionsHelper.Compatible;

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

        /// <summary>
        ///     Returns the drive if it is present.<br />
        ///     Returns <see langword="null" />, if the drive does not exist.
        /// </summary>
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

        /// <summary>
        ///     Returns the drives that are present.
        /// </summary>
        public IEnumerable<IDriveInfoMock> GetDrives()
            => _drives.Values;

        /// <inheritdoc cref="IStorage.GetLocation(string?, string?)" />
        [return: NotNullIfNotNull("path")]
        public InMemoryLocation? GetLocation(string? path, string? friendlyName = null)
        {
            if (path == null)
            {
                return null;
            }

            IDriveInfoMock? drive = _fileSystem.Storage.GetDrive(
                _fileSystem.Path.GetPathRoot(path));
            if (drive == null &&
                !_fileSystem.Path.IsPathRooted(path))
            {
                drive = _fileSystem.Storage.GetDrives().First();
            }

            return InMemoryLocation.New(drive, _fileSystem.Path.GetFullPath(path), path);
        }

        /// <summary>
        ///     Returns the drives that are present.
        /// </summary>
        public IDriveInfoMock GetOrAddDrive(string driveName)
        {
            DriveInfoMock drive = new(driveName, _fileSystem);
            return _drives.GetOrAdd(drive.Name, _ => drive);
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

#if FEATURE_FILESYSTEM_LINK
        public InMemoryLocation? ResolveLinkTarget(InMemoryLocation location,
                                                   bool returnFinalTarget = false)
        {
            if (_containers.TryGetValue(location,
                    out IStorageContainer? initialContainer) &&
                initialContainer.LinkTarget != null)
            {
                InMemoryLocation nextLocation =
                    _fileSystem.Storage.GetLocation(initialContainer.LinkTarget);
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
                                nextLocation =
                                    _fileSystem.Storage.GetLocation(container.LinkTarget);
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

        public bool TryAddContainer(InMemoryLocation location,
                                    Func<InMemoryLocation, FileSystemMock,
                                        IStorageContainer> containerGenerator,
                                    [NotNullWhen(true)] out IStorageContainer? container)
        {
            ChangeDescription? fileSystemChange = null;

            container = _containers.GetOrAdd(
                location,
                _ =>
                {
                    IStorageContainer container =
                        containerGenerator(location, _fileSystem);
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

        #endregion

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
                        _fileSystem.Storage.GetLocation(parentPath);
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

        private static void ValidateExpression(string expression)
        {
            if (expression.Contains('\0'))
            {
                throw ExceptionFactory.PathHasIllegalCharacters(expression);
            }
        }
    }
}