namespace Testably.Abstractions;

/// <summary>
///     Default implementation for file-related system dependencies.
///     <para />
///     Implements <seealso cref="IFileSystem" />
/// </summary>
public sealed partial class FileSystem : IFileSystem
{
    #region IFileSystem Members

    /// <inheritdoc cref="IFileSystem.Directory" />
    public IFileSystem.IDirectory Directory => new DirectoryFileSystem(this);

    /// <inheritdoc cref="IFileSystem.DirectoryInfo" />
    public IFileSystem.IDirectoryInfoFactory DirectoryInfo =>
        new DirectoryInfoFactory(this);

    /// <inheritdoc cref="IFileSystem.File" />
    public IFileSystem.IFile File => new FileFileSystem(this);

    /// <inheritdoc cref="IFileSystem.FileInfo" />
    public IFileSystem.IFileInfoFactory FileInfo =>
        new FileInfoFactory(this);

    /// <inheritdoc cref="IFileSystem.Path" />
    public IFileSystem.IPath Path => new PathFileSystem(this);

    #endregion
}