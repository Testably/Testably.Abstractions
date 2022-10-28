using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Testably.Abstractions;

public sealed partial class FileSystem
{
	private sealed class DriveInfoFactory : IDriveInfoFactory
	{
		internal DriveInfoFactory(FileSystem fileSystem)
		{
			FileSystem = fileSystem;
		}

		#region IDriveInfoFactory Members

		/// <inheritdoc cref="IFileSystemExtensionPoint.FileSystem" />
		public IFileSystem FileSystem { get; }

		/// <inheritdoc cref="IDriveInfoFactory.GetDrives()" />
		public IDriveInfo[] GetDrives()
			=> System.IO.DriveInfo.GetDrives()
			   .Select(driveInfo => Wrap(driveInfo))
			   .ToArray();

		/// <inheritdoc cref="IDriveInfoFactory.New(string)" />
		public IDriveInfo New(string driveName)
			=> DriveInfoWrapper.FromDriveInfo(
				new DriveInfo(driveName),
				FileSystem);

		/// <inheritdoc cref="IDriveInfoFactory.Wrap(DriveInfo)" />
		[return: NotNullIfNotNull("driveInfo")]
		public IDriveInfo? Wrap(DriveInfo? driveInfo)
			=> DriveInfoWrapper.FromDriveInfo(
				driveInfo,
				FileSystem);

		#endregion
	}
}