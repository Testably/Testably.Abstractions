using System.IO;
using System.Security.AccessControl;
using Testably.Abstractions.FileSystem;

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
		IFileSystemExtensionContainer extensionContainer =
			fileInfo.ExtensionContainer;
		return extensionContainer.HasWrappedInstance(out FileInfo? fi)
			? fi.GetAccessControl()
			: extensionContainer.RetrieveMetadata<FileSecurity>(
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
		IFileSystemExtensionContainer extensionContainer =
			fileInfo.ExtensionContainer;
		return extensionContainer.HasWrappedInstance(out FileInfo? fi)
			? fi.GetAccessControl(includeSections)
			: extensionContainer.RetrieveMetadata<FileSecurity>(
				AccessControlHelpers.AccessControl) ?? new FileSecurity();
	}

	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.SetAccessControl(FileInfo, FileSecurity)" />
	[SupportedOSPlatform("windows")]
	public static void SetAccessControl(this IFile file,
		string path,
		FileSecurity fileSecurity)
	{
		IFileInfo fileInfo = file.FileSystem.FileInfo.New(path);
		IFileSystemExtensionContainer extensionContainer =
			fileInfo.ExtensionContainer;
		if (extensionContainer.HasWrappedInstance(out FileInfo? fi))
		{
			fi.SetAccessControl(fileSecurity);
		}
		else
		{
			extensionContainer.StoreMetadata(AccessControlHelpers.AccessControl,
				fileSecurity);
		}
	}
}
