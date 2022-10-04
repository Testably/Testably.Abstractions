using System;

namespace Testably.Abstractions.Testing;

/// <summary>
///     Initializes the <see cref="IFileSystem" /> with test data.
/// </summary>
public static partial class FileSystemInitializer
{
    /// <summary>
    ///     Cleans the directory in <see cref="BasePath" /> on dispose.
    /// </summary>
    public interface IDirectoryCleaner : IDisposable
    {
        /// <summary>
        ///     The directory that gets cleaned up on dispose.
        /// </summary>
        string BasePath { get; }
    }
}