namespace Testably.Abstractions;

/// <summary>
///     Allows abstracting file-related system dependencies.
/// </summary>
public partial interface IFileSystem
{
    /// <summary>
    ///     Abstractions for <see cref="System.IO.Path" />.
    /// </summary>
    IPath Path { get; }

    /// <summary>
    ///     Abstractions for <see cref="System.IO.File" />.
    /// </summary>
    IFile File { get; }

    /// <summary>
    ///     Interface to support implementing extension methods on top of nested <see cref="IFileSystem" /> interfaces.
    /// </summary>
    interface IFileSystemExtensionPoint
    {
        /// <summary>
        ///     Exposes the underlying file system implementation.
        ///     <para />
        ///     This is useful for implementing extension methods.
        /// </summary>
        IFileSystem FileSystem { get; }
    }
}