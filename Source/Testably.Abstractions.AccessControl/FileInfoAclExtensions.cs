using System.IO;
using System.Runtime.Versioning;
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
		IFileSystemExtensionContainer extensionContainer =
			fileInfo.ExtensionContainer;
		return extensionContainer.HasWrappedInstance(out FileInfo? fi)
			? fi.GetAccessControl()
			: extensionContainer.RetrieveMetadata<FileSecurity>(
				AccessControlConstants.AccessControl) ?? new FileSecurity();
	}

	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.GetAccessControl(FileInfo, AccessControlSections)" />
	[SupportedOSPlatform("windows")]
	public static FileSecurity GetAccessControl(
		this IFileInfo fileInfo,
		AccessControlSections includeSections)
	{
		IFileSystemExtensionContainer extensionContainer =
			fileInfo.ExtensionContainer;
		return extensionContainer.HasWrappedInstance(out FileInfo? fi)
			? fi.GetAccessControl(includeSections)
			: extensionContainer.RetrieveMetadata<FileSecurity>(
				AccessControlConstants.AccessControl) ?? new FileSecurity();
	}

	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.SetAccessControl(FileInfo, FileSecurity)" />
	[SupportedOSPlatform("windows")]
	public static void SetAccessControl(this IFileInfo fileInfo,
	                                    FileSecurity fileSecurity)
	{
		IFileSystemExtensionContainer extensionContainer =
			fileInfo.ExtensionContainer;
		if (extensionContainer.HasWrappedInstance(out FileInfo? fi))
		{
			fi.SetAccessControl(fileSecurity);
		}
		else
		{
			extensionContainer.StoreMetadata(AccessControlConstants.AccessControl,
				fileSecurity);
		}
	}
}