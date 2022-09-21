using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Testably.Abstractions.Models;

internal sealed class DirectoryInfoWrapper : FileSystemInfoWrapper,
    IFileSystem.IDirectoryInfo
{
    private readonly IFileSystem _fileSystem;
    private readonly DirectoryInfo _instance;

    internal DirectoryInfoWrapper(DirectoryInfo instance, IFileSystem fileSystem)
        : base(instance, fileSystem)
    {
        _instance = instance;
        _fileSystem = fileSystem;
    }

    #region IDirectoryInfo Members

    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.Parent" />
    public IFileSystem.IDirectoryInfo? Parent
        => FromDirectoryInfo(_instance.Parent, _fileSystem);

    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.Root" />
    public IFileSystem.IDirectoryInfo Root
        => FromDirectoryInfo(_instance.Root, _fileSystem);

    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.Create()" />
    public void Create()
        => _instance.Create();

    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.CreateSubdirectory(string)" />
    public IFileSystem.IDirectoryInfo CreateSubdirectory(string path)
        => FromDirectoryInfo(_instance.CreateSubdirectory(path), _fileSystem);

    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.Delete(bool)" />
    public void Delete(bool recursive)
        => _instance.Delete(recursive);

    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateDirectories()" />
    public IEnumerable<IFileSystem.IDirectoryInfo> EnumerateDirectories()
        => _instance.EnumerateDirectories()
           .Select(directoryInfo => FromDirectoryInfo(directoryInfo, _fileSystem));

    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateDirectories(string)" />
    public IEnumerable<IFileSystem.IDirectoryInfo> EnumerateDirectories(
        string searchPattern)
        => _instance.EnumerateDirectories(searchPattern)
           .Select(directoryInfo =>
                FromDirectoryInfo(directoryInfo, _fileSystem));

    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateDirectories(string, SearchOption)" />
    public IEnumerable<IFileSystem.IDirectoryInfo> EnumerateDirectories(
        string searchPattern, SearchOption searchOption)
        => _instance.EnumerateDirectories(searchPattern, searchOption)
           .Select(directoryInfo =>
                FromDirectoryInfo(directoryInfo, _fileSystem));

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateDirectories(string, EnumerationOptions)" />
    public IEnumerable<IFileSystem.IDirectoryInfo> EnumerateDirectories(
        string searchPattern,
        EnumerationOptions enumerationOptions)
        => _instance.EnumerateDirectories(searchPattern, enumerationOptions)
           .Select(directoryInfo =>
                FromDirectoryInfo(directoryInfo, _fileSystem));
#endif

    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFiles()" />
    public IEnumerable<IFileSystem.IFileInfo> EnumerateFiles()
        => _instance.EnumerateFiles()
           .Select(fileInfo =>
                FileInfoWrapper.FromFileInfo(fileInfo, _fileSystem));

    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFiles(string)" />
    public IEnumerable<IFileSystem.IFileInfo> EnumerateFiles(string searchPattern)
        => _instance.EnumerateFiles(searchPattern)
           .Select(fileInfo =>
                FileInfoWrapper.FromFileInfo(fileInfo, _fileSystem));

    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFiles(string, SearchOption)" />
    public IEnumerable<IFileSystem.IFileInfo> EnumerateFiles(
        string searchPattern, SearchOption searchOption)
        => _instance.EnumerateFiles(searchPattern, searchOption)
           .Select(fileInfo =>
                FileInfoWrapper.FromFileInfo(fileInfo, _fileSystem));

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFiles(string, EnumerationOptions)" />
    public IEnumerable<IFileSystem.IFileInfo> EnumerateFiles(
        string searchPattern, EnumerationOptions enumerationOptions)
        => _instance.EnumerateFiles(searchPattern, enumerationOptions)
           .Select(fileInfo =>
                FileInfoWrapper.FromFileInfo(fileInfo, _fileSystem));
#endif

    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFileSystemInfos()" />
    public IEnumerable<IFileSystem.IFileSystemInfo> EnumerateFileSystemInfos()
        => _instance.EnumerateFileSystemInfos()
           .Select(fileSystemInfo =>
                FromFileSystemInfo(fileSystemInfo, _fileSystem));

    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFileSystemInfos(string)" />
    public IEnumerable<IFileSystem.IFileSystemInfo> EnumerateFileSystemInfos(
        string searchPattern)
        => _instance.EnumerateFileSystemInfos(searchPattern)
           .Select(fileSystemInfo =>
                FromFileSystemInfo(fileSystemInfo, _fileSystem));

    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFileSystemInfos(string, SearchOption)" />
    public IEnumerable<IFileSystem.IFileSystemInfo> EnumerateFileSystemInfos(
        string searchPattern, SearchOption searchOption)
        => _instance.EnumerateFileSystemInfos(searchPattern, searchOption)
           .Select(fileSystemInfo =>
                FromFileSystemInfo(fileSystemInfo, _fileSystem));

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFileSystemInfos(string, EnumerationOptions)" />
    public IEnumerable<IFileSystem.IFileSystemInfo> EnumerateFileSystemInfos(
        string searchPattern,
        EnumerationOptions enumerationOptions)
        => _instance.EnumerateFileSystemInfos(searchPattern, enumerationOptions)
           .Select(fileSystemInfo =>
                FromFileSystemInfo(fileSystemInfo, _fileSystem));
#endif

    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetDirectories()" />
    public IFileSystem.IDirectoryInfo[] GetDirectories()
        => _instance.GetDirectories()
           .Select(directoryInfo =>
                (IFileSystem.IDirectoryInfo)FromDirectoryInfo(directoryInfo, _fileSystem))
           .ToArray();

    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetDirectories(string)" />
    public IFileSystem.IDirectoryInfo[] GetDirectories(string searchPattern)
        => _instance.GetDirectories(searchPattern)
           .Select(directoryInfo =>
                (IFileSystem.IDirectoryInfo)FromDirectoryInfo(directoryInfo, _fileSystem))
           .ToArray();

    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetDirectories(string, SearchOption)" />
    public IFileSystem.IDirectoryInfo[] GetDirectories(
        string searchPattern, SearchOption searchOption)
        => _instance.GetDirectories(searchPattern, searchOption)
           .Select(directoryInfo =>
                (IFileSystem.IDirectoryInfo)FromDirectoryInfo(directoryInfo, _fileSystem))
           .ToArray();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetDirectories(string, EnumerationOptions)" />
    public IFileSystem.IDirectoryInfo[] GetDirectories(
        string searchPattern, EnumerationOptions enumerationOptions)
        => _instance.GetDirectories(searchPattern, enumerationOptions)
           .Select(directoryInfo =>
                (IFileSystem.IDirectoryInfo)FromDirectoryInfo(directoryInfo, _fileSystem))
           .ToArray();
#endif

    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFiles()" />
    public IFileSystem.IFileInfo[] GetFiles()
        => _instance.GetFiles()
           .Select(fileInfo =>
                (IFileSystem.IFileInfo)FromFileSystemInfo(fileInfo, _fileSystem))
           .ToArray();

    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFiles(string)" />
    public IFileSystem.IFileInfo[] GetFiles(string searchPattern)
        => _instance.GetFiles(searchPattern)
           .Select(fileInfo =>
                (IFileSystem.IFileInfo)FromFileSystemInfo(fileInfo, _fileSystem))
           .ToArray();

    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFiles(string, SearchOption)" />
    public IFileSystem.IFileInfo[] GetFiles(string searchPattern,
                                            SearchOption searchOption)
        => _instance.GetFiles(searchPattern, searchOption)
           .Select(fileInfo =>
                (IFileSystem.IFileInfo)FromFileSystemInfo(fileInfo, _fileSystem))
           .ToArray();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFiles(string, EnumerationOptions)" />
    public IFileSystem.IFileInfo[] GetFiles(string searchPattern,
                                            EnumerationOptions enumerationOptions)
        => _instance.GetFiles(searchPattern, enumerationOptions)
           .Select(fileInfo =>
                (IFileSystem.IFileInfo)FromFileSystemInfo(fileInfo, _fileSystem))
           .ToArray();
#endif

    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFileSystemInfos()" />
    public IFileSystem.IFileSystemInfo[] GetFileSystemInfos()
        => _instance.GetFileSystemInfos()
           .Select(fileSystemInfo =>
                (IFileSystem.IFileSystemInfo)FromFileSystemInfo(fileSystemInfo,
                    _fileSystem))
           .ToArray();

    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFileSystemInfos(string)" />
    public IFileSystem.IFileSystemInfo[] GetFileSystemInfos(string searchPattern)
        => _instance.GetFileSystemInfos(searchPattern)
           .Select(fileSystemInfo =>
                (IFileSystem.IFileSystemInfo)FromFileSystemInfo(fileSystemInfo,
                    _fileSystem))
           .ToArray();

    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFileSystemInfos(string, SearchOption)" />
    public IFileSystem.IFileSystemInfo[] GetFileSystemInfos(
        string searchPattern, SearchOption searchOption)
        => _instance.GetFileSystemInfos(searchPattern, searchOption)
           .Select(fileSystemInfo =>
                (IFileSystem.IFileSystemInfo)FromFileSystemInfo(fileSystemInfo,
                    _fileSystem))
           .ToArray();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFileSystemInfos(string, EnumerationOptions)" />
    public IFileSystem.IFileSystemInfo[] GetFileSystemInfos(string searchPattern,
        EnumerationOptions enumerationOptions)
        => _instance.GetFileSystemInfos(searchPattern, enumerationOptions)
           .Select(fileSystemInfo =>
                (IFileSystem.IFileSystemInfo)FromFileSystemInfo(fileSystemInfo,
                    _fileSystem))
           .ToArray();
#endif

    /// <inheritdoc cref="IFileSystem.IDirectoryInfo.MoveTo(string)" />
    public void MoveTo(string destDirName)
        => _instance.MoveTo(destDirName);

    #endregion

    [return: NotNullIfNotNull("instance")]
    internal static DirectoryInfoWrapper? FromDirectoryInfo(DirectoryInfo? instance,
        IFileSystem fileSystem)
    {
        if (instance == null)
        {
            return null;
        }

        return new DirectoryInfoWrapper(instance, fileSystem);
    }
}