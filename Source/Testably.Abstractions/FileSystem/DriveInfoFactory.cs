﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Testably.Abstractions.FileSystem;

internal sealed class DriveInfoFactory : IDriveInfoFactory
{
	internal DriveInfoFactory(RealFileSystem fileSystem)
	{
		FileSystem = fileSystem;
	}

	#region IDriveInfoFactory Members

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem { get; }

	/// <inheritdoc cref="IDriveInfoFactory.FromDriveName(string)" />
	[Obsolete("Use `IDriveInfoFactory.New(string)` instead")]
	public IDriveInfo FromDriveName(string driveName)
		=> New(driveName);

	/// <inheritdoc cref="IDriveInfoFactory.GetDrives()" />
	public IDriveInfo[] GetDrives()
		=> DriveInfo.GetDrives()
			// ReSharper disable once ConvertClosureToMethodGroup -- Not possible due to nullable
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
