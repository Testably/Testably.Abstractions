using System.IO;
using System.Runtime.Versioning;
using System.Security.AccessControl;

namespace Testably.Abstractions;

public static class FileAclExtensions
{
	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.GetAccessControl(FileInfo)" />
	[SupportedOSPlatform("windows")]
	public static FileSecurity GetAccessControl(
		this IFileSystem.IFile file, string path)
	{
		IFileSystem.IFileInfo fileInfo = file.FileSystem.FileInfo.New(path);
		IFileSystem.IFileSystemExtensionContainer extensionContainer =
			fileInfo.ExtensionContainer;
		if (extensionContainer.HasWrappedInstance(out FileInfo? fi))
		{
			return fi.GetAccessControl();
		}

		return extensionContainer.RetrieveMetadata<FileSecurity>(
			Constants.AccessControl) ?? new FileSecurity();
	}

	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.GetAccessControl(FileInfo, AccessControlSections)" />
	[SupportedOSPlatform("windows")]
	public static FileSecurity GetAccessControl(
		this IFileSystem.IFile file,
		string path,
		AccessControlSections includeSections)
	{
		IFileSystem.IFileInfo fileInfo = file.FileSystem.FileInfo.New(path);
		IFileSystem.IFileSystemExtensionContainer extensionContainer =
			fileInfo.ExtensionContainer;
		if (extensionContainer.HasWrappedInstance(out FileInfo? fi))
		{
			return fi.GetAccessControl(includeSections);
		}

		return extensionContainer.RetrieveMetadata<FileSecurity>(
			Constants.AccessControl) ?? new FileSecurity();
	}

	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.SetAccessControl(FileInfo, FileSecurity)" />
	[SupportedOSPlatform("windows")]
	public static void SetAccessControl(this IFileSystem.IFile file,
	                                    string path,
	                                    FileSecurity fileSecurity)
	{
		IFileSystem.IFileInfo fileInfo = file.FileSystem.FileInfo.New(path);
		IFileSystem.IFileSystemExtensionContainer extensionContainer =
			fileInfo.ExtensionContainer;
		if (extensionContainer.HasWrappedInstance(out FileInfo? fi))
		{
			fi.SetAccessControl(fileSecurity);
		}
		else
		{
			extensionContainer.StoreMetadata(Constants.AccessControl,
				fileSecurity);
		}
	}
}