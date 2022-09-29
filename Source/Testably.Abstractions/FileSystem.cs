using static Testably.Abstractions.IFileSystem;

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
    public IDirectory Directory
        => new DirectoryFileSystem(this);

    /// <inheritdoc cref="IFileSystem.DirectoryInfo" />
    public IDirectoryInfoFactory DirectoryInfo
        => new DirectoryInfoFactory(this);

    /// <inheritdoc cref="IFileSystem.DriveInfo" />
    public IDriveInfoFactory DriveInfo
        => new DriveInfoFactory(this);

    /// <inheritdoc cref="IFileSystem.File" />
    public IFile File
        => new FileFileSystem(this);

    /// <inheritdoc cref="IFileSystem.FileInfo" />
    public IFileInfoFactory FileInfo
        => new FileInfoFactory(this);

    /// <inheritdoc cref="IFileSystem.FileStream" />
    public IFileStreamFactory FileStream
        => new FileStreamFactory(this);

    /// <inheritdoc cref="IFileSystem.Path" />
    public IPath Path
        => new PathFileSystem(this);

    #endregion
}