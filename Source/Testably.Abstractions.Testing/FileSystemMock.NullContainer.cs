﻿using System;
using System.IO;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    internal sealed class NullContainer : IStorageContainer
    {
        /// <summary>
        ///     The default time returned by the file system if no time has been set.
        ///     <seealso href="https://learn.microsoft.com/en-us/windows/win32/sysinfo/file-times" />:
        ///     A file time is a 64-bit value that represents the number of 100-nanosecond intervals that have elapsed
        ///     since 12:00 A.M. January 1, 1601 Coordinated Universal Time (UTC).
        /// </summary>
        private static readonly DateTime NullTime =
            new(1601, 01, 01, 00, 00, 00, DateTimeKind.Utc);

        public static IStorageContainer Instance => new NullContainer();

        #region IStorageContainer Members

        /// <inheritdoc cref="IStorageContainer.Attributes" />
        public FileAttributes Attributes
        {
            get => (FileAttributes)(-1);
            set => _ = value;
        }

        /// <inheritdoc cref="IStorageContainer.CreationTime" />
        public DateTime CreationTime
        {
            get => NullTime;
            set => _ = value;
        }

        /// <inheritdoc cref="IStorageContainer.LastAccessTime" />
        public DateTime LastAccessTime
        {
            get => NullTime;
            set => _ = value;
        }

        /// <inheritdoc cref="IStorageContainer.LastWriteTime" />
        public DateTime LastWriteTime
        {
            get => NullTime;
            set => _ = value;
        }

        /// <inheritdoc cref="IStorageContainer.LinkTarget" />
        public string? LinkTarget
        {
            get => null;
            set => _ = value;
        }

        /// <inheritdoc cref="IStorageContainer.Type" />
        public InMemoryContainer.ContainerType Type
            => InMemoryContainer.ContainerType.Unknown;

        /// <inheritdoc cref="IStorageContainer.AdjustTimes(IStorageContainer.TimeAdjustments)" />
        public void AdjustTimes(IStorageContainer.TimeAdjustments timeAdjustments)
        {
            // Ignore in NullContainer TODO
        }

        /// <inheritdoc />
        public void AppendBytes(byte[] bytes)
            => throw new NotImplementedException();

        /// <inheritdoc />
        public void ClearBytes()
            => throw new NotImplementedException();

        /// <inheritdoc />
        public void Decrypt()
            => throw new NotImplementedException();

        /// <inheritdoc />
        public void Encrypt()
            => throw new NotImplementedException();

        /// <inheritdoc />
        public byte[] GetBytes()
            => throw new NotImplementedException();

        /// <inheritdoc cref="IStorageContainer.RequestAccess(FileAccess, FileShare)" />
        public IStorageAccessHandle RequestAccess(FileAccess access, FileShare share)
            => new NullStorageAccessHandle();

        /// <inheritdoc />
        public void WriteBytes(byte[] bytes)
            => throw new NotImplementedException();

        #endregion

        private sealed class NullStorageAccessHandle : IStorageAccessHandle
        {
            #region IStorageAccessHandle Members

            /// <inheritdoc cref="IStorageAccessHandle.Access" />
            public FileAccess Access => FileAccess.ReadWrite;

            /// <inheritdoc cref="IStorageAccessHandle.Share" />
            public FileShare Share => FileShare.None;

            /// <inheritdoc cref="IDisposable.Dispose()" />
            public void Dispose()
            {
            }

            #endregion
        }
    }
}