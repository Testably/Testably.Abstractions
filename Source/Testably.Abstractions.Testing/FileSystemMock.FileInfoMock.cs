using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using Testably.Abstractions.Testing.Internal;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
	/// <summary>
	///     A mocked file in the <see cref="InMemoryStorage" />.
	/// </summary>
	private sealed class FileInfoMock
		: FileSystemInfoMock, IFileSystem.IFileInfo
	{
		private FileInfoMock(IStorageLocation location,
		                     FileSystemMock fileSystem)
			: base(fileSystem, location)
		{
		}

		#region IFileInfo Members

		/// <inheritdoc cref="IFileSystem.IFileInfo.Directory" />
		public IFileSystem.IDirectoryInfo? Directory
			=> DirectoryInfoMock.New(Location.GetParent(),
				FileSystem);

		/// <inheritdoc cref="IFileSystem.IFileInfo.DirectoryName" />
		public string? DirectoryName
			=> Directory?.FullName;

		/// <inheritdoc cref="IFileSystem.IFileInfo.IsReadOnly" />
		public bool IsReadOnly
		{
			get => (Attributes & FileAttributes.ReadOnly) != 0;
			set
			{
				if (value)
				{
					Attributes |= FileAttributes.ReadOnly;
				}
				else
				{
					Attributes &= ~FileAttributes.ReadOnly;
				}
			}
		}

		/// <inheritdoc cref="IFileSystem.IFileInfo.Length" />
		public long Length
		{
			get
			{
				if (Container is NullContainer)
				{
					throw ExceptionFactory.FileNotFound(
						Execute.OnNetFramework(
							() => Location.FriendlyName,
							() => Location.FullPath));
				}

				return Container.GetBytes().Length;
			}
		}

		/// <inheritdoc cref="IFileSystem.IFileInfo.AppendText()" />
		public StreamWriter AppendText()
			=> new(Open(FileMode.Append, FileAccess.Write));

		/// <inheritdoc cref="IFileSystem.IFileInfo.CopyTo(string)" />
		public IFileSystem.IFileInfo CopyTo(string destFileName)
		{
			IStorageLocation location = FileSystem.Storage.Copy(
				                            Location,
				                            FileSystem.Storage.GetLocation(destFileName))
			                            ?? throw ExceptionFactory.FileNotFound(FullName);
			return FileSystem.FileInfo.New(location.FullPath);
		}

		/// <inheritdoc cref="IFileSystem.IFileInfo.CopyTo(string, bool)" />
		public IFileSystem.IFileInfo CopyTo(string destFileName, bool overwrite)
		{
			IStorageLocation location = FileSystem.Storage.Copy(
				                            Location,
				                            FileSystem.Storage.GetLocation(destFileName),
				                            overwrite)
			                            ?? throw ExceptionFactory.FileNotFound(FullName);
			return FileSystem.FileInfo.New(location.FullPath);
		}

		/// <inheritdoc cref="IFileSystem.IFileInfo.Create()" />
		public FileSystemStream Create()
			=> FileSystem.File.Create(FullName);

		/// <inheritdoc cref="IFileSystem.IFileInfo.CreateText()" />
		public StreamWriter CreateText()
			=> new(Create());

		/// <inheritdoc cref="IFileSystem.IFileInfo.Decrypt()" />
		[SupportedOSPlatform("windows")]
		public void Decrypt()
			=> Container.Decrypt();

		/// <inheritdoc cref="IFileSystem.IFileInfo.Encrypt()" />
		[SupportedOSPlatform("windows")]
		public void Encrypt()
			=> Container.Encrypt();

#if FEATURE_FILE_SYSTEM_ACL_EXTENSIONS
		/// <inheritdoc cref="IFileSystem.IFileInfo.GetAccessControl()" />
		[SupportedOSPlatform("windows")]
		public FileSecurity GetAccessControl()
			=> Container.AccessControl as FileSecurity ?? new FileSecurity();

		/// <inheritdoc cref="IFileSystem.IFileInfo.GetAccessControl(AccessControlSections)" />
		[SupportedOSPlatform("windows")]
		public FileSecurity GetAccessControl(AccessControlSections includeSections)
			=> GetAccessControl();
#endif

		/// <inheritdoc cref="IFileSystem.IFileInfo.MoveTo(string)" />
		public void MoveTo(string destFileName)
		{
			Location = FileSystem.Storage.Move(
				           Location,
				           FileSystem.Storage.GetLocation(destFileName))
			           ?? throw ExceptionFactory.FileNotFound(FullName);
		}

#if FEATURE_FILE_MOVETO_OVERWRITE
		/// <inheritdoc cref="IFileSystem.IFileInfo.MoveTo(string, bool)" />
		public void MoveTo(string destFileName, bool overwrite)
		{
			Location = FileSystem.Storage.Move(
				           Location,
				           FileSystem.Storage.GetLocation(destFileName),
				           overwrite)
			           ?? throw ExceptionFactory.FileNotFound(FullName);
		}
#endif

		/// <inheritdoc cref="IFileSystem.IFileInfo.Open(FileMode)" />
		public FileSystemStream Open(FileMode mode)
		{
			Execute.OnNetFrameworkIf(mode == FileMode.Append,
				() => throw ExceptionFactory.AppendAccessOnlyInWriteOnlyMode());

			return new FileStreamMock(
				FileSystem,
				FullName,
				mode,
				mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite,
				FileShare.None);
		}

		/// <inheritdoc cref="IFileSystem.IFileInfo.Open(FileMode, FileAccess)" />
		public FileSystemStream Open(FileMode mode, FileAccess access)
			=> new FileStreamMock(
				FileSystem,
				FullName,
				mode,
				access,
				FileShare.None);

		/// <inheritdoc cref="IFileSystem.IFileInfo.Open(FileMode, FileAccess, FileShare)" />
		public FileSystemStream Open(FileMode mode, FileAccess access, FileShare share)
			=> new FileStreamMock(
				FileSystem,
				FullName,
				mode,
				access,
				share);

#if FEATURE_FILESYSTEM_STREAM_OPTIONS
		/// <inheritdoc cref="IFileSystem.IFileInfo.Open(FileStreamOptions)" />
		public FileSystemStream Open(FileStreamOptions options)
			=> FileSystem.File.Open(FullName, options);
#endif

		/// <inheritdoc cref="IFileSystem.IFileInfo.OpenRead()" />
		public FileSystemStream OpenRead()
			=> new FileStreamMock(
				FileSystem,
				FullName,
				FileMode.Open,
				FileAccess.Read);

		/// <inheritdoc cref="IFileSystem.IFileInfo.OpenText()" />
		public StreamReader OpenText()
			=> new(OpenRead());

		/// <inheritdoc cref="IFileSystem.IFileInfo.OpenWrite()" />
		public FileSystemStream OpenWrite()
			=> new FileStreamMock(
				FileSystem,
				FullName,
				FileMode.OpenOrCreate,
				FileAccess.Write,
				FileShare.None);

		/// <inheritdoc cref="IFileSystem.IFileInfo.Replace(string, string?)" />
		public IFileSystem.IFileInfo Replace(string destinationFileName,
		                                     string? destinationBackupFileName)
		{
			IStorageLocation location = FileSystem.Storage.Replace(
				                            Location,
				                            FileSystem.Storage.GetLocation(
					                            destinationFileName),
				                            FileSystem.Storage.GetLocation(
					                            destinationBackupFileName))
			                            ?? throw ExceptionFactory.FileNotFound(FullName);
			return FileSystem.FileInfo.New(location.FullPath);
		}

		/// <inheritdoc cref="IFileSystem.IFileInfo.Replace(string, string?, bool)" />
		public IFileSystem.IFileInfo Replace(string destinationFileName,
		                                     string? destinationBackupFileName,
		                                     bool ignoreMetadataErrors)
		{
			IStorageLocation location = FileSystem.Storage.Replace(
				                            Location,
				                            FileSystem.Storage.GetLocation(
					                            destinationFileName),
				                            FileSystem.Storage.GetLocation(
					                            destinationBackupFileName),
				                            ignoreMetadataErrors)
			                            ?? throw ExceptionFactory.FileNotFound(FullName);
			return FileSystem.FileInfo.New(location.FullPath);
		}

#if FEATURE_FILE_SYSTEM_ACL_EXTENSIONS
		/// <inheritdoc cref="IFileSystem.IFileInfo.SetAccessControl(FileSecurity)" />
		[SupportedOSPlatform("windows")]
		public void SetAccessControl(FileSecurity fileSecurity)
			=> Container.AccessControl = fileSecurity;
#endif

		#endregion

		[return: NotNullIfNotNull("location")]
		internal static new FileInfoMock? New(IStorageLocation? location,
		                                      FileSystemMock fileSystem)
		{
			if (location == null)
			{
				return null;
			}

			return new FileInfoMock(location, fileSystem);
		}
	}
}