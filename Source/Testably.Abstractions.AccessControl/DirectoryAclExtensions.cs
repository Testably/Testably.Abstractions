using System;
using System.IO;
using System.Security.AccessControl;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions;

/// <summary>
///     ACL (access control list) extension methods for <see cref="IDirectory" />.
/// </summary>
public static class DirectoryAclExtensions
{
	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.CreateDirectory(DirectorySecurity, string)" />
	[SupportedOSPlatform("windows")]
	public static void CreateDirectory(this IDirectory directory,
		string path,
		DirectorySecurity directorySecurity)
	{
		IDirectoryInfo directoryInfo =
			directory.FileSystem.DirectoryInfo.New(path);
		IFileSystemExtensibility extensibility = directoryInfo as IFileSystemExtensibility
		                                         ?? throw new NotSupportedException(
			                                         $"{directoryInfo.GetType()} does not support IFileSystemExtensibility.");
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
		this IDirectory directory, string path)
	{
		IDirectoryInfo directoryInfo =
			directory.FileSystem.DirectoryInfo.New(path);
		directoryInfo.ThrowIfMissing();
		IFileSystemExtensibility extensibility = directoryInfo as IFileSystemExtensibility
		                                         ?? throw new NotSupportedException(
			                                         $"{directoryInfo.GetType()} does not support IFileSystemExtensibility.");
		return extensibility.TryGetWrappedInstance(out DirectoryInfo? di)
			? di.GetAccessControl()
			: extensibility.RetrieveMetadata<DirectorySecurity>(
				AccessControlHelpers.AccessControl) ?? new DirectorySecurity();
	}

	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.GetAccessControl(DirectoryInfo, AccessControlSections)" />
	[SupportedOSPlatform("windows")]
	public static DirectorySecurity GetAccessControl(
		this IDirectory directory,
		string path,
		AccessControlSections includeSections)
	{
		IDirectoryInfo directoryInfo =
			directory.FileSystem.DirectoryInfo.New(path);
		directoryInfo.ThrowIfMissing();
		IFileSystemExtensibility extensibility = directoryInfo as IFileSystemExtensibility
		                                         ?? throw new NotSupportedException(
			                                         $"{directoryInfo.GetType()} does not support IFileSystemExtensibility.");
		return extensibility.TryGetWrappedInstance(out DirectoryInfo? di)
			? di.GetAccessControl(includeSections)
			: extensibility.RetrieveMetadata<DirectorySecurity>(
				AccessControlHelpers.AccessControl) ?? new DirectorySecurity();
	}

	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.SetAccessControl(DirectoryInfo, DirectorySecurity)" />
	[SupportedOSPlatform("windows")]
	public static void SetAccessControl(this IDirectory directory,
		string path,
		DirectorySecurity directorySecurity)
	{
		IDirectoryInfo directoryInfo =
			directory.FileSystem.DirectoryInfo.New(path);
		IFileSystemExtensibility extensibility = directoryInfo as IFileSystemExtensibility
		                                         ?? throw new NotSupportedException(
			                                         $"{directoryInfo.GetType()} does not support IFileSystemExtensibility.");
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
