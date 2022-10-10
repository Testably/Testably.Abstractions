using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Testably.Abstractions;

public sealed partial class FileSystem
{
    private sealed class DriveInfoFactory : IFileSystem.IDriveInfoFactory
    {
        internal DriveInfoFactory(FileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        #region IDriveInfoFactory Members

        /// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
        public IFileSystem FileSystem { get; }

        /// <inheritdoc cref="IFileSystem.IDriveInfoFactory.GetDrives()" />
        public IFileSystem.IDriveInfo[] GetDrives()
            => System.IO.DriveInfo.GetDrives()
               .Select(driveInfo => Wrap(driveInfo))
               .ToArray();

        /// <inheritdoc cref="IFileSystem.IDriveInfoFactory.New(string)" />
        public IFileSystem.IDriveInfo New(string driveName)
        {
            if (driveName == null)
            {
                throw new ArgumentNullException(nameof(driveName));
            }

            return DriveInfoWrapper.FromDriveInfo(
                new DriveInfo(driveName),
                FileSystem);
        }

        /// <inheritdoc cref="IFileSystem.IDriveInfoFactory.Wrap(DriveInfo)" />
        [return: NotNullIfNotNull("driveInfo")]
        public IFileSystem.IDriveInfo? Wrap(DriveInfo? driveInfo)
            => DriveInfoWrapper.FromDriveInfo(
                driveInfo,
                FileSystem);

        #endregion
    }
}