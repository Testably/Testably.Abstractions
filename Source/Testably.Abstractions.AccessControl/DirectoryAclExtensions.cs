using System.IO;
using System.Runtime.Versioning;
using System.Security.AccessControl;

namespace Testably.Abstractions;

/// <summary>
///     ACL (access control list) extension methods for <see cref="IFileSystem.IDirectory" />.
/// </summary>
public static class DirectoryAclExtensions
{
	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.CreateDirectory(DirectorySecurity, string)" />
	[SupportedOSPlatform("windows")]
	public static void CreateDirectory(this IFileSystem.IDirectory directory,
	                                   string path,
	                                   DirectorySecurity directorySecurity)
	{
		IFileSystem.IDirectoryInfo directoryInfo =
			directory.FileSystem.DirectoryInfo.New(path);
		IFileSystem.IFileSystemExtensionContainer extensionContainer =
			directoryInfo.ExtensionContainer;
		if (extensionContainer.HasWrappedInstance(out DirectoryInfo? di))
		{
			di.Create(directorySecurity);
		}
		else
		{
			extensionContainer.StoreMetadata(AccessControlConstants.AccessControl,
				directorySecurity);
			directoryInfo.Create();
		}
	}

	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.GetAccessControl(DirectoryInfo)" />
	[SupportedOSPlatform("windows")]
	public static DirectorySecurity GetAccessControl(
		this IFileSystem.IDirectory directory, string path)
	{
		IFileSystem.IDirectoryInfo directoryInfo =
			directory.FileSystem.DirectoryInfo.New(path);
		IFileSystem.IFileSystemExtensionContainer extensionContainer =
			directoryInfo.ExtensionContainer;
		if (extensionContainer.HasWrappedInstance(out DirectoryInfo? di))
		{
			return di.GetAccessControl();
		}

		return extensionContainer.RetrieveMetadata<DirectorySecurity>(
			AccessControlConstants.AccessControl) ?? new DirectorySecurity();
	}

	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.GetAccessControl(DirectoryInfo, AccessControlSections)" />
	[SupportedOSPlatform("windows")]
	public static DirectorySecurity GetAccessControl(
		this IFileSystem.IDirectory directory,
		string path,
		AccessControlSections includeSections)
	{
		IFileSystem.IDirectoryInfo directoryInfo =
			directory.FileSystem.DirectoryInfo.New(path);
		IFileSystem.IFileSystemExtensionContainer extensionContainer =
			directoryInfo.ExtensionContainer;
		if (extensionContainer.HasWrappedInstance(out DirectoryInfo? di))
		{
			return di.GetAccessControl(includeSections);
		}

		return extensionContainer.RetrieveMetadata<DirectorySecurity>(
			AccessControlConstants.AccessControl) ?? new DirectorySecurity();
	}

	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.SetAccessControl(DirectoryInfo, DirectorySecurity)" />
	[SupportedOSPlatform("windows")]
	public static void SetAccessControl(this IFileSystem.IDirectory directory,
	                                    string path,
	                                    DirectorySecurity directorySecurity)
	{
		IFileSystem.IDirectoryInfo directoryInfo =
			directory.FileSystem.DirectoryInfo.New(path);
		IFileSystem.IFileSystemExtensionContainer extensionContainer =
			directoryInfo.ExtensionContainer;
		if (extensionContainer.HasWrappedInstance(out DirectoryInfo? di))
		{
			di.SetAccessControl(directorySecurity);
		}
		else
		{
			extensionContainer.StoreMetadata(AccessControlConstants.AccessControl,
				directorySecurity);
		}
	}
}