using System.IO;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    /// <summary>
    ///     A <see cref="IFileSystem.IDriveInfo" /> which allows to be manipulated.
    /// </summary>
    public interface IStorageDrive : IFileSystem.IDriveInfo
    {
        /// <summary>
        ///     Changes the currently used bytes by <paramref name="usedBytesDelta" />.
        ///     <para />
        ///     Throws an <see cref="IOException" /> if the <see cref="IFileSystem.IDriveInfo.AvailableFreeSpace" /> becomes
        ///     negative.
        /// </summary>
        IStorageDrive ChangeUsedBytes(long usedBytesDelta);

        /// <summary>
        ///     Changes the <see cref="IFileSystem.IDriveInfo.DriveFormat" /> of the mocked <see cref="IFileSystem.IDriveInfo" />.
        /// </summary>
        IStorageDrive SetDriveFormat(
            string driveFormat = DriveInfoMock.DefaultDriveFormat);

        /// <summary>
        ///     Changes the <see cref="IFileSystem.IDriveInfo.DriveType" /> of the mocked <see cref="IFileSystem.IDriveInfo" />.
        /// </summary>
        IStorageDrive SetDriveType(DriveType driveType = DriveInfoMock.DefaultDriveType);

        /// <summary>
        ///     Changes the <see cref="IFileSystem.IDriveInfo.IsReady" /> property of the mocked
        ///     <see cref="IFileSystem.IDriveInfo" />.
        /// </summary>
        IStorageDrive SetIsReady(bool isReady = true);

        /// <summary>
        ///     Changes the total size of the mocked <see cref="IFileSystem.IDriveInfo" />.
        /// </summary>
        IStorageDrive SetTotalSize(long totalSize = DriveInfoMock.DefaultTotalSize);
    }
}