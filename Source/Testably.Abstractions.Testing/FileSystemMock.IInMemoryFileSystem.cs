using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

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
            string path,
            string expression,
            EnumerationOptions enumerationOptions,
            Func<Exception> notFoundException)
            where TFileSystemInfoInfo : IFileSystem.IFileSystemInfo;

        /// <summary>
        ///     Checks if a <see cref="FileSystemInfoMock" /> exists on the given <paramref name="path" />.
        /// </summary>
        bool Exists([NotNullWhen(true)] string? path);

        /// <summary>
        ///     Gets a directory if it exists.<br />
        ///     Returns <c>null</c>, if the directory does not exist.
        /// </summary>
        IFileSystem.IDirectoryInfo? GetDirectory(string path);

        /// <summary>
        ///     Returns the drives that are present.
        /// </summary>
        IEnumerable<IFileSystem.IDriveInfo> GetDrives();

        /// <summary>
        ///     Gets a file if it exists.<br />
        ///     Returns <c>null</c>, if the file does not exist.
        /// </summary>
        IWritableFileInfo? GetFile(string path);

        /// <summary>
        ///     Gets or adds a directory.
        /// </summary>
        IFileSystem.IDirectoryInfo? GetOrAddDirectory(string path);

        /// <summary>
        ///     Gets or adds a file.
        /// </summary>
        IWritableFileInfo? GetOrAddFile(string path);

        /// <summary>
        ///     Returns the relative subdirectory path from <paramref name="fullFilePath" /> to the <paramref name="givenPath" />.
        /// </summary>
        string GetSubdirectoryPath(string fullFilePath, string givenPath);

        /// <summary>
        ///     An <see cref="IFileSystem.IFileInfo" /> which allows writing to the underlying byte array.
        /// </summary>
        public interface IWritableFileInfo : IFileSystem.IFileInfo
        {
            /// <summary>
            ///     Appends the <paramref name="bytes" /> to the <see cref="IFileSystem.IFileInfo" />.
            /// </summary>
            void AppendBytes(byte[] bytes);

            /// <summary>
            ///     Gets the bytes in the <see cref="IFileSystem.IFileInfo" />.
            /// </summary>
            byte[] GetBytes();

            /// <summary>
            ///     Writes the <paramref name="bytes" /> to the <see cref="IFileSystem.IFileInfo" />.
            /// </summary>
            void WriteBytes(byte[] bytes);

            /// <summary>
            ///     Requests access to this file with the given <paramref name="share" />.
            ///     <para />
            ///     The returned <see cref="IDisposable" /> is used to release the access lock.
            /// </summary>
            IDisposable RequestAccess(FileAccess access, FileShare share);
        }
    }
}