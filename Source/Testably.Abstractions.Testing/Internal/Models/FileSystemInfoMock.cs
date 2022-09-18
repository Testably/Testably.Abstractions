using System.Diagnostics.CodeAnalysis;
using System.IO;
using System;

namespace Testably.Abstractions.Testing.Internal.Models;


internal class FileSystemInfoMock : IFileSystem.IFileSystemInfo
{
    protected readonly FileSystemMock FileSystem;
    private DateTime _creationTime;
    private DateTime _lastAccessTime;
    private DateTime _lastWriteTime;
    private readonly string _path;
    protected readonly string OriginalPath;

    internal FileSystemInfoMock(string path, FileSystemMock fileSystem)
    {
        OriginalPath = path;
        _path = path;//TODO Adjust
        FileSystem = fileSystem;
        _creationTime = fileSystem.TimeSystem.DateTime.UtcNow;
        _lastAccessTime = fileSystem.TimeSystem.DateTime.UtcNow;
        _lastWriteTime = fileSystem.TimeSystem.DateTime.UtcNow;
    }

    #region IFileSystemInfo Members

    /// <inheritdoc cref="IFileSystem.IFileSystemInfo.CreationTime" />
    public DateTime CreationTime
    {
        get => _creationTime.ToLocalTime();
        set => _creationTime = AdjustTime(value);
    }

    private static DateTime AdjustTime(DateTime time, DateTimeKind kind = DateTimeKind.Utc)
    {
        if (time.Kind == DateTimeKind.Unspecified)
        {
            return DateTime.SpecifyKind(time, kind);
        }

        if (kind == DateTimeKind.Local)
        {
            return time.ToLocalTime();
        }

        return time.ToUniversalTime();
    }

    /// <inheritdoc cref="IFileSystem.IFileSystemInfo.CreationTimeUtc" />
    public DateTime CreationTimeUtc
    {
        get => _creationTime.ToUniversalTime();
        set => _creationTime = AdjustTime(value);
    }

    /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Exists" />
    public bool Exists => FileSystem.InMemoryFileSystem.Exists(_path);

    /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Extension" />
    public string Extension => FileSystem.Path.GetExtension(_path);

    /// <inheritdoc cref="IFileSystem.IFileSystemInfo.FullName" />
    public string FullName => _path;

    /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastAccessTime" />
    public DateTime LastAccessTime
    {
        get => _lastAccessTime.ToLocalTime();
        set => _lastAccessTime = AdjustTime(value);
    }

    /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastAccessTimeUtc" />
    public DateTime LastAccessTimeUtc
    {
        get => _lastAccessTime.ToUniversalTime();
        set => _lastAccessTime = AdjustTime(value);
    }

    /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastWriteTime" />
    public DateTime LastWriteTime
    {
        get => _lastWriteTime.ToLocalTime();
        set => _lastWriteTime = AdjustTime(value);
    }

    /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastWriteTimeUtc" />
    public DateTime LastWriteTimeUtc
    {
        get => _lastWriteTime.ToUniversalTime();
        set => _lastWriteTime = AdjustTime(value);
    }

#if FEATURE_FILESYSTEM_LINK
    /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LinkTarget" />
    public string? LinkTarget => throw new NotImplementedException();
#endif

    /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Name" />
    public string Name => FileSystem.Path.GetFileName(_path);

#if FEATURE_FILESYSTEM_LINK
    /// <inheritdoc cref="IFileSystem.IFileSystemInfo.CreateAsSymbolicLink(string)" />
    public void CreateAsSymbolicLink(string pathToTarget)
        => throw new NotImplementedException();
#endif

    /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Delete()" />
    public void Delete() => FileSystem.InMemoryFileSystem.Delete(_path);

#if FEATURE_FILESYSTEM_LINK
    /// <inheritdoc cref="IFileSystem.IFileSystemInfo.ResolveLinkTarget(bool)" />
    public FileSystemInfo? ResolveLinkTarget(bool returnFinalTarget)
        => throw new NotImplementedException();
#endif

    #endregion

#if NETSTANDARD2_0
/// <inheritdoc cref="object.ToString()" />
#else
    /// <inheritdoc cref="FileSystemInfo.ToString()" />
#endif
    public override string ToString() => OriginalPath;
    
}