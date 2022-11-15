using System.IO;
using System.Security.AccessControl;

namespace Testably.Abstractions;

/// <summary>
///     ACL (access control list) extension methods for <see cref="FileSystemStream" />.
/// </summary>
public static class FileStreamAclExtensions
{
	/// <inheritdoc cref="FileSystemAclExtensions.GetAccessControl(FileStream)" />
	[SupportedOSPlatform("windows")]
	public static FileSecurity GetAccessControl(this FileSystemStream fileStream)
	{
		IFileSystemExtensibility extensibility =
			fileStream.Extensibility;
		return extensibility.TryGetWrappedInstance(out FileStream? fs)
			? fs.GetAccessControl()
			: extensibility.RetrieveMetadata<FileSecurity>(
				AccessControlHelpers.AccessControl) ?? new FileSecurity();
	}

	/// <inheritdoc cref="FileSystemAclExtensions.SetAccessControl(FileStream, FileSecurity)" />
	[SupportedOSPlatform("windows")]
	public static void SetAccessControl(this FileSystemStream fileStream,
		FileSecurity fileSecurity)
	{
		IFileSystemExtensibility extensibility =
			fileStream.Extensibility;
		if (extensibility.TryGetWrappedInstance(out FileStream? fs))
		{
			fs.SetAccessControl(fileSecurity);
		}
		else
		{
			extensibility.StoreMetadata(AccessControlHelpers.AccessControl,
				fileSecurity);
		}
	}
}
