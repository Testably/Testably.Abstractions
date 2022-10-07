using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    private class FileSystemInfoMock : IFileSystem.IFileSystemInfo
    {
        private readonly InMemoryLocation _location;
        protected readonly FileSystemMock FileSystem;
        private bool _isInitialized;
        protected IStorage.IFileSystemInfoMock? Container;
        protected string OriginalPath => _location.FriendlyName;
        protected bool IsEncrypted;

        private readonly ConcurrentDictionary<Guid, FileHandle> _fileHandles = new();
        private readonly IStorageContainer _container;

        /// <summary>
        ///     The <see cref="Drive" /> in which the <see cref="IFileSystem.IFileSystemInfo" /> is stored.
        /// </summary>
        protected IDriveInfoMock? Drive => _location.Drive;

        internal FileSystemInfoMock(InMemoryLocation location,
                                    FileSystemMock fileSystem)
        {
            _location = location;
            FileSystem = fileSystem;
            _container = fileSystem.Storage.GetContainer(location);
        }

        internal FileSystemInfoMock(string fullName, string originalPath,
                                    FileSystemMock fileSystem)
        :this(InMemoryLocation.New(fileSystem, fullName, originalPath), fileSystem)
        {
        }

        #region IFileSystemInfo Members

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Attributes" />
        public FileAttributes Attributes
        {
            get
            {
                FileAttributes attributes = _attributes;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) &&
                    System.IO.Path.GetFileName(FullName).StartsWith("."))
                {
                    attributes |= FileAttributes.Hidden;
                }

#if FEATURE_FILESYSTEM_LINK
                if (LinkTarget != null)
                {
                    attributes |= FileAttributes.ReparsePoint;
                }
#endif

                if (IsEncrypted)
                {
                    attributes |= FileAttributes.Encrypted;
                }

                if (attributes == 0)
                {
                    attributes = FileAttributes.Normal;
                }

                return attributes;
            }
            set
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    value &= FileAttributes.Directory |
                             FileAttributes.ReadOnly |
                             FileAttributes.Archive |
                             FileAttributes.Hidden |
                             FileAttributes.NoScrubData |
                             FileAttributes.NotContentIndexed |
                             FileAttributes.Offline |
                             FileAttributes.System |
                             FileAttributes.Temporary;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    value &= FileAttributes.Hidden |
                             FileAttributes.Directory |
                             FileAttributes.ReadOnly;
                }
                else
                {
                    value &= FileAttributes.Directory |
                             FileAttributes.ReadOnly;
                }

                _attributes = value;
            }
        }

        private FileAttributes _attributes;

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.CreationTime" />
        public DateTime CreationTime
        {
            get => _container.CreationTime.ToLocalTime();
            set => _container.CreationTime = value;
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.CreationTimeUtc" />
        public DateTime CreationTimeUtc
        {
            get => _container.CreationTime.ToUniversalTime();
            set => _container.CreationTime = value;
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
        public string FullName => _location.FullPath;

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastAccessTime" />
        public DateTime LastAccessTime
        {
            get => _container.LastAccessTime.ToLocalTime();
            set => _container.LastAccessTime = value;
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastAccessTimeUtc" />
        public DateTime LastAccessTimeUtc
        {
            get => _container.LastAccessTime.ToUniversalTime();
            set => _container.LastAccessTime = value;
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastWriteTime" />
        public DateTime LastWriteTime
        {
            get => _container.LastWriteTime.ToLocalTime();
            set => _container.LastWriteTime = value;
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastWriteTimeUtc" />
        public DateTime LastWriteTimeUtc
        {
            get => _container.LastWriteTime.ToUniversalTime();
            set => _container.LastWriteTime = value;
        }

#if FEATURE_FILESYSTEM_LINK
        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LinkTarget" />
        public string? LinkTarget
        {
            get
            {
                RefreshInternal();
                return (Container as FileSystemInfoMock)?._linkTarget;
            }
            protected set => _linkTarget = value;
        }

        private string? _linkTarget;
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
                throw ExceptionFactory.CannotCreateFileAsAlreadyExists(OriginalPath);
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
        public IDisposable RequestAccess(FileAccess access, FileShare share)
        {
            if (Drive == null)
            {
                throw ExceptionFactory.DirectoryNotFound(FullName);
            }

            if (!Drive.IsReady)
            {
                throw ExceptionFactory.NetworkPathNotFound(FullName);
            }

            if (CanGetAccess(access, share))
            {
                Guid guid = Guid.NewGuid();
                FileHandle fileHandle = new(guid, ReleaseAccess, access, share);
                _fileHandles.TryAdd(guid, fileHandle);
                return fileHandle;
            }

            throw ExceptionFactory.ProcessCannotAccessTheFile(FullName);
        }

        private void ReleaseAccess(Guid guid)
        {
            _fileHandles.TryRemove(guid, out _);
        }

        private bool CanGetAccess(FileAccess access, FileShare share)
        {
            foreach (KeyValuePair<Guid, FileHandle> fileHandle in _fileHandles)
            {
                if (!fileHandle.Value.GrantAccess(access, share))
                {
                    return false;
                }
            }

            return true;
        }

        private sealed class FileHandle : IDisposable
        {
            private readonly Action<Guid> _releaseCallback;
            private readonly FileAccess _access;
            private readonly FileShare _share;
            private readonly Guid _key;

            public FileHandle(Guid key, Action<Guid> releaseCallback, FileAccess access,
                              FileShare share)
            {
                _releaseCallback = releaseCallback;
                _access = access;
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    _share = share == FileShare.None
                        ? FileShare.None
                        : FileShare.ReadWrite;
                }
                else
                {
                    _share = share;
                }

                _key = key;
            }

            public bool GrantAccess(FileAccess access, FileShare share)
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    share = FileShare.ReadWrite;
                }

                return CheckAccessWithShare(access, _share) &&
                       CheckAccessWithShare(_access, share);
            }

            private static bool CheckAccessWithShare(FileAccess access, FileShare share)
            {
                switch (access)
                {
                    case FileAccess.Read:
                        return share.HasFlag(FileShare.Read);
                    case FileAccess.Write:
                        return share.HasFlag(FileShare.Write);
                    default:
                        return share == FileShare.ReadWrite;
                }
            }

            /// <inheritdoc cref="IDisposable.Dispose()" />
            public void Dispose()
            {
                _releaseCallback.Invoke(_key);
            }
        }

#if NETSTANDARD2_0
        /// <inheritdoc cref="object.ToString()" />
#else
        /// <inheritdoc cref="System.IO.FileSystemInfo.ToString()" />
#endif
        public override string ToString()
            => OriginalPath;

        internal FileSystemInfoMock AdjustTimes(TimeAdjustments timeAdjustments)
        {
            ChangeDescription? fileSystemChange = null;
            if (HasNotifyFilters(timeAdjustments, out NotifyFilters notifyFilters))
            {
                fileSystemChange = FileSystem.ChangeHandler.NotifyPendingChange(
                    FullName,
                    ChangeTypes.Modified,
                    notifyFilters);
            }

            _container.AdjustTimes(timeAdjustments);

            FileSystem.ChangeHandler.NotifyCompletedChange(fileSystemChange);

            return this;
        }

        private static bool HasNotifyFilters(TimeAdjustments timeAdjustments,
                                             out NotifyFilters notifyFilters)
        {
            notifyFilters = 0;
            if (timeAdjustments.HasFlag(TimeAdjustments.CreationTime))
            {
                notifyFilters |= NotifyFilters.CreationTime;
            }

            if (timeAdjustments.HasFlag(TimeAdjustments.LastAccessTime))
            {
                notifyFilters |= NotifyFilters.LastAccess;
            }

            if (timeAdjustments.HasFlag(TimeAdjustments.LastWriteTime))
            {
                notifyFilters |= NotifyFilters.LastWrite;
            }

            return notifyFilters > 0;
        }

        protected void RefreshInternal()
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;
            Container = FileSystem.Storage.GetFile(FullName);
        }
    }
}