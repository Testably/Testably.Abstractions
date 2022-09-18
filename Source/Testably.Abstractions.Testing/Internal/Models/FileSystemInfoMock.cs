using System;

namespace Testably.Abstractions.Testing.Internal.Models;

internal class FileSystemInfoMock : IFileSystem.IFileSystemInfo
{
    protected readonly FileSystemMock FileSystem;
    private DateTime _creationTime;
    private DateTime _lastAccessTime;
    private DateTime _lastWriteTime;
    protected readonly string OriginalPath;

    internal FileSystemInfoMock(string path, FileSystemMock fileSystem)
    {
        if (path == null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (path == string.Empty)
        {
            throw new ArgumentException("The path is empty.", nameof(path));
        }

        OriginalPath = path;
        FullName = fileSystem.Path.GetFullPath(path).NormalizePath().TrimEnd(' ');
        FileSystem = fileSystem;
        AdjustTimes(TimeAdjustments.All);
    }

    #region IFileSystemInfo Members

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
    public bool Exists => FileSystem.InMemoryFileSystem.Exists(FullName);

    /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Extension" />
    public string Extension => FileSystem.Path.GetExtension(FullName);

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
    public string? LinkTarget => throw new NotImplementedException();
#endif

    /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Name" />
    public string Name => FileSystem.Path.GetFileName(FullName.TrimEnd(
        FileSystem.Path.DirectorySeparatorChar,
        FileSystem.Path.AltDirectorySeparatorChar));

#if FEATURE_FILESYSTEM_LINK
    /// <inheritdoc cref="IFileSystem.IFileSystemInfo.CreateAsSymbolicLink(string)" />
    public void CreateAsSymbolicLink(string pathToTarget)
        => throw new NotImplementedException();
#endif

    /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Delete()" />
    public void Delete() => FileSystem.InMemoryFileSystem.Delete(FullName);

#if FEATURE_FILESYSTEM_LINK
    /// <inheritdoc cref="IFileSystem.IFileSystemInfo.ResolveLinkTarget(bool)" />
    public IFileSystem.IFileSystemInfo? ResolveLinkTarget(bool returnFinalTarget)
        => throw new NotImplementedException();
#endif

    #endregion

#if NETSTANDARD2_0
    /// <inheritdoc cref="object.ToString()" />
#else
    /// <inheritdoc cref="System.IO.FileSystemInfo.ToString()" />
#endif
    public override string ToString() => OriginalPath;

    internal FileSystemInfoMock AdjustTimes(TimeAdjustments timeAdjustments)
    {
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

        return this;
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
}