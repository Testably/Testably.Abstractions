using System.Diagnostics.CodeAnalysis;
using System.IO;
#if !NETSTANDARD2_0
using System;
#endif

namespace Testably.Abstractions.Helpers;

/// <summary>
///     Default implementation for <see cref="System.IO.Path" /> abstractions.
///     <para />
///     Implements <seealso cref="IFileSystem.IPath" />
/// </summary>
public abstract class PathSystem : IFileSystem.IPath
{
    /// <summary>
    ///     Initializes a new instance of <see cref="PathSystem" /> for the given <paramref name="fileSystem" />.
    /// </summary>
    protected PathSystem(IFileSystem fileSystem)
    {
        FileSystem = fileSystem;
    }

    #region IPath Members

    /// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
    public IFileSystem FileSystem { get; }

    /// <inheritdoc cref="Path.AltDirectorySeparatorChar" />
    public char AltDirectorySeparatorChar
        => Path.AltDirectorySeparatorChar;

    /// <inheritdoc cref="Path.DirectorySeparatorChar" />
    public char DirectorySeparatorChar
        => Path.DirectorySeparatorChar;

    /// <inheritdoc cref="Path.PathSeparator" />
    public char PathSeparator
        => Path.PathSeparator;

    /// <inheritdoc cref="Path.VolumeSeparatorChar" />
    public char VolumeSeparatorChar
        => Path.VolumeSeparatorChar;

    /// <inheritdoc cref="Path.ChangeExtension(string, string)" />
    [return: NotNullIfNotNull("path")]
    public string? ChangeExtension(string? path, string? extension)
        => Path.ChangeExtension(path, extension);

    /// <inheritdoc cref="Path.Combine(string, string)" />
    public string Combine(string path1, string path2)
        => Path.Combine(path1, path2);

    /// <inheritdoc cref="Path.Combine(string, string, string)" />
    public string Combine(string path1, string path2, string path3)
        => Path.Combine(path1, path2, path3);

    /// <inheritdoc cref="Path.Combine(string, string, string, string)" />
    public string Combine(string path1, string path2, string path3, string path4)
        => Path.Combine(path1, path2, path3, path4);

    /// <inheritdoc cref="Path.Combine(string[])" />
    public string Combine(params string[] paths)
        => Path.Combine(paths);

#if FEATURE_SPAN
    /// <inheritdoc cref="Path.GetDirectoryName(ReadOnlySpan{char})" />
    public ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path)
        => Path.GetDirectoryName(path);
#endif

    /// <inheritdoc cref="Path.GetDirectoryName(string)" />
    public string? GetDirectoryName(string? path)
        => Path.GetDirectoryName(path);

#if FEATURE_SPAN
    /// <inheritdoc cref="Path.GetExtension(ReadOnlySpan{char})" />
    public ReadOnlySpan<char> GetExtension(ReadOnlySpan<char> path)
        => Path.GetExtension(path);
#endif

    /// <inheritdoc cref="Path.GetExtension(string)" />
    [return: NotNullIfNotNull("path")]
    public string? GetExtension(string? path)
        => Path.GetExtension(path);

#if FEATURE_SPAN
    /// <inheritdoc cref="Path.GetFileName(ReadOnlySpan{char})" />
    public ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path)
        => Path.GetFileName(path);
#endif

    /// <inheritdoc cref="Path.GetFileName(string)" />
    [return: NotNullIfNotNull("path")]
    public string? GetFileName(string? path)
        => Path.GetFileName(path);

#if FEATURE_SPAN
    /// <inheritdoc cref="Path.GetFileNameWithoutExtension(ReadOnlySpan{char})" />
    public ReadOnlySpan<char> GetFileNameWithoutExtension(ReadOnlySpan<char> path)
        => Path.GetFileNameWithoutExtension(path);
#endif

    /// <inheritdoc cref="Path.GetFileNameWithoutExtension(string)" />
    [return: NotNullIfNotNull("path")]
    public string? GetFileNameWithoutExtension(string? path)
        => Path.GetFileNameWithoutExtension(path);

    /// <inheritdoc cref="Path.GetFullPath(string)" />
    public virtual string GetFullPath(string path)
        => Path.GetFullPath(path);

#if FEATURE_PATH_RELATIVE
    /// <inheritdoc cref="Path.GetFullPath(string, string)" />
    public virtual string GetFullPath(string path, string basePath)
        => Path.GetFullPath(path, basePath);
#endif

    /// <inheritdoc cref="Path.GetInvalidFileNameChars()" />
    public char[] GetInvalidFileNameChars()
        => Path.GetInvalidFileNameChars();

    /// <inheritdoc cref="Path.GetInvalidPathChars()" />
    public char[] GetInvalidPathChars()
        => Path.GetInvalidPathChars();

#if FEATURE_SPAN
    /// <inheritdoc cref="Path.GetPathRoot(ReadOnlySpan{char})" />
    public ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path)
        => Path.GetPathRoot(path);
#endif

    /// <inheritdoc cref="Path.GetPathRoot(string?)" />
    public string? GetPathRoot(string? path)
        => Path.GetPathRoot(path);

    /// <inheritdoc cref="Path.GetRandomFileName()" />
    public string GetRandomFileName()
        => Path.GetRandomFileName();

#if FEATURE_PATH_RELATIVE
    /// <inheritdoc cref="Path.GetRelativePath(string, string)" />
    public string GetRelativePath(string relativeTo, string path)
        => Path.GetRelativePath(relativeTo, path);
#endif

    /// <inheritdoc cref="Path.GetTempPath()" />
    public string GetTempPath()
        => Path.GetTempPath();

    /// <inheritdoc cref="Path.GetTempFileName()" />
#if !NETSTANDARD2_0
    [Obsolete(
        "Insecure temporary file creation methods should not be used. Use `Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())` instead.")]
#endif
    public string GetTempFileName()
        => Path.GetTempFileName();

#if FEATURE_SPAN
    /// <inheritdoc cref="Path.HasExtension(ReadOnlySpan{char})" />
    public bool HasExtension(ReadOnlySpan<char> path)
        => Path.HasExtension(path);
#endif

    /// <inheritdoc cref="Path.HasExtension(string)" />
    public bool HasExtension([NotNullWhen(true)] string? path)
        => Path.HasExtension(path);

#if FEATURE_SPAN
    /// <inheritdoc cref="Path.IsPathFullyQualified(ReadOnlySpan{char})" />
    public bool IsPathFullyQualified(ReadOnlySpan<char> path)
        => Path.IsPathFullyQualified(path);
#endif

#if FEATURE_PATH_RELATIVE
    /// <inheritdoc cref="Path.IsPathFullyQualified(string)" />
    public bool IsPathFullyQualified(string path)
        => Path.IsPathFullyQualified(path);
#endif

#if FEATURE_SPAN
    /// <inheritdoc cref="Path.IsPathRooted(ReadOnlySpan{char})" />
    public bool IsPathRooted(ReadOnlySpan<char> path)
        => Path.IsPathRooted(path);
#endif

    /// <inheritdoc cref="Path.IsPathRooted(string)" />
    public bool IsPathRooted(string? path)
        => Path.IsPathRooted(path);

    #endregion

#if FEATURE_PATH_ADVANCED
    /// <inheritdoc cref="Path.EndsInDirectorySeparator(ReadOnlySpan{char})" />
    public bool EndsInDirectorySeparator(ReadOnlySpan<char> path)
        => Path.EndsInDirectorySeparator(path);

    /// <inheritdoc cref="Path.EndsInDirectorySeparator(string)" />
    public bool EndsInDirectorySeparator(string path)
        => Path.EndsInDirectorySeparator(path);
#endif

#if FEATURE_PATH_JOIN
    /// <inheritdoc cref="Path.Join(ReadOnlySpan{char}, ReadOnlySpan{char})" />
    public string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2)
        => Path.Join(path1, path2);

    /// <inheritdoc cref="Path.Join(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char})" />
    public string Join(ReadOnlySpan<char> path1,
                       ReadOnlySpan<char> path2,
                       ReadOnlySpan<char> path3)
        => Path.Join(path1, path2, path3);
#endif

#if FEATURE_PATH_ADVANCED
    /// <inheritdoc cref="Path.Join(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char})" />
    public string Join(ReadOnlySpan<char> path1,
                       ReadOnlySpan<char> path2,
                       ReadOnlySpan<char> path3,
                       ReadOnlySpan<char> path4)
        => Path.Join(path1, path2, path3, path4);

    /// <inheritdoc cref="Path.Join(string, string)" />
    public string Join(string? path1, string? path2)
        => Path.Join(path1, path2);

    /// <inheritdoc cref="Path.Join(string, string, string)" />
    public string Join(string? path1, string? path2, string? path3)
        => Path.Join(path1, path2, path3);

    /// <inheritdoc cref="Path.Join(string, string, string, string)" />
    public string Join(string? path1, string? path2, string? path3, string? path4)
        => Path.Join(path1, path2, path3, path4);

    /// <inheritdoc cref="Path.Join(string[])" />
    public string Join(params string?[] paths)
        => Path.Join(paths);
#endif

#if FEATURE_PATH_ADVANCED
    /// <inheritdoc cref="Path.TrimEndingDirectorySeparator(ReadOnlySpan{char})" />
    public ReadOnlySpan<char> TrimEndingDirectorySeparator(ReadOnlySpan<char> path)
        => Path.TrimEndingDirectorySeparator(path);

    /// <inheritdoc cref="Path.TrimEndingDirectorySeparator(string)" />
    public string TrimEndingDirectorySeparator(string path)
        => Path.TrimEndingDirectorySeparator(path);
#endif

#if FEATURE_PATH_JOIN
    /// <inheritdoc cref="Path.TryJoin(ReadOnlySpan{char}, ReadOnlySpan{char}, Span{char}, out int)" />
    public bool TryJoin(ReadOnlySpan<char> path1,
                        ReadOnlySpan<char> path2,
                        Span<char> destination,
                        out int charsWritten)
        => Path.TryJoin(path1, path2, destination, out charsWritten);

    /// <inheritdoc cref="Path.TryJoin(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char}, Span{char}, out int)" />
    public bool TryJoin(ReadOnlySpan<char> path1,
                        ReadOnlySpan<char> path2,
                        ReadOnlySpan<char> path3,
                        Span<char> destination,
                        out int charsWritten)
        => Path.TryJoin(path1, path2, path3, destination, out charsWritten);
#endif
}