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
    internal interface IStorage
    {
        ///// <summary>
        /////     The current directory used in <see cref="System.IO.Directory.GetCurrentDirectory()" /> and
        /////     <see cref="System.IO.Directory.SetCurrentDirectory(string)" />
        ///// </summary>
        //string CurrentDirectory { get; set; }

        ///// <summary>
        /////     Deletes the <see cref="FileSystemInfoMock" /> on the given <paramref name="path" />.
        ///// </summary>
        //bool Delete(string path, bool recursive = false);

        ///// <summary>
        /////     Checks if a <see cref="FileSystemInfoMock" /> exists on the given <paramref name="path" />.
        ///// </summary>
        //bool Exists([NotNullWhen(true)] string? path);

        ///// <summary>
        /////     Gets a directory if it exists.<br />
        /////     Returns <c>null</c>, if the directory does not exist.
        ///// </summary>
        //IDirectoryInfoMock? GetDirectory(string path);

        ///// <summary>
        /////     Add a file to the storage.
        ///// </summary>
        //bool TryAddFile(string path, [NotNullWhen(true)] out IFileInfoMock? createdFile);

        ///// <summary>
        /////     Gets a file system info if it exists.<br />
        /////     Returns <c>null</c>, if the file system info does not exist.
        ///// </summary>
        //IFileSystemInfoMock? GetFileSystemInfo(string path);

        ///// <summary>
        /////     Returns the drive if it is present.<br />
        /////     Returns <c>null</c>, if the drive does not exist.
        ///// </summary>
        //IDriveInfoMock? GetDrive(string? driveName);

        ///// <summary>
        /////     Returns the drives that are present.
        ///// </summary>
        //IEnumerable<IDriveInfoMock> GetDrives();

        ///// <summary>
        /////     Returns the drives that are present.
        ///// </summary>
        //IDriveInfoMock GetOrAddDrive(string driveName);

        ///// <summary>
        /////     Gets a file if it exists.<br />
        /////     Returns <c>null</c>, if the file does not exist.
        ///// </summary>
        //IFileInfoMock? GetFile(string path);

        ///// <summary>
        /////     Gets or adds a directory.
        ///// </summary>
        //IDirectoryInfoMock? GetOrAddDirectory(string path);

        ///// <summary>
        /////     Gets or adds a file.
        ///// </summary>
        //IFileInfoMock? GetOrAddFile(string path);

        ///// <summary>
        /////     Returns the relative subdirectory path from <paramref name="fullFilePath" /> to the <paramref name="givenPath" />.
        ///// </summary>
        //string GetSubdirectoryPath(string fullFilePath, string givenPath);

        /// <summary>
        ///     An <see cref="IFileSystem.IDirectoryInfo" /> with <see cref="IFileSystemInfoMock" /> functionality.
        /// </summary>
        internal interface IDirectoryInfoMock : IFileSystem.IDirectoryInfo,
            IFileSystemInfoMock
        {
        }

        /// <summary>
        ///     An <see cref="IFileSystem.IFileInfo" /> which allows writing to the underlying byte array.
        /// </summary>
        internal interface IFileInfoMock : IFileSystem.IFileInfo, IFileSystemInfoMock
        {
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
            ///     Gets the bytes in the <see cref="IFileSystem.IFileInfo" />.
            /// </summary>
            byte[] GetBytes();

            /// <summary>
            ///     Writes the <paramref name="bytes" /> to the <see cref="IFileSystem.IFileInfo" />.
            /// </summary>
            void WriteBytes(byte[] bytes);

#if FEATURE_FILESYSTEM_LINK
            /// <summary>
            ///     Sets the link target to <paramref name="pathToTarget" />.
            /// </summary>
            void SetLinkTarget(string pathToTarget);
#endif
        }

        /// <summary>
        ///     An <see cref="IFileSystem.IFileSystemInfo" /> which allows requesting access to.
        /// </summary>
        internal interface IFileSystemInfoMock : IFileSystem.IFileSystemInfo
        {
            /// <summary>
            ///     Requests access to this file with the given <paramref name="share" />.
            ///     <para />
            ///     The returned <see cref="IDisposable" /> is used to release the access lock.
            /// </summary>
            IStorageAccessHandle RequestAccess(FileAccess access, FileShare share);
        }
    }
}