using System.IO;
using System.Runtime.Versioning;
using System.Security.AccessControl;

namespace Testably.Abstractions;

public sealed partial class FileSystem
{
	private sealed class FileStreamWrapper : FileSystemStream
	{
		private readonly FileStream _fileStream;

		public FileStreamWrapper(FileStream fileStream)
			: base(fileStream, fileStream.Name, fileStream.IsAsync)

		{
			_fileStream = fileStream;
		}

#if FEATURE_FILE_SYSTEM_ACL_EXTENSIONS
		/// <inheritdoc cref="FileSystemStream.GetAccessControl()" />
		[SupportedOSPlatform("windows")]
		public override FileSecurity GetAccessControl()
			=> _fileStream.GetAccessControl();

		/// <inheritdoc cref="FileSystemStream.SetAccessControl(FileSecurity)" />
		[SupportedOSPlatform("windows")]
		public override void SetAccessControl(FileSecurity fileSecurity)
			=> _fileStream.SetAccessControl(fileSecurity);
#endif
	}
}