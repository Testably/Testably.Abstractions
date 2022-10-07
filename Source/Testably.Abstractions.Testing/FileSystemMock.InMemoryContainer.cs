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
    internal interface IStorageContainer
    {
        DateTime CreationTime
        {
            get;
            set;
        }

        public DateTime LastAccessTime
        {
            get;
            set;
        }

        public DateTime LastWriteTime
        {
            get;
            set;
        }

        void AdjustTimes(TimeAdjustments timeAdjustments);
    }

    internal class InMemoryContainer : IStorageContainer
    {
        public string? LinkTarget { get; set; }

        public ContainerType Type { get; }

        private byte[] _bytes = Array.Empty<byte>();

        private DateTime _creationTime;

        private readonly ConcurrentDictionary<Guid, FileHandle> _fileHandles = new();
        private readonly FileSystemMock _fileSystem;
        private DateTime _lastAccessTime;
        private DateTime _lastWriteTime;
        private readonly InMemoryLocation _location;

        public InMemoryContainer(ContainerType type,
                                 InMemoryLocation location,
                                 FileSystemMock fileSystem)
        {
            _location = location;
            _fileSystem = fileSystem;
            Type = type;
            AdjustTimes(TimeAdjustments.All);
        }

        #region IStorageContainer Members

        /// <inheritdoc cref="IStorageContainer.CreationTime" />
        public DateTime CreationTime
        {
            get => _creationTime.ToLocalTime();
            set => _creationTime = ConsiderUnspecifiedKind(value);
        }

        /// <inheritdoc cref="IStorageContainer.LastAccessTime" />
        public DateTime LastAccessTime
        {
            get => _lastAccessTime.ToLocalTime();
            set => _lastAccessTime = ConsiderUnspecifiedKind(value);
        }

        /// <inheritdoc cref="IStorageContainer.LastWriteTime" />
        public DateTime LastWriteTime
        {
            get => _lastWriteTime.ToLocalTime();
            set => _lastWriteTime = ConsiderUnspecifiedKind(value);
        }

        /// <inheritdoc cref="IStorageContainer.AdjustTimes(TimeAdjustments)" />
        public void AdjustTimes(TimeAdjustments timeAdjustments)
        {
            DateTime now = _fileSystem.TimeSystem.DateTime.UtcNow;
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
        }

        #endregion

        /// <inheritdoc cref="IStorage.IFileInfoMock.AppendBytes(byte[])" />
        public void AppendBytes(byte[] bytes)
        {
            WriteBytes(_bytes.Concat(bytes).ToArray());
        }

        /// <inheritdoc cref="IStorage.IFileInfoMock.ClearBytes()" />
        public void ClearBytes()
        {
            _location.Drive?.ChangeUsedBytes(0 - _bytes.Length);
            _bytes = Array.Empty<byte>();
        }

        /// <inheritdoc cref="IStorage.IFileInfoMock.GetBytes()" />
        public byte[] GetBytes() => _bytes;

        public static InMemoryContainer NewDirectory(InMemoryLocation location,
                                                     FileSystemMock fileSystem)
        {
            return new InMemoryContainer(ContainerType.Directory, location, fileSystem);
        }

        public static InMemoryContainer NewFile(InMemoryLocation location,
                                                FileSystemMock fileSystem)
        {
            return new InMemoryContainer(ContainerType.File, location, fileSystem);
        }

        /// <inheritdoc cref="IStorage.IFileSystemInfoMock.RequestAccess(FileAccess, FileShare)" />
        public IDisposable RequestAccess(FileAccess access, FileShare share)
        {
            if (_location.Drive == null)
            {
                throw ExceptionFactory.DirectoryNotFound(_location.FullPath);
            }

            if (!_location.Drive.IsReady)
            {
                throw ExceptionFactory.NetworkPathNotFound(_location.FullPath);
            }

            if (CanGetAccess(access, share))
            {
                Guid guid = Guid.NewGuid();
                FileHandle fileHandle = new(guid, ReleaseAccess, access, share);
                _fileHandles.TryAdd(guid, fileHandle);
                return fileHandle;
            }

            throw ExceptionFactory.ProcessCannotAccessTheFile(_location.FullPath);
        }

        /// <inheritdoc cref="IStorage.IFileInfoMock.WriteBytes(byte[])" />
        public void WriteBytes(byte[] bytes)
        {
            _location.Drive?.ChangeUsedBytes(bytes.Length - _bytes.Length);
            _bytes = bytes;
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

        private void ReleaseAccess(Guid guid)
        {
            _fileHandles.TryRemove(guid, out _);
        }

        public enum ContainerType
        {
            Directory,
            File,
            Unknown
        }

        private sealed class FileHandle : IDisposable
        {
            private readonly FileAccess _access;
            private readonly Guid _key;
            private readonly Action<Guid> _releaseCallback;
            private readonly FileShare _share;

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

            #region IDisposable Members

            /// <inheritdoc cref="IDisposable.Dispose()" />
            public void Dispose()
            {
                _releaseCallback.Invoke(_key);
            }

            #endregion

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
        }
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