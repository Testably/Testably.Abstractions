using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Versioning;

namespace Testably.Abstractions.Models;

internal class FileInfoWrapper : FileSystemInfoWrapper, IFileSystem.IFileInfo
{
    private readonly IFileSystem _fileSystem;
    private readonly FileInfo _instance;

    internal FileInfoWrapper(FileInfo instance, IFileSystem fileSystem)
        : base(instance, fileSystem)
    {
        _instance = instance;
        _fileSystem = fileSystem;
    }

    #region IFileInfo Members

    /// <inheritdoc cref="IFileSystem.IFileInfo.Directory" />
    public IFileSystem.IDirectoryInfo? Directory
        => DirectoryInfoWrapper.FromDirectoryInfo(_instance.Directory, _fileSystem);

    /// <inheritdoc cref="IFileSystem.IFileInfo.DirectoryName" />
    public string? DirectoryName
        => _instance.DirectoryName;

    /// <inheritdoc cref="IFileSystem.IFileInfo.IsReadOnly" />
    public bool IsReadOnly
    {
        get => _instance.IsReadOnly;
        set => _instance.IsReadOnly = value;
    }

    /// <inheritdoc cref="IFileSystem.IFileInfo.Length" />
    public long Length
        => _instance.Length;

    /// <inheritdoc cref="IFileSystem.IFileInfo.AppendText()" />
    public StreamWriter AppendText()
        => _instance.AppendText();

    /// <inheritdoc cref="IFileSystem.IFileInfo.CopyTo(string)" />
    public IFileSystem.IFileInfo CopyTo(string destFileName)
        => FromFileInfo(_instance.CopyTo(destFileName), _fileSystem);

    /// <inheritdoc cref="IFileSystem.IFileInfo.CopyTo(string, bool)" />
    public IFileSystem.IFileInfo CopyTo(string destFileName, bool overwrite)
        => FromFileInfo(_instance.CopyTo(destFileName, overwrite), _fileSystem);

    /// <inheritdoc cref="IFileSystem.IFileInfo.Create()" />
    public FileStream Create()
        => _instance.Create();

    /// <inheritdoc cref="IFileSystem.IFileInfo.CreateText()" />
    public StreamWriter CreateText()
        => _instance.CreateText();

    /// <inheritdoc cref="IFileSystem.IFileInfo.Decrypt()" />
#if NET6_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    public void Decrypt()
        => _instance.Decrypt();

    /// <inheritdoc cref="IFileSystem.IFileInfo.Encrypt()" />
#if NET6_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    public void Encrypt()
        => _instance.Encrypt();

    /// <inheritdoc cref="IFileSystem.IFileInfo.MoveTo(string)" />
    public void MoveTo(string destFileName)
        => _instance.MoveTo(destFileName);

#if FEATURE_FILE_MOVETO_OVERWRITE
    /// <inheritdoc cref="IFileSystem.IFileInfo.MoveTo(string, bool)" />
    public void MoveTo(string destFileName, bool overwrite)
        => _instance.MoveTo(destFileName, overwrite);
#endif

    /// <inheritdoc cref="IFileSystem.IFileInfo.Open(FileMode)" />
    public FileStream Open(FileMode mode)
        => _instance.Open(mode);

    /// <inheritdoc cref="IFileSystem.IFileInfo.Open(FileMode, FileAccess)" />
    public FileStream Open(FileMode mode, FileAccess access)
        => _instance.Open(mode, access);

    /// <inheritdoc cref="IFileSystem.IFileInfo.Open(FileMode, FileAccess, FileShare)" />
    public FileStream Open(FileMode mode, FileAccess access, FileShare share)
        => _instance.Open(mode, access, share);

    /// <inheritdoc cref="IFileSystem.IFileInfo.OpenRead()" />
    public FileStream OpenRead()
        => _instance.OpenRead();

    /// <inheritdoc cref="IFileSystem.IFileInfo.OpenText()" />
    public StreamReader OpenText()
        => _instance.OpenText();

    /// <inheritdoc cref="IFileSystem.IFileInfo.OpenWrite()" />
    public FileStream OpenWrite()
        => _instance.OpenWrite();

    /// <inheritdoc cref="IFileSystem.IFileInfo.Replace(string, string?)" />
    public IFileSystem.IFileInfo Replace(string destinationFileName,
                                         string? destinationBackupFileName)
        => FromFileInfo(
            _instance.Replace(destinationFileName, destinationBackupFileName),
            _fileSystem);

    /// <inheritdoc cref="IFileSystem.IFileInfo.Replace(string, string?, bool)" />
    public IFileSystem.IFileInfo Replace(string destinationFileName,
                                         string? destinationBackupFileName,
                                         bool ignoreMetadataErrors)
        => FromFileInfo(
            _instance.Replace(destinationFileName, destinationBackupFileName,
                ignoreMetadataErrors),
            _fileSystem);

    #endregion

    [return: NotNullIfNotNull("instance")]
    internal static FileInfoWrapper? FromFileInfo(FileInfo? instance,
                                                  IFileSystem fileSystem)
    {
        if (instance == null)
        {
            return null;
        }

        return new FileInfoWrapper(instance, fileSystem);
    }
}