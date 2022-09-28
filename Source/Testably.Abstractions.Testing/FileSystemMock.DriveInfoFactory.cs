using System.IO;

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
            => throw new System.NotImplementedException();

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