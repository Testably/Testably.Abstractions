using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.FileSystem;

internal sealed class DriveInfoFactoryMock : IDriveInfoFactory
{
	private readonly MockFileSystem _fileSystem;

	internal DriveInfoFactoryMock(MockFileSystem fileSystem)
	{
		_fileSystem = fileSystem;
	}

	#region IDriveInfoFactory Members

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem
		=> _fileSystem;

	/// <inheritdoc cref="IDriveInfoFactory.GetDrives()" />
	public IDriveInfo[] GetDrives()
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DriveInfo.RegisterMethod(nameof(GetDrives));

		return _fileSystem.Storage.GetDrives()
			.Where(x => !x.IsUncPath)
			.Cast<IDriveInfo>()
			.OrderBy(x => x.Name)
			.ToArray();
	}

	/// <inheritdoc cref="IDriveInfoFactory.New(string)" />
	public IDriveInfo New(string driveName)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DriveInfo.RegisterMethod(nameof(New),
				driveName);

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
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DriveInfo.RegisterMethod(nameof(Wrap),
				driveInfo);

		if (_fileSystem.SimulationMode != SimulationMode.Native)
		{
			throw new NotSupportedException(
				"Wrapping a DriveInfo in a simulated file system is not supported!");
		}

		if (driveInfo?.Name == null)
		{
			return null;
		}

		return New(driveInfo.Name);
	}

	#endregion
}
