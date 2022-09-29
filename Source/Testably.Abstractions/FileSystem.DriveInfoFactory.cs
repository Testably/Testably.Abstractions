using System.IO;
using System.Linq;
using Testably.Abstractions.Models;

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
            => System.IO.DriveInfo.GetDrives().Select(Wrap).ToArray();

        /// <inheritdoc cref="IFileSystem.IDriveInfoFactory.New" />
        public IFileSystem.IDriveInfo New(string path)
            => new DriveInfoWrapper(
                new DriveInfo(path),
                FileSystem);

        /// <inheritdoc cref="IFileSystem.IDriveInfoFactory.Wrap(DriveInfo)" />
        public IFileSystem.IDriveInfo Wrap(DriveInfo driveInfo)
            => new DriveInfoWrapper(
                driveInfo,
                FileSystem);

        #endregion
    }
}