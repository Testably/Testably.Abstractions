using System.Diagnostics.CodeAnalysis;
using System.IO;
using Testably.Abstractions.Models;
#if NET6_0_OR_GREATER
using System;
using System.Runtime.Versioning;
#endif

namespace Testably.Abstractions;

public sealed partial class FileSystem
{
    private sealed class DriveInfoWrapper : IFileSystem.IDriveInfo
    {
        private readonly DriveInfo _instance;

        internal DriveInfoWrapper(DriveInfo driveInfo, IFileSystem fileSystem)
        {
            _instance = driveInfo;
            FileSystem = fileSystem;
        }

        #region IDriveInfo Members

        /// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
        public IFileSystem FileSystem { get; }

        /// <inheritdoc cref="IFileSystem.IDriveInfo.AvailableFreeSpace" />
        public long AvailableFreeSpace
            => _instance.AvailableFreeSpace;

        /// <inheritdoc cref="IFileSystem.IDriveInfo.DriveFormat" />
        public string DriveFormat
            => _instance.DriveFormat;

        /// <inheritdoc cref="IFileSystem.IDriveInfo.DriveType" />
        public DriveType DriveType
            => _instance.DriveType;

        /// <inheritdoc cref="IFileSystem.IDriveInfo.IsReady" />
        public bool IsReady
            => _instance.IsReady;

        /// <inheritdoc cref="IFileSystem.IDriveInfo.Name" />
        public string Name
            => _instance.Name;

        /// <inheritdoc cref="IFileSystem.IDriveInfo.RootDirectory" />
        public IFileSystem.IDirectoryInfo RootDirectory
            => DirectoryInfoWrapper.FromDirectoryInfo(
                _instance.RootDirectory,
                FileSystem);

        /// <inheritdoc cref="IFileSystem.IDriveInfo.TotalFreeSpace" />
        public long TotalFreeSpace
            => _instance.TotalFreeSpace;

        /// <inheritdoc cref="IFileSystem.IDriveInfo.TotalSize" />
        public long TotalSize
            => _instance.TotalSize;

        /// <inheritdoc cref="IFileSystem.IDriveInfo.VolumeLabel" />
        [AllowNull]
        public string VolumeLabel
        {
            get => _instance.VolumeLabel;
#if NET6_0_OR_GREATER
            [SupportedOSPlatform("windows")]
            set
            {
                if (OperatingSystem.IsWindows())
                {
                    _instance.VolumeLabel = value;
                }

            }
#else
            set => _instance.VolumeLabel = value;
#endif
        }

        #endregion
    }
}