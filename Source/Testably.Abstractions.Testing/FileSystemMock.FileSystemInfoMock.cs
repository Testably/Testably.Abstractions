using System;
using System.IO;
using System.Runtime.InteropServices;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    private class FileSystemInfoMock : IFileSystem.IFileSystemInfo
    {
        protected readonly InMemoryLocation Location;
        protected readonly FileSystemMock FileSystem;
        protected IStorageContainer Container { get; set; }
        
        internal FileSystemInfoMock(FileSystemMock fileSystem, InMemoryLocation location)
        {
            FileSystem = fileSystem;
            Location = location;
            Container = fileSystem.Storage.GetContainer(location);
        }

        internal FileSystemInfoMock(string fullName, string originalPath,
                                    FileSystemMock fileSystem)
            : this(fileSystem, InMemoryLocation.New(fileSystem, fullName, originalPath))
        {
        }

        #region IFileSystemInfo Members

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Attributes" />
        public FileAttributes Attributes
        {
            get => Container.Attributes;
            set => Container.Attributes = value;
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.CreationTime" />
        public DateTime CreationTime
        {
            get => Container.CreationTime.ToLocalTime();
            set => Container.CreationTime = value;
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.CreationTimeUtc" />
        public DateTime CreationTimeUtc
        {
            get => Container.CreationTime.ToUniversalTime();
            set => Container.CreationTime = value;
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Exists" />
        public bool Exists
        {
            get
            {
                _exists ??= FileSystem.Storage.Exists(FullName);
                return _exists.Value;
            }
        }

        private bool? _exists;

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Extension" />
        public string Extension
            => FileSystem.Path.GetExtension(FullName);

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.FullName" />
        public string FullName => Location.FullPath;

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastAccessTime" />
        public DateTime LastAccessTime
        {
            get => Container.LastAccessTime.ToLocalTime();
            set => Container.LastAccessTime = value;
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastAccessTimeUtc" />
        public DateTime LastAccessTimeUtc
        {
            get => Container.LastAccessTime.ToUniversalTime();
            set => Container.LastAccessTime = value;
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastWriteTime" />
        public DateTime LastWriteTime
        {
            get => Container.LastWriteTime.ToLocalTime();
            set => Container.LastWriteTime = value;
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastWriteTimeUtc" />
        public DateTime LastWriteTimeUtc
        {
            get => Container.LastWriteTime.ToUniversalTime();
            set => Container.LastWriteTime = value;
        }

#if FEATURE_FILESYSTEM_LINK
        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LinkTarget" />
        public string? LinkTarget
        {
            get => Container.LinkTarget;
            protected set => Container.LinkTarget = value;
        }
#endif

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Name" />
        public string Name
            => FileSystem.Path.GetFileName(FullName.TrimEnd(
                FileSystem.Path.DirectorySeparatorChar,
                FileSystem.Path.AltDirectorySeparatorChar));

#if FEATURE_FILESYSTEM_LINK
        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.CreateAsSymbolicLink(string)" />
        public void CreateAsSymbolicLink(string pathToTarget)
        {
            if (FileSystem.Storage.TryAddFile(FullName,
                out IStorage.IFileInfoMock? createdFile))
            {
                createdFile.SetLinkTarget(pathToTarget);
                LinkTarget = pathToTarget;
                Attributes |= FileAttributes.ReparsePoint;
            }
            else
            {
                throw ExceptionFactory.CannotCreateFileAsAlreadyExists(Location
                   .FriendlyName);
            }
        }
#endif

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Refresh()" />
        public void Refresh()
        {
#if !NETFRAMEWORK
            // The DirectoryInfo is not updated in .NET Framework!
            _exists = null;
#endif
        }
        private bool _isInitialized;

        protected void RefreshInternal()
        {
            if (_isInitialized)
            {
                return;
            }

            Container = FileSystem.Storage.GetContainer(Location);
            _isInitialized = true;
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Delete()" />
        public void Delete()
        {
            if (!FileSystem.Storage.Delete(FullName))
            {
                throw ExceptionFactory.DirectoryNotFound(FullName);
            }

            Refresh();
        }

#if FEATURE_FILESYSTEM_LINK
        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.ResolveLinkTarget(bool)" />
        public IFileSystem.IFileSystemInfo? ResolveLinkTarget(bool returnFinalTarget)
        {
            IStorage.IFileSystemInfoMock? self =
                FileSystem.Storage.GetFileSystemInfo(FullName);
            if (returnFinalTarget)
            {
                IFileSystem.IFileSystemInfo? linkTarget = self;
                int maxResolveLinks = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? 63
                    : 40;
                for (int i = 0; i < maxResolveLinks; i++)
                {
                    IFileSystem.IFileSystemInfo? nextLink =
                        linkTarget?.ResolveLinkTarget(false);

                    if (nextLink == null)
                    {
                        return linkTarget;
                    }

                    linkTarget = nextLink;
                }

                if (linkTarget?.LinkTarget != null)
                {
                    throw ExceptionFactory.FileNameCannotBeResolved(FullName);
                }

                return linkTarget;
            }

            if (self?.LinkTarget != null)
            {
                return FileSystem.FileInfo.New(self.LinkTarget) as
                    IStorage.IFileSystemInfoMock;
            }

            return null;
        }
#endif

        #endregion

        /// <inheritdoc cref="IStorage.IFileSystemInfoMock.RequestAccess(FileAccess, FileShare)" />
        public IStorageAccessHandle RequestAccess(FileAccess access, FileShare share)
        {
            return Container.RequestAccess(access, share);
        }

#if NETSTANDARD2_0
        /// <inheritdoc cref="object.ToString()" />
#else
        /// <inheritdoc cref="System.IO.FileSystemInfo.ToString()" />
#endif
        public override string ToString()
            => Location.FriendlyName;

        internal FileSystemInfoMock AdjustTimes(
            IStorageContainer.TimeAdjustments timeAdjustments)
        {
            ChangeDescription? fileSystemChange = null;
            if (HasNotifyFilters(timeAdjustments, out NotifyFilters notifyFilters))
            {
                fileSystemChange = FileSystem.ChangeHandler.NotifyPendingChange(
                    FullName,
                    ChangeTypes.Modified,
                    notifyFilters);
            }

            Container.AdjustTimes(timeAdjustments);

            FileSystem.ChangeHandler.NotifyCompletedChange(fileSystemChange);

            return this;
        }

        private static bool HasNotifyFilters(
            IStorageContainer.TimeAdjustments timeAdjustments,
            out NotifyFilters notifyFilters)
        {
            notifyFilters = 0;
            if (timeAdjustments.HasFlag(IStorageContainer.TimeAdjustments.CreationTime))
            {
                notifyFilters |= NotifyFilters.CreationTime;
            }

            if (timeAdjustments.HasFlag(IStorageContainer.TimeAdjustments.LastAccessTime))
            {
                notifyFilters |= NotifyFilters.LastAccess;
            }

            if (timeAdjustments.HasFlag(IStorageContainer.TimeAdjustments.LastWriteTime))
            {
                notifyFilters |= NotifyFilters.LastWrite;
            }

            return notifyFilters > 0;
        }
    }
}