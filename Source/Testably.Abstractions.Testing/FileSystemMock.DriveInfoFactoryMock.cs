using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

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

			return DriveInfoMock.New(driveName, _fileSystem);
		}

		/// <inheritdoc cref="IFileSystem.IDriveInfoFactory.Wrap(DriveInfo)" />
		[return: NotNullIfNotNull("driveInfo")]
		public IFileSystem.IDriveInfo? Wrap(DriveInfo? driveInfo)
			=> DriveInfoMock.New(
				driveInfo?.Name,
				_fileSystem);

		#endregion
	}
}