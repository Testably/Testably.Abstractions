using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
        ///     The current directory used in <see cref="System.IO.Directory.GetCurrentDirectory()" /> and
        ///     <see cref="System.IO.Directory.SetCurrentDirectory(string)" />
        /// </summary>
        string CurrentDirectory { get; set; }

        /// <summary>
        ///     Deletes the <see cref="FileSystemInfoMock" /> on the given <paramref name="path" />.
        /// </summary>
        bool Delete(string path, bool recursive = false);

        /// <summary>
        ///     Enumerates all directories under <paramref name="path" /> that match the <paramref name="expression" />.
        /// </summary>
        IEnumerable<TFileSystemInfoInfo> Enumerate<TFileSystemInfoInfo>(
            string path, string expression, EnumerationOptions enumerationOptions)
            where TFileSystemInfoInfo : IFileSystem.IFileSystemInfo;

        /// <summary>
        ///     Checks if a <see cref="FileSystemInfoMock" /> exists on the given <paramref name="path" />.
        /// </summary>
        bool Exists([NotNullWhen(true)] string? path);

        /// <summary>
        ///     Gets or adds a directory.
        /// </summary>
        IFileSystem.IDirectoryInfo? GetOrAddDirectory(string path);

        /// <summary>
        /// Returns the relative subdirectory path from <paramref name="fullFilePath"/> to the <paramref name="givenPath"/>.
        /// </summary>
        string GetSubdirectoryPath(string fullFilePath, string givenPath);
    }
}