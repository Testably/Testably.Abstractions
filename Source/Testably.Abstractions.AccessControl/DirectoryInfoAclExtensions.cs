using System;
using System.IO;
using System.Security.AccessControl;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions;

/// <summary>
///     ACL (access control list) extension methods for <see cref="IDirectoryInfo" />.
/// </summary>
public static class DirectoryInfoAclExtensions
{
	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.Create(DirectoryInfo, DirectorySecurity)" />
	[SupportedOSPlatform("windows")]
	public static void Create(this IDirectoryInfo directoryInfo,
		DirectorySecurity directorySecurity)
	{
		IFileSystemExtensibility extensibility = directoryInfo.GetExtensibilityOrThrow();
		if (extensibility.TryGetWrappedInstance(out DirectoryInfo? di))
		{
			di.Create(directorySecurity);
		}
		else
		{
			_ = directorySecurity ?? throw new ArgumentNullException(nameof(directorySecurity));
			directoryInfo.ThrowIfParentMissing();
			directoryInfo.Create();
			extensibility.StoreMetadata(AccessControlHelpers.AccessControl,
				directorySecurity);
		}
	}

	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.GetAccessControl(DirectoryInfo)" />
	[SupportedOSPlatform("windows")]
	public static DirectorySecurity GetAccessControl(
		this IDirectoryInfo directoryInfo)
	{
		directoryInfo.ThrowIfMissing();
		IFileSystemExtensibility extensibility = directoryInfo.GetExtensibilityOrThrow();
		return extensibility.TryGetWrappedInstance(out DirectoryInfo? di)
			? di.GetAccessControl()
			: extensibility.RetrieveMetadata<DirectorySecurity>(
				AccessControlHelpers.AccessControl) ?? new DirectorySecurity();
	}

	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.GetAccessControl(DirectoryInfo, AccessControlSections)" />
	[SupportedOSPlatform("windows")]
	public static DirectorySecurity GetAccessControl(
		this IDirectoryInfo directoryInfo,
		AccessControlSections includeSections)
	{
		directoryInfo.ThrowIfMissing();
		IFileSystemExtensibility extensibility = directoryInfo.GetExtensibilityOrThrow();
		return extensibility.TryGetWrappedInstance(out DirectoryInfo? di)
			? di.GetAccessControl(includeSections)
			: extensibility.RetrieveMetadata<DirectorySecurity>(
				AccessControlHelpers.AccessControl) ?? new DirectorySecurity();
	}

	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.SetAccessControl(DirectoryInfo, DirectorySecurity)" />
	[SupportedOSPlatform("windows")]
	public static void SetAccessControl(this IDirectoryInfo directoryInfo,
		DirectorySecurity directorySecurity)
	{
		IFileSystemExtensibility extensibility = directoryInfo.GetExtensibilityOrThrow();
		if (extensibility.TryGetWrappedInstance(out DirectoryInfo? di))
		{
			di.SetAccessControl(directorySecurity);
		}
		else
		{
			extensibility.StoreMetadata(AccessControlHelpers.AccessControl,
				directorySecurity);
		}
	}
}
