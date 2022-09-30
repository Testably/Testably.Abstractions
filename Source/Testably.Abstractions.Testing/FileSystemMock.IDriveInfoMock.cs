using System.IO;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    /// <summary>
    ///     A <see cref="IFileSystem.IDriveInfo" /> which allows to be manipulated.
    /// </summary>
    public interface IDriveInfoMock : IFileSystem.IDriveInfo
    {
        /// <summary>
        ///     Changes the currently used bytes by <paramref name="usedBytesDelta" />.
        ///     <para />
        ///     Throws an <see cref="IOException" /> if the <see cref="IFileSystem.IDriveInfo.AvailableFreeSpace" /> becomes
        ///     negative.
        /// </summary>
        IDriveInfoMock ChangeUsedBytes(long usedBytesDelta);

        /// <summary>
        ///     Changes the <see cref="IFileSystem.IDriveInfo.DriveFormat" /> of the mocked <see cref="IFileSystem.IDriveInfo" />.
        /// </summary>
        IDriveInfoMock SetDriveFormat(
            string driveFormat = DriveInfoMock.DefaultDriveFormat);

        /// <summary>
        ///     Changes the <see cref="IFileSystem.IDriveInfo.DriveType" /> of the mocked <see cref="IFileSystem.IDriveInfo" />.
        /// </summary>
        IDriveInfoMock SetDriveType(DriveType driveType = DriveInfoMock.DefaultDriveType);

        /// <summary>
        ///     Changes the <see cref="IFileSystem.IDriveInfo.IsReady" /> property of the mocked
        ///     <see cref="IFileSystem.IDriveInfo" />.
        /// </summary>
        IDriveInfoMock SetIsReady(bool isReady = true);

        /// <summary>
        ///     Changes the total size of the mocked <see cref="IFileSystem.IDriveInfo" />.
        /// </summary>
        IDriveInfoMock SetTotalSize(long totalSize = DriveInfoMock.DefaultTotalSize);
    }
}