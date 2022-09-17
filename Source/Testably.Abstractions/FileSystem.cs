namespace Testably.Abstractions;

/// <summary>
///     Default implementation for file-related system dependencies.
///     <para />
///     Implements <seealso cref="IFileSystem" />
/// </summary>
public sealed partial class FileSystem : IFileSystem
{
    #region IFileSystem Members

    /// <inheritdoc cref="IFileSystem.Path" />
    public IFileSystem.IPath Path => new PathFileSystem(this);

    /// <inheritdoc cref="IFileSystem.File" />
    public IFileSystem.IFile File => new FileFileSystem(this);

    #endregion
}