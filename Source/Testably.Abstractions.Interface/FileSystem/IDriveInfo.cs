using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Versioning;

namespace Testably.Abstractions.FileSystem;

/// <summary>
///     Abstractions for <see cref="DriveInfo" />.
/// </summary>
public interface IDriveInfo : IFileSystemExtensionPoint
{
	/// <inheritdoc cref="DriveInfo.AvailableFreeSpace" />
	long AvailableFreeSpace { get; }

	/// <inheritdoc cref="DriveInfo.DriveFormat" />
	string DriveFormat { get; }

	/// <inheritdoc cref="DriveInfo.DriveType" />
	DriveType DriveType { get; }

	/// <inheritdoc cref="DriveInfo.IsReady" />
	bool IsReady { get; }

	/// <inheritdoc cref="DriveInfo.Name" />
	string Name { get; }

	/// <inheritdoc cref="DriveInfo.RootDirectory" />
	IDirectoryInfo RootDirectory { get; }

	/// <inheritdoc cref="DriveInfo.TotalFreeSpace" />
	long TotalFreeSpace { get; }

	/// <inheritdoc cref="DriveInfo.TotalSize" />
	long TotalSize { get; }

	/// <inheritdoc cref="DriveInfo.VolumeLabel" />
	[AllowNull]
	string VolumeLabel
	{
		get;
		[SupportedOSPlatform("windows")]
		set;
	}
}