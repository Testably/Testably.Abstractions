using System.IO;
using System.Security.AccessControl;

namespace Testably.Abstractions;

/// <summary>
///     ACL (access control list) extension methods for <see cref="IFileInfo" />.
/// </summary>
public static class FileInfoAclExtensions
{
	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.GetAccessControl(FileInfo)" />
	[SupportedOSPlatform("windows")]
	public static FileSecurity GetAccessControl(
		this IFileInfo fileInfo)
	{
		fileInfo.ThrowIfMissing();
		IFileSystemExtensibility extensibility =
			fileInfo.Extensibility;
		return extensibility.TryGetWrappedInstance(out FileInfo? fi)
			? fi.GetAccessControl()
			: extensibility.RetrieveMetadata<FileSecurity>(
				AccessControlHelpers.AccessControl) ?? new FileSecurity();
	}

	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.GetAccessControl(FileInfo, AccessControlSections)" />
	[SupportedOSPlatform("windows")]
	public static FileSecurity GetAccessControl(
		this IFileInfo fileInfo,
		AccessControlSections includeSections)
	{
		fileInfo.ThrowIfMissing();
		IFileSystemExtensibility extensibility =
			fileInfo.Extensibility;
		return extensibility.TryGetWrappedInstance(out FileInfo? fi)
			? fi.GetAccessControl(includeSections)
			: extensibility.RetrieveMetadata<FileSecurity>(
				AccessControlHelpers.AccessControl) ?? new FileSecurity();
	}

	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.SetAccessControl(FileInfo, FileSecurity)" />
	[SupportedOSPlatform("windows")]
	public static void SetAccessControl(this IFileInfo fileInfo,
		FileSecurity fileSecurity)
	{
		IFileSystemExtensibility extensibility =
			fileInfo.Extensibility;
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
