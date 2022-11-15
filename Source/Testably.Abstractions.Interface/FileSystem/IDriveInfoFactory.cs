using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions.FileSystem;

/// <summary>
///     Factory for abstracting the creation of <see cref="DriveInfo" />.
/// </summary>
public interface IDriveInfoFactory : IFileSystemEntity
{
	/// <inheritdoc cref="DriveInfo.GetDrives()" />
	IDriveInfo[] GetDrives();

	/// <inheritdoc cref="DriveInfo(string)" />
	IDriveInfo New(string driveName);

	/// <summary>
	///     Wraps the <paramref name="driveInfo" /> to the testable interface <see cref="IDriveInfo" />.
	/// </summary>
	[return: NotNullIfNotNull("driveInfo")]
	IDriveInfo? Wrap(DriveInfo? driveInfo);
}
