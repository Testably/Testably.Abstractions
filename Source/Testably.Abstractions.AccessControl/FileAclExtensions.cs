using System;
using System.IO;
using System.Security.AccessControl;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions;

/// <summary>
///     ACL (access control list) extension methods for <see cref="IFile" />.
/// </summary>
public static class FileAclExtensions
{
	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.GetAccessControl(FileInfo)" />
	[SupportedOSPlatform("windows")]
	public static FileSecurity GetAccessControl(
		this IFile file, string path)
	{
		IFileInfo fileInfo = file.FileSystem.FileInfo.New(path);
		fileInfo.ThrowIfMissing();
		IFileSystemExtensibility extensibility = fileInfo as IFileSystemExtensibility
		                                         ?? throw new NotSupportedException(
			                                         $"{fileInfo.GetType()} does not support IFileSystemExtensibility.");
		return extensibility.TryGetWrappedInstance(out FileInfo? fi)
			? fi.GetAccessControl()
			: extensibility.RetrieveMetadata<FileSecurity>(
				AccessControlHelpers.AccessControl) ?? new FileSecurity();
	}

	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.GetAccessControl(FileInfo, AccessControlSections)" />
	[SupportedOSPlatform("windows")]
	public static FileSecurity GetAccessControl(
		this IFile file,
		string path,
		AccessControlSections includeSections)
	{
		IFileInfo fileInfo = file.FileSystem.FileInfo.New(path);
		fileInfo.ThrowIfMissing();
		IFileSystemExtensibility extensibility = fileInfo as IFileSystemExtensibility
		                                         ?? throw new NotSupportedException(
			                                         $"{fileInfo.GetType()} does not support IFileSystemExtensibility.");
		return extensibility.TryGetWrappedInstance(out FileInfo? fi)
			? fi.GetAccessControl(includeSections)
			: extensibility.RetrieveMetadata<FileSecurity>(
				AccessControlHelpers.AccessControl) ?? new FileSecurity();
	}

	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.SetAccessControl(FileInfo, FileSecurity)" />
	[SupportedOSPlatform("windows")]
	public static void SetAccessControl(this IFile file,
		string path,
		FileSecurity fileSecurity)
	{
		IFileInfo fileInfo = file.FileSystem.FileInfo.New(path);
		IFileSystemExtensibility extensibility = fileInfo as IFileSystemExtensibility
		                                         ?? throw new NotSupportedException(
			                                         $"{fileInfo.GetType()} does not support IFileSystemExtensibility.");
		if (extensibility.TryGetWrappedInstance(out FileInfo? fi))
		{
			fi.SetAccessControl(fileSecurity);
		}
		else
		{
			extensibility.StoreMetadata(AccessControlHelpers.AccessControl,
				fileSecurity);
		}
	}
}
