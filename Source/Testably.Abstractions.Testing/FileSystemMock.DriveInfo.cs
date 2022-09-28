using System;
using System.IO;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    private sealed class DriveInfoWrapper : IFileSystem.IDriveInfo
    {
        private readonly DriveInfo _driveInfo;

        internal DriveInfoWrapper(DriveInfo driveInfo, IFileSystem fileSystem)
        {
            _driveInfo = driveInfo;
            FileSystem = fileSystem;
        }

        #region IDriveInfo Members

        /// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
        public IFileSystem FileSystem { get; }

        /// <inheritdoc cref="IFileSystem.IDriveInfo.AvailableFreeSpace" />
        public long AvailableFreeSpace
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IDriveInfo.DriveFormat" />
        public string DriveFormat
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IDriveInfo.DriveType" />
        public DriveType DriveType
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IDriveInfo.IsReady" />
        public bool IsReady
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IDriveInfo.Name" />
        public string Name
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IDriveInfo.RootDirectory" />
        public IFileSystem.IDirectoryInfo RootDirectory
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IDriveInfo.TotalFreeSpace" />
        public long TotalFreeSpace
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IDriveInfo.TotalSize" />
        public long TotalSize
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IDriveInfo.VolumeLabel" />
        public string VolumeLabel
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        #endregion
    }
}