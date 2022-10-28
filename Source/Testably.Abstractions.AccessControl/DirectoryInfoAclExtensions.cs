using System.IO;
using System.Security.AccessControl;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions;

/// <summary>
///     ACL (access control list) extension methods for <see cref="IDirectoryInfo" />.
/// </summary>
public static class DirectoryInfoAclExtensions
{
	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.Create(DirectoryInfo, DirectorySecurity)" />
	[SupportedOSPlatform("windows")]
	public static void Create(this IDirectoryInfo directoryInfo,
	                          DirectorySecurity directorySecurity)
	{
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
		this IDirectoryInfo directoryInfo)
	{
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
		this IDirectoryInfo directoryInfo,
		AccessControlSections includeSections)
	{
		IFileSystemExtensionContainer extensionContainer =
			directoryInfo.ExtensionContainer;
		return extensionContainer.HasWrappedInstance(out DirectoryInfo? di)
			? di.GetAccessControl(includeSections)
			: extensionContainer.RetrieveMetadata<DirectorySecurity>(
				AccessControlConstants.AccessControl) ?? new DirectorySecurity();
	}

	/// <inheritdoc cref="System.IO.FileSystemAclExtensions.SetAccessControl(DirectoryInfo, DirectorySecurity)" />
	[SupportedOSPlatform("windows")]
	public static void SetAccessControl(this IDirectoryInfo directoryInfo,
	                                    DirectorySecurity directorySecurity)
	{
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