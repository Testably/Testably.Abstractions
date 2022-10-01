using System;
using System.IO;

namespace Testably.Abstractions.Testing;

/// <summary>
///     A test helper for simulating the file system. Implements <see cref="IFileSystem" />.
/// </summary>
public sealed partial class FileSystemMock : IFileSystem
{
    /// <summary>
    ///     The callback handler for intercepting events of the <see cref="FileSystemMock" /> before they occur.
    /// </summary>
    public IInterceptionHandler Intercept => Callback;

    /// <summary>
    ///     The callback handler for notifications of the <see cref="FileSystemMock" /> after an event occurred.
    /// </summary>
    public INotificationHandler Notify => Callback;

    /// <summary>
    ///     The used random system.
    /// </summary>
    public IRandomSystem RandomSystem { get; }

    /// <summary>
    ///     The used time system.
    /// </summary>
    public ITimeSystem TimeSystem { get; }

    internal IStorage Storage { get; }

    internal FileSystemMockCallbackHandler Callback { get; }
    private readonly DirectoryMock _directoryMock;
    private readonly FileMock _fileMock;
    private readonly PathMock _pathMock;

    internal IFileSystem.IFileSystemInfo NullFileSystemInfo { get; }

    /// <summary>
    ///     Initializes the <see cref="FileSystemMock" />.
    /// </summary>
    public FileSystemMock()
    {
        RandomSystem = new RandomSystem();
        TimeSystem = new TimeSystemMock(TimeProvider.Now());
        _pathMock = new PathMock(this);
        Storage = new InMemoryStorage(this);
        Callback = new FileSystemMockCallbackHandler(this);
        _directoryMock = new DirectoryMock(this);
        _fileMock = new FileMock(this);
        DirectoryInfo = new DirectoryInfoFactoryMock(this);
        DriveInfo = new DriveInfoFactoryMock(this);
        FileInfo = new FileInfoFactoryMock(this);
        FileStream = new FileStreamFactoryMock(this);
        NullFileSystemInfo = new FileSystemInfoMock(string.Empty, string.Empty, this)
        {
            LastWriteTime = new DateTime(1601, 01, 01, 00, 00, 00, DateTimeKind.Utc),
            LastAccessTime = new DateTime(1601, 01, 01, 00, 00, 00, DateTimeKind.Utc),
            CreationTime = new DateTime(1601, 01, 01, 00, 00, 00, DateTimeKind.Utc),
            Attributes = (FileAttributes)(-1),
        };
    }

    /// <summary>
    ///     Changes the parameters of the specified <paramref name="drive" />.
    ///     <para />
    ///     If the <paramref name="drive" /> does not exist, it will be created/mounted.
    /// </summary>
    public FileSystemMock WithDrive(string? drive,
                                    Action<IDriveInfoMock>? driveCallback = null)
    {
        IDriveInfoMock driveInfoMock = Storage.GetOrAddDrive(
            drive ?? "".PrefixRoot());
        driveCallback?.Invoke(driveInfoMock);
        return this;
    }

    #region IFileSystem Members

    /// <inheritdoc cref="IFileSystem.Directory" />
    public IFileSystem.IDirectory Directory
        => _directoryMock;

    /// <inheritdoc cref="IFileSystem.DirectoryInfo" />
    public IFileSystem.IDirectoryInfoFactory DirectoryInfo { get; }

    /// <inheritdoc cref="IFileSystem.DriveInfo" />
    public IFileSystem.IDriveInfoFactory DriveInfo { get; }

    /// <inheritdoc cref="IFileSystem.File" />
    public IFileSystem.IFile File
        => _fileMock;

    /// <inheritdoc cref="IFileSystem.FileInfo" />
    public IFileSystem.IFileInfoFactory FileInfo { get; }

    /// <inheritdoc cref="IFileSystem.FileStream" />
    public IFileSystem.IFileStreamFactory FileStream { get; }

    /// <inheritdoc cref="IFileSystem.Path" />
    public IFileSystem.IPath Path
        => _pathMock;

    #endregion
}

/// <summary>
///     Extension methods for the <see cref="FileSystemMock" />
/// </summary>
public static class FileSystemMockExtensions
{
    /// <summary>
    ///     Changes the parameters of the default drive ('C:\' on Windows, '/' on Linux)
    /// </summary>
    public static FileSystemMock WithDrive(
        this FileSystemMock fileSystemMock,
        Action<FileSystemMock.IDriveInfoMock> driveCallback)
        => fileSystemMock.WithDrive(null, driveCallback);
}