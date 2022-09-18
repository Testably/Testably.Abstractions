using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Testably.Abstractions.Testing.Internal.Models;

namespace Testably.Abstractions.Testing.Internal;

internal class InMemoryFileSystem : FileSystemMock.IInMemoryFileSystem
{
    private static readonly char[] AdditionalInvalidPathChars = { '*', '?' };
    public IFileSystem FileSystem => _fileSystem;
    private readonly FileSystemMock _fileSystem;

    private readonly ConcurrentDictionary<string, FileSystemInfoMock> _files = new();

    public InMemoryFileSystem(FileSystemMock fileSystem)
    {
        _fileSystem = fileSystem;
    }

    #region IInMemoryFileSystem Members

    public string CurrentDirectory { get; set; } = "".PrefixRoot();

    /// <inheritdoc />
    public IFileSystem.IDirectoryInfo? GetOrAddDirectory(string path)
    {
        return _files.GetOrAdd(path, p => new DirectoryInfoMock(path, _fileSystem)) as
            IFileSystem.IDirectoryInfo;
    }

    /// <inheritdoc />
    public bool Exists([NotNullWhen(true)] string? path)
    {
        if (path == null)
        {
            return false;
        }

        return _files.ContainsKey(path);
    }

    /// <inheritdoc />
    public bool Delete(string path)
    {
        return _files.TryRemove(path, out _);
    }

    /// <summary>
    ///     Determines whether the given path contains illegal characters.
    /// </summary>
    public bool HasIllegalCharacters(string path)
    {
        if (path == null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        //TODO Add to IPath interface and use from _fileSystem
        char[] invalidPathChars = Path.GetInvalidPathChars();

        if (path.IndexOfAny(invalidPathChars) >= 0)
        {
            return true;
        }

        return path.IndexOfAny(AdditionalInvalidPathChars) >= 0;
    }

    #endregion

    public IFileSystem.IDirectoryInfo? GetOrAddDirectory(string path,
                                                         Func<string, DirectoryInfoMock>
                                                             func)
    {
        return _files.GetOrAdd(path, func) as IFileSystem.IDirectoryInfo;
    }
}