using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Versioning;

namespace Testably.Abstractions;

public sealed partial class FileSystem
{
	private sealed class FileInfoWrapper : FileSystemInfoWrapper, IFileInfo
	{
		private readonly IFileSystem _fileSystem;
		private readonly FileInfo _instance;

		private FileInfoWrapper(FileInfo instance, IFileSystem fileSystem)
			: base(instance, fileSystem)
		{
			_instance = instance;
			_fileSystem = fileSystem;
		}

		#region IFileInfo Members

		/// <inheritdoc cref="IFileInfo.Directory" />
		public IDirectoryInfo? Directory
			=> DirectoryInfoWrapper.FromDirectoryInfo(_instance.Directory, _fileSystem);

		/// <inheritdoc cref="IFileInfo.DirectoryName" />
		public string? DirectoryName
			=> _instance.DirectoryName;

		/// <inheritdoc cref="IFileInfo.IsReadOnly" />
		public bool IsReadOnly
		{
			get => _instance.IsReadOnly;
			set => _instance.IsReadOnly = value;
		}

		/// <inheritdoc cref="IFileInfo.Length" />
		public long Length
			=> _instance.Length;

		/// <inheritdoc cref="IFileInfo.AppendText()" />
		public StreamWriter AppendText()
			=> _instance.AppendText();

		/// <inheritdoc cref="IFileInfo.CopyTo(string)" />
		public IFileInfo CopyTo(string destFileName)
			=> FromFileInfo(_instance.CopyTo(destFileName), _fileSystem);

		/// <inheritdoc cref="IFileInfo.CopyTo(string, bool)" />
		public IFileInfo CopyTo(string destFileName, bool overwrite)
			=> FromFileInfo(_instance.CopyTo(destFileName, overwrite), _fileSystem);

		/// <inheritdoc cref="IFileInfo.Create()" />
		public FileSystemStream Create()
			=> new FileStreamWrapper(_instance.Create());

		/// <inheritdoc cref="IFileInfo.CreateText()" />
		public StreamWriter CreateText()
			=> _instance.CreateText();

		/// <inheritdoc cref="IFileInfo.Decrypt()" />
		[SupportedOSPlatform("windows")]
		public void Decrypt()
			=> _instance.Decrypt();

		/// <inheritdoc cref="IFileInfo.Encrypt()" />
		[SupportedOSPlatform("windows")]
		public void Encrypt()
			=> _instance.Encrypt();

		/// <inheritdoc cref="IFileInfo.MoveTo(string)" />
		public void MoveTo(string destFileName)
			=> _instance.MoveTo(destFileName);

#if FEATURE_FILE_MOVETO_OVERWRITE
		/// <inheritdoc cref="IFileInfo.MoveTo(string, bool)" />
		public void MoveTo(string destFileName, bool overwrite)
			=> _instance.MoveTo(destFileName, overwrite);
#endif

		/// <inheritdoc cref="IFileInfo.Open(FileMode)" />
		public FileSystemStream Open(FileMode mode)
			=> new FileStreamWrapper(_instance.Open(mode));

		/// <inheritdoc cref="IFileInfo.Open(FileMode, FileAccess)" />
		public FileSystemStream Open(FileMode mode, FileAccess access)
			=> new FileStreamWrapper(_instance.Open(mode, access));

		/// <inheritdoc cref="IFileInfo.Open(FileMode, FileAccess, FileShare)" />
		public FileSystemStream Open(FileMode mode, FileAccess access, FileShare share)
			=> new FileStreamWrapper(_instance.Open(mode, access, share));

#if FEATURE_FILESYSTEM_STREAM_OPTIONS
		/// <inheritdoc cref="IFileInfo.Open(FileStreamOptions)" />
		public FileSystemStream Open(FileStreamOptions options)
			=> new FileStreamWrapper(_instance.Open(options));
#endif

		/// <inheritdoc cref="IFileInfo.OpenRead()" />
		public FileSystemStream OpenRead()
			=> new FileStreamWrapper(_instance.OpenRead());

		/// <inheritdoc cref="IFileInfo.OpenText()" />
		public StreamReader OpenText()
			=> _instance.OpenText();

		/// <inheritdoc cref="IFileInfo.OpenWrite()" />
		public FileSystemStream OpenWrite()
			=> new FileStreamWrapper(_instance.OpenWrite());

		/// <inheritdoc cref="IFileInfo.Replace(string, string?)" />
		public IFileInfo Replace(string destinationFileName,
		                                     string? destinationBackupFileName)
			=> FromFileInfo(
				_instance.Replace(destinationFileName, destinationBackupFileName),
				_fileSystem);

		/// <inheritdoc cref="IFileInfo.Replace(string, string?, bool)" />
		public IFileInfo Replace(string destinationFileName,
		                                     string? destinationBackupFileName,
		                                     bool ignoreMetadataErrors)
			=> FromFileInfo(
				_instance.Replace(destinationFileName, destinationBackupFileName,
					ignoreMetadataErrors),
				_fileSystem);

		#endregion

		[return: NotNullIfNotNull("instance")]
		internal static FileInfoWrapper? FromFileInfo(FileInfo? instance,
		                                              IFileSystem fileSystem)
		{
			if (instance == null)
			{
				return null;
			}

			return new FileInfoWrapper(instance, fileSystem);
		}
	}
}