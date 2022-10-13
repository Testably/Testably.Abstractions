using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Storage;

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
			=> _fileSystem.Storage.GetDrives()
			   .Cast<IFileSystem.IDriveInfo>()
			   .ToArray();

		/// <inheritdoc cref="IFileSystem.IDriveInfoFactory.New(string)" />
		public IFileSystem.IDriveInfo New(string driveName)
		{
			if (driveName == null)
			{
				throw new ArgumentNullException(nameof(driveName));
			}

			DriveInfoMock driveInfo = DriveInfoMock.New(driveName, _fileSystem);
			IStorageDrive? existingDrive = _fileSystem.Storage.GetDrive(driveInfo.Name);
			return existingDrive ?? driveInfo;
		}

		/// <inheritdoc cref="IFileSystem.IDriveInfoFactory.Wrap(DriveInfo)" />
		[return: NotNullIfNotNull("driveInfo")]
		public IFileSystem.IDriveInfo? Wrap(DriveInfo? driveInfo)
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