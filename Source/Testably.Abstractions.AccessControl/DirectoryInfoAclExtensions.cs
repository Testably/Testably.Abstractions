using System.IO;
using System.Runtime.Versioning;
using System.Security.AccessControl;

namespace Testably.Abstractions;

/// <summary>
///     ACL (access control list) extension methods for <see cref="IFileSystem.IDirectoryInfo" />.
/// </summary>
public static class DirectoryInfoAclExtensions
{
	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.Create(DirectoryInfo, DirectorySecurity)" />
	[SupportedOSPlatform("windows")]
	public static void Create(this IFileSystem.IDirectoryInfo directoryInfo,
	                          DirectorySecurity directorySecurity)
	{
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
		this IFileSystem.IDirectoryInfo directoryInfo)
	{
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
		this IFileSystem.IDirectoryInfo directoryInfo,
		AccessControlSections includeSections)
	{
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
	public static void SetAccessControl(this IFileSystem.IDirectoryInfo directoryInfo,
	                                    DirectorySecurity directorySecurity)
	{
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