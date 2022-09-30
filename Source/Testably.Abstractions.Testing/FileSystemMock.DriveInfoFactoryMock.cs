using System.IO;
using System.Linq;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    private sealed class DriveInfoFactoryMock : IFileSystem.IDriveInfoFactory
    {
        private readonly FileSystemMock _fileSystem;

        internal DriveInfoFactoryMock(FileSystemMock fileSystem)
        {
            _fileSystem = fileSystem;
        }

        #region IDriveInfoFactory Members

        /// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
        public IFileSystem FileSystem
            => _fileSystem;

        /// <inheritdoc cref="IFileSystem.IDriveInfoFactory.GetDrives()" />
        public IFileSystem.IDriveInfo[] GetDrives()
            => _fileSystem.FileSystemContainer.GetDrives().ToArray();

        /// <inheritdoc cref="IFileSystem.IDriveInfoFactory.New(string)" />
        public IFileSystem.IDriveInfo New(string driveName)
            => new DriveInfoMock(driveName, FileSystem);

        /// <inheritdoc cref="IFileSystem.IDriveInfoFactory.Wrap(DriveInfo)" />
        public IFileSystem.IDriveInfo Wrap(DriveInfo driveInfo)
            => new DriveInfoMock(
                driveInfo.Name,
                FileSystem);

        #endregion
    }
}