using System.IO;
using System.Runtime.Versioning;
using System.Security.AccessControl;

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
		IFileSystemExtensionContainer extensionContainer =
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
		this IDirectory directory, string path)
	{
		IDirectoryInfo directoryInfo =
			directory.FileSystem.DirectoryInfo.New(path);
		IFileSystemExtensionContainer extensionContainer =
			directoryInfo.ExtensionContainer;
		return extensionContainer.HasWrappedInstance(out DirectoryInfo? di)
			? di.GetAccessControl()
			: extensionContainer.RetrieveMetadata<DirectorySecurity>(
				AccessControlConstants.AccessControl) ?? new DirectorySecurity();
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
		IFileSystemExtensionContainer extensionContainer =
			directoryInfo.ExtensionContainer;
		return extensionContainer.HasWrappedInstance(out DirectoryInfo? di)
			? di.GetAccessControl(includeSections)
			: extensionContainer.RetrieveMetadata<DirectorySecurity>(
				AccessControlConstants.AccessControl) ?? new DirectorySecurity();
	}

	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.SetAccessControl(DirectoryInfo, DirectorySecurity)" />
	[SupportedOSPlatform("windows")]
	public static void SetAccessControl(this IDirectory directory,
	                                    string path,
	                                    DirectorySecurity directorySecurity)
	{
		IDirectoryInfo directoryInfo =
			directory.FileSystem.DirectoryInfo.New(path);
		IFileSystemExtensionContainer extensionContainer =
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