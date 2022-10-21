using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Versioning;

namespace Testably.Abstractions;

public sealed partial class FileSystem
{
	private sealed class DriveInfoWrapper : IFileSystem.IDriveInfo
	{
		private readonly DriveInfo _instance;

		private DriveInfoWrapper(DriveInfo driveInfo, IFileSystem fileSystem)
		{
			_instance = driveInfo;
			FileSystem = fileSystem;
		}

		#region IDriveInfo Members

		/// <inheritdoc cref="IFileSystem.IDriveInfo.AvailableFreeSpace" />
		public long AvailableFreeSpace
			=> _instance.AvailableFreeSpace;

		/// <inheritdoc cref="IFileSystem.IDriveInfo.DriveFormat" />
		public string DriveFormat
			=> _instance.DriveFormat;

		/// <inheritdoc cref="IFileSystem.IDriveInfo.DriveType" />
		public DriveType DriveType
			=> _instance.DriveType;

		/// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
		public IFileSystem FileSystem { get; }

		/// <inheritdoc cref="IFileSystem.IDriveInfo.IsReady" />
		public bool IsReady
			=> _instance.IsReady;

		/// <inheritdoc cref="IFileSystem.IDriveInfo.Name" />
		public string Name
			=> _instance.Name;

		/// <inheritdoc cref="IFileSystem.IDriveInfo.RootDirectory" />
		public IFileSystem.IDirectoryInfo RootDirectory
			=> DirectoryInfoWrapper.FromDirectoryInfo(
				_instance.RootDirectory,
				FileSystem);

		/// <inheritdoc cref="IFileSystem.IDriveInfo.TotalFreeSpace" />
		public long TotalFreeSpace
			=> _instance.TotalFreeSpace;

		/// <inheritdoc cref="IFileSystem.IDriveInfo.TotalSize" />
		public long TotalSize
			=> _instance.TotalSize;

		/// <inheritdoc cref="IFileSystem.IDriveInfo.VolumeLabel" />
		[AllowNull]
		public string VolumeLabel
		{
			get => _instance.VolumeLabel;
			[SupportedOSPlatform("windows")]
#pragma warning disable CA1416
			set => _instance.VolumeLabel = value;
#pragma warning restore CA1416
		}

#endregion

		[return: NotNullIfNotNull("instance")]
		internal static DriveInfoWrapper? FromDriveInfo(DriveInfo? instance,
		                                                IFileSystem fileSystem)
		{
			if (instance == null)
			{
				return null;
			}

			return new DriveInfoWrapper(instance, fileSystem);
		}
	}
}