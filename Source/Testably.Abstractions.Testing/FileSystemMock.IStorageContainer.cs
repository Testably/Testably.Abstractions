using System;
using System.IO;
using static Testably.Abstractions.Testing.FileSystemMock.InMemoryContainer;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    internal interface IStorageContainer
    {
        ContainerType Type { get; }
        FileAttributes Attributes { get; set; }

        DateTime CreationTime { get; set; }

        public DateTime LastAccessTime { get; set; }

        public DateTime LastWriteTime { get; set; }
        public string? LinkTarget { get; set; }

        void AdjustTimes(TimeAdjustments timeAdjustments);
        IStorageAccessHandle RequestAccess(FileAccess access, FileShare share);


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

        /// <summary>
        ///     Flags indicating which times to adjust for a <see cref="FileSystemMock.FileSystemInfoMock" />.
        /// </summary>
        /// .
        [Flags]
        internal enum TimeAdjustments
        {
            /// <summary>
            ///     Adjusts no times on the <see cref="FileSystemMock.FileSystemInfoMock" />
            /// </summary>
            None = 0,

            /// <summary>
            ///     Adjusts the <see cref="FileSystemMock.FileSystemInfoMock.CreationTime" />
            /// </summary>
            CreationTime = 1 << 0,

            /// <summary>
            ///     Adjusts the <see cref="FileSystemMock.FileSystemInfoMock.LastAccessTime" />
            /// </summary>
            LastAccessTime = 1 << 1,

            /// <summary>
            ///     Adjusts the <see cref="FileSystemMock.FileSystemInfoMock.LastWriteTime" />
            /// </summary>
            LastWriteTime = 1 << 2,

            /// <summary>
            ///     Adjusts all times on the <see cref="FileSystemMock.FileSystemInfoMock" />
            /// </summary>
            All = CreationTime | LastAccessTime | LastWriteTime,
        }
    }
}