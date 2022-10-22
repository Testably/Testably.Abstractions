using System.IO;
using System.Runtime.Versioning;
using System.Security.AccessControl;

namespace Testably.Abstractions;

public static class FileInfoAclExtensions
{
	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.GetAccessControl(FileInfo)" />
	[SupportedOSPlatform("windows")]
	public static FileSecurity GetAccessControl(
		this IFileSystem.IFileInfo fileInfo)
	{
		IFileSystem.IFileSystemExtensionContainer extensionContainer =
			fileInfo.ExtensionContainer;
		if (extensionContainer.HasWrappedInstance(out FileInfo? fi))
		{
			return fi.GetAccessControl();
		}

		return extensionContainer.RetrieveMetadata<FileSecurity>(
			AccessControlConstants.AccessControl) ?? new FileSecurity();
	}

	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.GetAccessControl(FileInfo, AccessControlSections)" />
	[SupportedOSPlatform("windows")]
	public static FileSecurity GetAccessControl(
		this IFileSystem.IFileInfo fileInfo,
		AccessControlSections includeSections)
	{
		IFileSystem.IFileSystemExtensionContainer extensionContainer =
			fileInfo.ExtensionContainer;
		if (extensionContainer.HasWrappedInstance(out FileInfo? fi))
		{
			return fi.GetAccessControl(includeSections);
		}

		return extensionContainer.RetrieveMetadata<FileSecurity>(
			AccessControlConstants.AccessControl) ?? new FileSecurity();
	}

	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.SetAccessControl(FileInfo, FileSecurity)" />
	[SupportedOSPlatform("windows")]
	public static void SetAccessControl(this IFileSystem.IFileInfo fileInfo,
	                                    FileSecurity fileSecurity)
	{
		IFileSystem.IFileSystemExtensionContainer extensionContainer =
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