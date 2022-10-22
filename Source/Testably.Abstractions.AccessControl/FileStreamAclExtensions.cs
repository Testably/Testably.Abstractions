using System.IO;
using System.Runtime.Versioning;
using System.Security.AccessControl;

namespace Testably.Abstractions;

public static class FileStreamAclExtensions
{
	/// <inheritdoc cref="FileSystemAclExtensions.GetAccessControl(FileStream)" />
	[SupportedOSPlatform("windows")]
	public static FileSecurity GetAccessControl(this FileSystemStream fileStream)
	{
		IFileSystem.IFileSystemExtensionContainer extensionContainer =
			fileStream.ExtensionContainer;
		if (extensionContainer.HasWrappedInstance(out FileStream? fs))
		{
			return fs.GetAccessControl();
		}

		return extensionContainer.RetrieveMetadata<FileSecurity>(
			nameof(FileSystemAclExtensions)) ?? new FileSecurity();
	}

	/// <inheritdoc cref="FileSystemAclExtensions.SetAccessControl(FileStream, FileSecurity)" />
	[SupportedOSPlatform("windows")]
	public static void SetAccessControl(this FileSystemStream fileStream,
	                                    FileSecurity fileSecurity)
	{
		IFileSystem.IFileSystemExtensionContainer extensionContainer =
			fileStream.ExtensionContainer;
		if (extensionContainer.HasWrappedInstance(out FileStream? fs))
		{
			fs.SetAccessControl(fileSecurity);
		}
		else
		{
			extensionContainer.StoreMetadata(nameof(FileSystemAclExtensions),
				fileSecurity);
		}
	}
}