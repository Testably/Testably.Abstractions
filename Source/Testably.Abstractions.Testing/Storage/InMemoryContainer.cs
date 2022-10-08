using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing.Storage;

internal class InMemoryContainer : IStorageContainer
{
    private FileAttributes _attributes;
    private byte[] _bytes = Array.Empty<byte>();
    private DateTime _creationTime;
    private readonly ConcurrentDictionary<Guid, FileHandle> _fileHandles = new();
    private readonly FileSystemMock _fileSystem;
    private bool _isEncrypted;
    private DateTime _lastAccessTime;
    private DateTime _lastWriteTime;
    private readonly IStorageLocation _location;

    public InMemoryContainer(ContainerType type,
                             IStorageLocation location,
                             FileSystemMock fileSystem)
    {
        _location = location;
        _fileSystem = fileSystem;
        Type = type;
        this.AdjustTimes(TimeAdjustments.All);
    }

    #region IStorageContainer Members

    /// <inheritdoc cref="IStorageContainer.Attributes" />
    public FileAttributes Attributes
    {
        get
        {
            FileAttributes attributes = _attributes;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) &&
                Path.GetFileName(_location.FullPath).StartsWith("."))
            {
                attributes |= FileAttributes.Hidden;
            }

#if FEATURE_FILESYSTEM_LINK
            if (LinkTarget != null)
            {
                attributes |= FileAttributes.ReparsePoint;
            }
#endif

            if (_isEncrypted)
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

    /// <inheritdoc cref="IStorageContainer.CreationTime" />
    public DateTime CreationTime
    {
        get => _creationTime.ToLocalTime();
        set => _creationTime = ConsiderUnspecifiedKind(value);
    }

    /// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
    public IFileSystem FileSystem => _fileSystem;

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

    /// <inheritdoc cref="IStorageContainer.LinkTarget" />
    public string? LinkTarget { get; set; }

    /// <inheritdoc cref="ITimeSystem.ITimeSystemExtensionPoint.TimeSystem" />
    public ITimeSystem TimeSystem => _fileSystem.TimeSystem;

    /// <inheritdoc cref="IStorageContainer.Type" />
    public ContainerType Type { get; }

    /// <inheritdoc cref="IStorageContainer.AppendBytes(byte[])" />
    public void AppendBytes(byte[] bytes)
    {
        WriteBytes(_bytes.Concat(bytes).ToArray());
    }

    /// <inheritdoc cref="IStorageContainer.ClearBytes()" />
    public void ClearBytes()
    {
        _location.Drive?.ChangeUsedBytes(0 - _bytes.Length);
        _bytes = Array.Empty<byte>();
    }

    /// <inheritdoc cref="IStorageContainer.Decrypt()" />
    public void Decrypt()
    {
        if (!_isEncrypted)
        {
            return;
        }

        using (RequestAccess(FileAccess.Write, FileShare.Read))
        {
            _isEncrypted = false;
            WriteBytes(EncryptionHelper.Decrypt(GetBytes()));
        }
    }

    /// <inheritdoc cref="IStorageContainer.Encrypt()" />
    public void Encrypt()
    {
        if (_isEncrypted)
        {
            return;
        }

        using (RequestAccess(FileAccess.Write, FileShare.Read))
        {
            _isEncrypted = true;
            WriteBytes(EncryptionHelper.Encrypt(GetBytes()));
        }
    }

    /// <inheritdoc cref="IStorageContainer.GetBytes()" />
    public byte[] GetBytes() => _bytes;

    /// <inheritdoc cref="IStorageContainer.RequestAccess(FileAccess, FileShare)" />
    public IStorageAccessHandle RequestAccess(FileAccess access, FileShare share)
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

    /// <inheritdoc cref="IStorageContainer.WriteBytes(byte[])" />
    public void WriteBytes(byte[] bytes)
    {
        _location.Drive?.ChangeUsedBytes(bytes.Length - _bytes.Length);
        _bytes = bytes;
    }

    #endregion

    /// <summary>
    ///     Create a new directory on the <paramref name="location" />.
    /// </summary>
    public static IStorageContainer NewDirectory(IStorageLocation location,
                                                 FileSystemMock fileSystem)
    {
        return new InMemoryContainer(ContainerType.Directory, location, fileSystem);
    }

    /// <summary>
    ///     Create a new file on the <paramref name="location" />.
    /// </summary>
    public static IStorageContainer NewFile(IStorageLocation location,
                                            FileSystemMock fileSystem)
    {
        return new InMemoryContainer(ContainerType.File, location, fileSystem);
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

    private void ReleaseAccess(Guid guid)
    {
        _fileHandles.TryRemove(guid, out _);
    }

    private sealed class FileHandle : IStorageAccessHandle
    {
        private readonly Guid _key;
        private readonly Action<Guid> _releaseCallback;

        public FileHandle(Guid key, Action<Guid> releaseCallback, FileAccess access,
                          FileShare share)
        {
            _releaseCallback = releaseCallback;
            Access = access;
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Share = share == FileShare.None
                    ? FileShare.None
                    : FileShare.ReadWrite;
            }
            else
            {
                Share = share;
            }

            _key = key;
        }

        #region IStorageAccessHandle Members

        /// <inheritdoc cref="IStorageAccessHandle.Access" />
        public FileAccess Access { get; }

        /// <inheritdoc cref="IStorageAccessHandle.Share" />
        public FileShare Share { get; }

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

            return CheckAccessWithShare(access, Share) &&
                   CheckAccessWithShare(Access, share);
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