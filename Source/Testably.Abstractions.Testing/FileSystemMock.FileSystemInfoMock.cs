using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    private class FileSystemInfoMock : IFileSystem.IFileSystemInfo
    {
        protected readonly FileSystemMock FileSystem;
        private DateTime _creationTime;
        private DateTime _lastAccessTime;
        private DateTime _lastWriteTime;
        protected readonly string OriginalPath;

        private readonly ConcurrentDictionary<Guid, FileHandle> _fileHandles = new();

        /// <summary>
        ///     The <see cref="Drive" /> in which the <see cref="IFileSystem.IFileSystemInfo" /> is stored.
        /// </summary>
        protected IDriveInfoMock? Drive { get; }

        internal FileSystemInfoMock(string fullName, string originalPath,
                                    FileSystemMock fileSystem)
        {
            FullName = fullName;
            OriginalPath = originalPath.RemoveLeadingDot();
            if (Framework.IsNetFramework)
            {
                OriginalPath = OriginalPath.TrimOnWindows();
            }

            FileSystem = fileSystem;
            AdjustTimes(TimeAdjustments.All);
            if (string.IsNullOrEmpty(fullName))
            {
                Drive = FileSystem.Storage.GetDrives().First();
            }
            else
            {
                Drive = fileSystem.Storage.GetDrive(
                    fileSystem.Path.GetPathRoot(fullName));
            }
        }

        #region IFileSystemInfo Members

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Attributes" />
        public FileAttributes Attributes { get; set; }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.CreationTime" />
        public DateTime CreationTime
        {
            get => _creationTime.ToLocalTime();
            set => _creationTime = ConsiderUnspecifiedKind(value);
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.CreationTimeUtc" />
        public DateTime CreationTimeUtc
        {
            get => _creationTime.ToUniversalTime();
            set => _creationTime = ConsiderUnspecifiedKind(value);
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
        public string FullName { get; }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastAccessTime" />
        public DateTime LastAccessTime
        {
            get => _lastAccessTime.ToLocalTime();
            set => _lastAccessTime = ConsiderUnspecifiedKind(value);
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastAccessTimeUtc" />
        public DateTime LastAccessTimeUtc
        {
            get => _lastAccessTime.ToUniversalTime();
            set => _lastAccessTime = ConsiderUnspecifiedKind(value);
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastWriteTime" />
        public DateTime LastWriteTime
        {
            get => _lastWriteTime.ToLocalTime();
            set => _lastWriteTime = ConsiderUnspecifiedKind(value);
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastWriteTimeUtc" />
        public DateTime LastWriteTimeUtc
        {
            get => _lastWriteTime.ToUniversalTime();
            set => _lastWriteTime = ConsiderUnspecifiedKind(value);
        }

#if FEATURE_FILESYSTEM_LINK
        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LinkTarget" />
        public string? LinkTarget
            => throw new NotImplementedException();
#endif

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Name" />
        public string Name
            => FileSystem.Path.GetFileName(FullName.TrimEnd(
                FileSystem.Path.DirectorySeparatorChar,
                FileSystem.Path.AltDirectorySeparatorChar));

#if FEATURE_FILESYSTEM_LINK
        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.CreateAsSymbolicLink(string)" />
        public void CreateAsSymbolicLink(string pathToTarget)
            => throw new NotImplementedException();
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
            => throw new NotImplementedException();
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

            DateTime now = FileSystem.TimeSystem.DateTime.UtcNow;
            if (timeAdjustments.HasFlag(TimeAdjustments.CreationTime))
            {
                CreationTime = ConsiderUnspecifiedKind(now);
            }

            if (timeAdjustments.HasFlag(TimeAdjustments.LastAccessTime))
            {
                LastAccessTime = ConsiderUnspecifiedKind(now);
            }

            if (timeAdjustments.HasFlag(TimeAdjustments.LastWriteTime))
            {
                LastWriteTime = ConsiderUnspecifiedKind(now);
            }

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

        private static DateTime ConsiderUnspecifiedKind(
            DateTime time,
            DateTimeKind targetKind = DateTimeKind.Utc)
        {
            if (time.Kind == DateTimeKind.Unspecified)
            {
                return DateTime.SpecifyKind(time, targetKind);
            }

            if (targetKind == DateTimeKind.Local)
            {
                return time.ToLocalTime();
            }

            return time.ToUniversalTime();
        }

        /// <summary>
        ///     Flags indicating which times to adjust for a <see cref="FileSystemMock.FileSystemInfoMock" />.
        /// </summary>
        /// .
        [Flags]
        internal enum TimeAdjustments
        {
            /// <summary>
            ///     Adjusts no times on the <see cref="FileSystemMock.FileSystemInfoMock" />
            /// </summary>
            None = 0,

            /// <summary>
            ///     Adjusts the <see cref="FileSystemMock.FileSystemInfoMock.CreationTime" />
            /// </summary>
            CreationTime = 1 << 0,

            /// <summary>
            ///     Adjusts the <see cref="FileSystemMock.FileSystemInfoMock.LastAccessTime" />
            /// </summary>
            LastAccessTime = 1 << 1,

            /// <summary>
            ///     Adjusts the <see cref="FileSystemMock.FileSystemInfoMock.LastWriteTime" />
            /// </summary>
            LastWriteTime = 1 << 2,

            /// <summary>
            ///     Adjusts all times on the <see cref="FileSystemMock.FileSystemInfoMock" />
            /// </summary>
            All = CreationTime | LastAccessTime | LastWriteTime,
        }
    }
}