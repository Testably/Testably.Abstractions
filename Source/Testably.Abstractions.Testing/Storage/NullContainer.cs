using System;
using System.IO;

namespace Testably.Abstractions.Testing.Storage;

internal sealed class NullContainer : IStorageContainer
{
    /// <summary>
    ///     The default time returned by the file system if no time has been set.
    ///     <seealso href="https://learn.microsoft.com/en-us/windows/win32/sysinfo/file-times" />:
    ///     A file time is a 64-bit value that represents the number of 100-nanosecond intervals that have elapsed
    ///     since 12:00 A.M. January 1, 1601 Coordinated Universal Time (UTC).
    /// </summary>
    private static readonly DateTime NullTime =
        new(1601, 01, 01, 00, 00, 00, DateTimeKind.Utc);

    private NullContainer(IFileSystem fileSystem, ITimeSystem timeSystem)
    {
        FileSystem = fileSystem;
        TimeSystem = timeSystem;
    }

    #region IStorageContainer Members

    /// <inheritdoc cref="IStorageContainer.Attributes" />
    public FileAttributes Attributes
    {
        get => (FileAttributes)(-1);
        set => _ = value;
    }

    /// <inheritdoc cref="IStorageContainer.CreationTime" />
    public DateTime CreationTime
    {
        get => NullTime;
        set => _ = value;
    }

    /// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
    public IFileSystem FileSystem { get; }

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

    /// <inheritdoc cref="IStorageContainer.LinkTarget" />
    public string? LinkTarget
    {
        get => null;
        set => _ = value;
    }

    /// <inheritdoc cref="ITimeSystem.ITimeSystemExtensionPoint.TimeSystem" />
    public ITimeSystem TimeSystem { get; }

    /// <inheritdoc cref="IStorageContainer.Type" />
    public ContainerTypes Type
        => ContainerTypes.DirectoryOrFile;

    /// <inheritdoc cref="IStorageContainer.AppendBytes(byte[])" />
    public void AppendBytes(byte[] bytes)
    {
        // Do nothing in NullContainer
    }

    /// <inheritdoc cref="IStorageContainer.ClearBytes()" />
    public void ClearBytes()
    {
        // Do nothing in NullContainer
    }

    /// <inheritdoc cref="IStorageContainer.Decrypt()" />
    public void Decrypt()
    {
        // Do nothing in NullContainer
    }

    /// <inheritdoc cref="IStorageContainer.Encrypt()" />
    public void Encrypt()
    {
        // Do nothing in NullContainer
    }

    /// <inheritdoc cref="IStorageContainer.GetBytes()" />
    public byte[] GetBytes()
        => Array.Empty<byte>();

    /// <inheritdoc cref="IStorageContainer.RequestAccess(FileAccess, FileShare)" />
    public IStorageAccessHandle RequestAccess(FileAccess access, FileShare share)
        => new NullStorageAccessHandle();

    /// <inheritdoc cref="IStorageContainer.WriteBytes(byte[])" />
    public void WriteBytes(byte[] bytes)
    {
        // Do nothing in NullContainer
    }

    #endregion

    internal static IStorageContainer New(FileSystemMock fileSystem)
        => new NullContainer(fileSystem, fileSystem.TimeSystem);

    private sealed class NullStorageAccessHandle : IStorageAccessHandle
    {
        #region IStorageAccessHandle Members

        /// <inheritdoc cref="IStorageAccessHandle.Access" />
        public FileAccess Access => FileAccess.ReadWrite;

        /// <inheritdoc cref="IStorageAccessHandle.Share" />
        public FileShare Share => FileShare.None;

        /// <inheritdoc cref="IDisposable.Dispose()" />
        public void Dispose()
        {
            // Nothing to do!
        }

        #endregion
    }
}