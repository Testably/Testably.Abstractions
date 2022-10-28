using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing;

public sealed partial class MockFileSystem
{
	private sealed class DriveInfoFactoryMock : IDriveInfoFactory
	{
		private readonly MockFileSystem _fileSystem;

		internal DriveInfoFactoryMock(MockFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		#region IDriveInfoFactory Members

		/// <inheritdoc cref="IFileSystemExtensionPoint.FileSystem" />
		public IFileSystem FileSystem
			=> _fileSystem;

		/// <inheritdoc cref="IDriveInfoFactory.GetDrives()" />
		public IDriveInfo[] GetDrives()
			=> _fileSystem.Storage.GetDrives()
			   .Cast<IDriveInfo>()
			   .ToArray();

		/// <inheritdoc cref="IDriveInfoFactory.New(string)" />
		public IDriveInfo New(string driveName)
		{
			if (driveName == null)
			{
				throw new ArgumentNullException(nameof(driveName));
			}

			DriveInfoMock driveInfo = DriveInfoMock.New(driveName, _fileSystem);
			IStorageDrive? existingDrive = _fileSystem.Storage.GetDrive(driveInfo.Name);
			return existingDrive ?? driveInfo;
		}

		/// <inheritdoc cref="IDriveInfoFactory.Wrap(DriveInfo)" />
		[return: NotNullIfNotNull("driveInfo")]
		public IDriveInfo? Wrap(DriveInfo? driveInfo)
		{
			if (driveInfo?.Name == null)
			{
				return null;
			}

			return New(driveInfo.Name);
		}

		#endregion
	}
}