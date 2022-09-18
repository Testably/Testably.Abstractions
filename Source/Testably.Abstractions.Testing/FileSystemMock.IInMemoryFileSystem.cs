using System;
using Testably.Abstractions.Testing.Internal.Models;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    /// <summary>
    ///     The container storing the current data of the <see cref="IFileSystem" /> in memory.
    /// </summary>
    public interface IInMemoryFileSystem
    {
        /// <summary>
        ///     Gets or adds a directory.
        /// </summary>
        IFileSystem.IDirectoryInfo? GetOrAddDirectory(string path);
        /// <summary>
        /// Checks if a <see cref="FileSystemInfoMock"/> exists on the given <paramref name="path"/>.
        /// </summary>
        bool Exists(string path);

        /// <summary>
        /// Deletes the <see cref="FileSystemInfoMock"/> on the given <paramref name="path"/>.
        /// </summary>
        bool Delete(string path);
    }
}