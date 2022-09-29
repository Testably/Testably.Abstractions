using System.IO;

namespace Testably.Abstractions;

public partial interface IFileSystem
{
    /// <summary>
    ///     Factory for abstracting creation of <see cref="System.IO.DriveInfo" />.
    /// </summary>
    public interface IDriveInfoFactory : IFileSystemExtensionPoint
    {
        /// <inheritdoc cref="System.IO.DriveInfo.GetDrives()" />
        IDriveInfo[] GetDrives();

        /// <inheritdoc cref="System.IO.DriveInfo(string)" />
        IDriveInfo New(string driveName);

        /// <summary>
        ///     Wraps the <paramref name="driveInfo" /> to the testable interface <see cref="IDriveInfo" />.
        /// </summary>
        IDriveInfo Wrap(DriveInfo driveInfo);
    }
}