using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     Mocked instance of a <see cref="IDriveInfo" />
/// </summary>
internal sealed class DriveInfoMock : IStorageDrive
{
	/// <summary>
	///     The default <see cref="IDriveInfo.DriveFormat" />.
	/// </summary>
	public const string DefaultDriveFormat = "NTFS";

	/// <summary>
	///     The default <see cref="IDriveInfo.DriveType" />.
	/// </summary>
	public const DriveType DefaultDriveType = DriveType.Fixed;

	/// <summary>
	///     The default total size of a mocked <see cref="IStorageDrive" />.
	///     <para />
	///     The number is equal to 1GB (1 Gigabyte).
	/// </summary>
	public const long DefaultTotalSize = 1024 * 1024 * 1024;

	private readonly MockFileSystem _fileSystem;

	private long _usedBytes;
	private string _volumeLabel = nameof(MockFileSystem);
	private readonly string _name;
	private long _totalSize;

	private DriveInfoMock(string driveName, MockFileSystem fileSystem)
	{
		_fileSystem = fileSystem;

		if (driveName.IsUncPath(_fileSystem))
		{
			IsUncPath = true;
			driveName = new string(fileSystem.Path.DirectorySeparatorChar, 2) +
			            GetTopmostParentDirectory(driveName.Substring(2));
		}
		else
		{
			driveName = ValidateDriveLetter(driveName, fileSystem);
		}

		_name = driveName;
		_totalSize = DefaultTotalSize;
		DriveFormat = DefaultDriveFormat;
		DriveType = DefaultDriveType;
		IsReady = true;
	}

	#region IStorageDrive Members

	/// <inheritdoc cref="IDriveInfo.AvailableFreeSpace" />
	public long AvailableFreeSpace
	{
		get
		{
			using IDisposable registration = RegisterProperty(nameof(AvailableFreeSpace), PropertyStatistic.AccessMode.Get);

			return TotalFreeSpace;
		}
	}

	/// <inheritdoc cref="IDriveInfo.DriveFormat" />
	public string DriveFormat { get; private set; }

	/// <inheritdoc cref="IDriveInfo.DriveType" />
	public DriveType DriveType { get; private set; }

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem
		=> _fileSystem;

	/// <inheritdoc cref="IDriveInfo.IsReady" />
	public bool IsReady { get; private set; }

	/// <summary>
	///     Flag indicating if the drive is a UNC drive
	/// </summary>
	public bool IsUncPath { get; }

	/// <inheritdoc cref="IDriveInfo.Name" />
	public string Name
	{
		get
		{
			//using IDisposable registration = RegisterProperty(nameof(Name), PropertyStatistic.AccessMode.Get);

			return _name;
		}
	}

	/// <inheritdoc cref="IDriveInfo.RootDirectory" />
	public IDirectoryInfo RootDirectory
		=> DirectoryInfoMock.New(_fileSystem.Storage.GetLocation(Name), _fileSystem);

	/// <inheritdoc cref="IDriveInfo.TotalFreeSpace" />
	public long TotalFreeSpace
	{
		get
		{
			using IDisposable registration = RegisterProperty(nameof(TotalFreeSpace), PropertyStatistic.AccessMode.Get);

			return _totalSize - _usedBytes;
		}
	}

	/// <inheritdoc cref="IDriveInfo.TotalSize" />
	public long TotalSize
	{
		get
		{
			using IDisposable registration = RegisterProperty(nameof(TotalSize), PropertyStatistic.AccessMode.Get);

			return _totalSize;
		}
	}

	/// <inheritdoc cref="IDriveInfo.VolumeLabel" />
	[AllowNull]
	public string VolumeLabel
	{
		get
		{
			using IDisposable registration = RegisterProperty(nameof(VolumeLabel), PropertyStatistic.AccessMode.Get);

			return _volumeLabel;
		}
		[SupportedOSPlatform("windows")]
		set
		{
			using IDisposable registration = RegisterProperty(nameof(VolumeLabel), PropertyStatistic.AccessMode.Set);

			_volumeLabel = value ?? _volumeLabel;
			_fileSystem.Execute.NotOnWindows(
				() => throw ExceptionFactory.OperationNotSupportedOnThisPlatform());
		}
	}

	/// <inheritdoc cref="IStorageDrive.ChangeUsedBytes(long)" />
	public IStorageDrive ChangeUsedBytes(long usedBytesDelta)
	{
		long newUsedBytes = Math.Max(0, _usedBytes + usedBytesDelta);

		if (newUsedBytes > _totalSize)
		{
			throw ExceptionFactory.NotEnoughDiskSpace(Name);
		}

		_usedBytes = newUsedBytes;

		return this;
	}

	/// <inheritdoc cref="IStorageDrive.SetDriveFormat(string)" />
	public IStorageDrive SetDriveFormat(
		string driveFormat = DefaultDriveFormat)
	{
		DriveFormat = driveFormat;
		return this;
	}

	/// <inheritdoc cref="IStorageDrive.SetDriveType(System.IO.DriveType)" />
	public IStorageDrive SetDriveType(
		DriveType driveType = DefaultDriveType)
	{
		DriveType = driveType;
		return this;
	}

	/// <inheritdoc cref="IStorageDrive.SetIsReady(bool)" />
	public IStorageDrive SetIsReady(bool isReady = true)
	{
		IsReady = isReady;
		return this;
	}

	/// <inheritdoc cref="IStorageDrive.SetTotalSize(long)" />
	public IStorageDrive SetTotalSize(
		long totalSize = DefaultTotalSize)
	{
		_totalSize = totalSize;
		return this;
	}

	#endregion

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> Name;

	private string GetTopmostParentDirectory(string path)
	{
		while (true)
		{
			string? child = FileSystem.Path.GetDirectoryName(path);
			if (string.IsNullOrEmpty(child))
			{
				break;
			}

			path = child;
		}

		return path;
	}

	private static string ValidateDriveLetter(string driveName,
		MockFileSystem fileSystem)
	{
		if (driveName.Length == 1 &&
		    char.IsLetter(driveName, 0))
		{
			return $"{driveName.ToUpperInvariant()}:\\";
		}

		if (fileSystem.Path.IsPathRooted(driveName))
		{
			return fileSystem.Execute.OnWindows(() =>
				{
					string rootedPath = fileSystem.Path.GetPathRoot(driveName)!;
					return $"{rootedPath.TrimEnd('\\')}\\";
				},
				() => fileSystem.Path.GetPathRoot(driveName)!);
		}

		throw ExceptionFactory.InvalidDriveName();
	}

	[return: NotNullIfNotNull("driveName")]
	internal static DriveInfoMock? New(string? driveName,
		MockFileSystem fileSystem)
	{
		if (driveName == null)
		{
			return null;
		}

		return new DriveInfoMock(driveName, fileSystem);
	}

	private IDisposable RegisterProperty(string name, PropertyStatistic.AccessMode mode)
		=> _fileSystem.StatisticsRegistration.DriveInfo.RegisterProperty(_name, name, mode);
}
