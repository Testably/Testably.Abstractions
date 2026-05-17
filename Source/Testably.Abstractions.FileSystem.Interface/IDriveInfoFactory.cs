using System;
using System.IO;
using System.Diagnostics.CodeAnalysis;

namespace Testably.Abstractions
{
/// <summary>
///     A factory for the creation of wrappers for <see cref="DriveInfo" /> in a <see cref="IFileSystem" />.
/// </summary>
public interface IDriveInfoFactory : IFileSystemEntity
{
	/// <summary>
	///     Retrieves the drive names of all logical drives on a computer.
	/// </summary>
	/// <returns>An array of type <see cref="IDriveInfo" /> that represents the logical drives on a computer.</returns>
	IDriveInfo[] GetDrives();

	/// <summary>
	///     Provides access to the information on the specified drive.
	/// </summary>
	/// <param name="driveName">
	///     A valid drive path or drive letter.
	///     This can be either uppercase or lowercase, 'a' to 'z'.
	///     A <see langword="null" /> value is not valid.
	/// </param>
	IDriveInfo New(string driveName);

	/// <summary>
	///     Wraps the <paramref name="driveInfo" /> in a wrapper for <see cref="DriveInfo" /> which implements
	///     <see cref="IDriveInfo" />.
	/// </summary>
	[return: NotNullIfNotNull("driveInfo")]
	IDriveInfo? Wrap(DriveInfo? driveInfo);
}
}

namespace System.IO.Abstractions
{
	/// <summary>
	///     Backwards-compatibility alias for <see cref="Testably.Abstractions.IDriveInfoFactory" />.
	/// </summary>
	public interface IDriveInfoFactory : Testably.Abstractions.IDriveInfoFactory
	{
	}
}