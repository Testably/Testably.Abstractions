using System;
using System.IO;
using System.Runtime.InteropServices;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing.Storage;

internal sealed class InMemoryLocation : IStorageLocation
{
    private static readonly StringComparison StringComparisonMode =
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

    private readonly string _key;

    private InMemoryLocation(IStorageDrive? drive,
                             string fullPath,
                             string friendlyName)
    {
        FullPath = fullPath
           .NormalizePath()
           .TrimOnWindows();
#if FEATURE_PATH_ADVANCED
        _key = Path.TrimEndingDirectorySeparator(FullPath);
#else
            _key = FileFeatureExtensionMethods.TrimEndingDirectorySeparator(
                FullPath,
                System.IO.Path.DirectorySeparatorChar,
                System.IO.Path.AltDirectorySeparatorChar);
#endif
        if (Framework.IsNetFramework)
        {
            friendlyName = friendlyName.TrimOnWindows();
        }

        FriendlyName = friendlyName.RemoveLeadingDot();
        Drive = drive;
    }

    #region IStorageLocation Members

    /// <summary>
    ///     The <see cref="Drive" /> in which the <see cref="IFileSystem.IFileSystemInfo" /> is stored.
    /// </summary>
    public IStorageDrive? Drive { get; }

    /// <summary>
    ///     The friendly name from the location of the file or directory.
    /// </summary>
    public string FriendlyName { get; }

    /// <summary>
    ///     The full path of the location of the file or directory.
    /// </summary>
    public string FullPath { get; }

    /// <inheritdoc cref="IEquatable{IStorageLocation}.Equals(IStorageLocation)" />
    public bool Equals(IStorageLocation? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is InMemoryLocation location)
        {
            return _key.Equals(location._key, StringComparisonMode);
        }

        return FullPath.Equals(other.FullPath, StringComparisonMode);
    }

    /// <inheritdoc cref="IStorageLocation.GetParent()" />
    public IStorageLocation? GetParent()
    {
        if (Path.GetPathRoot(FullPath) == FullPath)
        {
            return null;
        }

        string? parentPath = Path.GetDirectoryName(FullPath);
        if (parentPath == null)
        {
            return null;
        }

        return New(Drive,
            parentPath,
            Path.GetDirectoryName(FriendlyName));
    }

    #endregion

    /// <inheritdoc cref="object.Equals(object?)" />
    public override bool Equals(object? obj)
        => ReferenceEquals(this, obj) ||
           (obj is IStorageLocation other && Equals(other));

    /// <inheritdoc cref="object.GetHashCode()" />
    public override int GetHashCode()
#if NETSTANDARD2_0
            => _key.ToLowerInvariant().GetHashCode();
#else
    {
        return _key.GetHashCode(StringComparisonMode);
    }
#endif

    public static InMemoryLocation New(IStorageDrive? drive,
                                       string path,
                                       string? friendlyName = null)
    {
        if (path == string.Empty)
        {
            if (Framework.IsNetFramework)
            {
                throw ExceptionFactory.PathHasNoLegalForm();
            }

            throw ExceptionFactory.PathIsEmpty(nameof(path));
        }

        friendlyName ??= path;
        return new InMemoryLocation(drive, path, friendlyName);
    }

    /// <inheritdoc cref="object.ToString()" />
    public override string ToString()
        => FullPath;
}