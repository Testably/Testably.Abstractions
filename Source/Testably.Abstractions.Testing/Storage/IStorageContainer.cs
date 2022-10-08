using System;
using System.IO;

namespace Testably.Abstractions.Testing.Storage;

/// <summary>
///     A container for a stored file or directory in the <see cref="IStorage" />.
/// </summary>
internal interface IStorageContainer : IFileSystem.IFileSystemExtensionPoint, ITimeSystem.ITimeSystemExtensionPoint
{
    /// <inheritdoc cref="System.IO.FileSystemInfo.Attributes" />
    FileAttributes Attributes { get; set; }

    /// <inheritdoc cref="System.IO.FileSystemInfo.CreationTime" />
    DateTime CreationTime { get; set; }

    /// <inheritdoc cref="System.IO.FileSystemInfo.LastAccessTime" />
    public DateTime LastAccessTime { get; set; }

    /// <inheritdoc cref="System.IO.FileSystemInfo.LastWriteTime" />
    public DateTime LastWriteTime { get; set; }

    /// <summary>
    ///     If this instance represents a link, returns the link target's path, otherwise returns <see langword="null" />.
    /// </summary>
    public string? LinkTarget { get; set; }

    /// <summary>
    ///     The type of the container indicates if it is a <see cref="ContainerTypes.File" /> or
    ///     <see cref="Directory" />.
    /// </summary>
    ContainerTypes Type { get; }

    /// <summary>
    ///     Appends the <paramref name="bytes" /> to the <see cref="IFileSystem.IFileInfo" />.
    /// </summary>
    void AppendBytes(byte[] bytes);

    /// <summary>
    ///     Clears the content of the <see cref="IFileSystem.IFileInfo" />.
    ///     <para />
    ///     This is used to delete the file.
    /// </summary>
    void ClearBytes();

    /// <summary>
    ///     Decrypts the file content and removes the <see cref="FileAttributes.Encrypted" /> attribute.
    ///     <para />
    ///     Does nothing if the file is not encrypted.
    /// </summary>
    void Decrypt();

    /// <summary>
    ///     Encrypts the file content and adds the <see cref="FileAttributes.Encrypted" /> attribute.
    ///     <para />
    ///     Does nothing if the file is already encrypted.
    /// </summary>
    void Encrypt();

    /// <summary>
    ///     Gets the bytes in the <see cref="IFileSystem.IFileInfo" />.
    /// </summary>
    byte[] GetBytes();

    /// <summary>
    ///     Requests access to this file with the given <paramref name="share" />.
    /// </summary>
    /// <returns>An <see cref="IStorageAccessHandle" /> that is used to release the access lock on dispose.</returns>
    IStorageAccessHandle RequestAccess(FileAccess access, FileShare share);

    /// <summary>
    ///     Writes the <paramref name="bytes" /> to the <see cref="IFileSystem.IFileInfo" />.
    /// </summary>
    void WriteBytes(byte[] bytes);
}