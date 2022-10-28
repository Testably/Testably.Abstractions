using System.IO;

namespace Testably.Abstractions.Testing.Storage;

/// <summary>
///     A <see cref="IDriveInfo" /> which allows to be manipulated.
/// </summary>
public interface IStorageDrive : IDriveInfo
{
	/// <summary>
	///     Changes the currently used bytes by <paramref name="usedBytesDelta" />.
	///     <para />
	///     Throws an <see cref="IOException" /> if the <see cref="IDriveInfo.AvailableFreeSpace" /> becomes
	///     negative.
	/// </summary>
	IStorageDrive ChangeUsedBytes(long usedBytesDelta);

	/// <summary>
	///     Changes the <see cref="IDriveInfo.DriveFormat" /> of the mocked <see cref="IDriveInfo" />.
	/// </summary>
	IStorageDrive SetDriveFormat(
		string driveFormat = MockFileSystem.DriveInfoMock.DefaultDriveFormat);

	/// <summary>
	///     Changes the <see cref="IDriveInfo.DriveType" /> of the mocked <see cref="IDriveInfo" />.
	/// </summary>
	IStorageDrive SetDriveType(
		DriveType driveType = MockFileSystem.DriveInfoMock.DefaultDriveType);

	/// <summary>
	///     Changes the <see cref="IDriveInfo.IsReady" /> property of the mocked
	///     <see cref="IDriveInfo" />.
	/// </summary>
	IStorageDrive SetIsReady(bool isReady = true);

	/// <summary>
	///     Changes the total size of the mocked <see cref="IDriveInfo" />.
	/// </summary>
	IStorageDrive SetTotalSize(
		long totalSize = MockFileSystem.DriveInfoMock.DefaultTotalSize);
}