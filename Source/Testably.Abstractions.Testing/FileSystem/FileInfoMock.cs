using System.Diagnostics.CodeAnalysis;
using System.IO;
using Testably.Abstractions.FileSystem;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     A mocked file in the <see cref="InMemoryStorage" />.
/// </summary>
internal sealed class FileInfoMock
	: FileSystemInfoMock, IFileInfo
{
	private FileInfoMock(IStorageLocation location,
	                     MockFileSystem fileSystem)
		: base(fileSystem, location, FileSystemTypes.File)
	{
	}

	#region IFileInfo Members

	/// <inheritdoc cref="IFileInfo.Directory" />
	public IDirectoryInfo? Directory
		=> DirectoryInfoMock.New(Location.GetParent(),
			FileSystem);

	/// <inheritdoc cref="IFileInfo.DirectoryName" />
	public string? DirectoryName
		=> Directory?.FullName;

	/// <inheritdoc cref="IFileSystemInfo.Exists" />
	public override bool Exists
		=> base.Exists && FileSystemType == FileSystemTypes.File;

	/// <inheritdoc cref="IFileInfo.IsReadOnly" />
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

	/// <inheritdoc cref="IFileInfo.Length" />
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

	/// <inheritdoc cref="IFileInfo.AppendText()" />
	public StreamWriter AppendText()
		=> new(Open(FileMode.Append, FileAccess.Write));

	/// <inheritdoc cref="IFileInfo.CopyTo(string)" />
	public IFileInfo CopyTo(string destFileName)
	{
		IStorageLocation location = FileSystem.Storage.Copy(
			                            Location,
			                            FileSystem.Storage.GetLocation(destFileName
				                           .EnsureValidArgument(FileSystem,
					                            nameof(destFileName))))
		                            ?? throw ExceptionFactory.FileNotFound(FullName);
		return FileSystem.FileInfo.New(location.FullPath);
	}

	/// <inheritdoc cref="IFileInfo.CopyTo(string, bool)" />
	public IFileInfo CopyTo(string destFileName, bool overwrite)
	{
		IStorageLocation location = FileSystem.Storage.Copy(
			                            Location,
			                            FileSystem.Storage.GetLocation(destFileName
				                           .EnsureValidArgument(FileSystem,
					                            nameof(destFileName))),
			                            overwrite)
		                            ?? throw ExceptionFactory.FileNotFound(FullName);
		return FileSystem.FileInfo.New(location.FullPath);
	}

	/// <inheritdoc cref="IFileInfo.Create()" />
	public FileSystemStream Create()
		=> FileSystem.File.Create(FullName);

	/// <inheritdoc cref="IFileInfo.CreateText()" />
	public StreamWriter CreateText()
		=> new(Create());

	/// <inheritdoc cref="IFileInfo.Decrypt()" />
	[SupportedOSPlatform("windows")]
	public void Decrypt()
		=> Container.Decrypt();

	/// <inheritdoc cref="IFileInfo.Encrypt()" />
	[SupportedOSPlatform("windows")]
	public void Encrypt()
		=> Container.Encrypt();

	/// <inheritdoc cref="IFileInfo.MoveTo(string)" />
	public void MoveTo(string destFileName)
	{
		Location = FileSystem.Storage.Move(
			           Location,
			           FileSystem.Storage.GetLocation(destFileName
				          .EnsureValidArgument(FileSystem, nameof(destFileName))))
		           ?? throw ExceptionFactory.FileNotFound(FullName);
	}

#if FEATURE_FILE_MOVETO_OVERWRITE
	/// <inheritdoc cref="IFileInfo.MoveTo(string, bool)" />
	public void MoveTo(string destFileName, bool overwrite)
	{
		Location = FileSystem.Storage.Move(
			           Location,
			           FileSystem.Storage.GetLocation(destFileName
				          .EnsureValidArgument(FileSystem, nameof(destFileName))),
			           overwrite)
		           ?? throw ExceptionFactory.FileNotFound(FullName);
	}
#endif

	/// <inheritdoc cref="IFileInfo.Open(FileMode)" />
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

	/// <inheritdoc cref="IFileInfo.Open(FileMode, FileAccess)" />
	public FileSystemStream Open(FileMode mode, FileAccess access)
		=> new FileStreamMock(
			FileSystem,
			FullName,
			mode,
			access,
			FileShare.None);

	/// <inheritdoc cref="IFileInfo.Open(FileMode, FileAccess, FileShare)" />
	public FileSystemStream Open(FileMode mode, FileAccess access, FileShare share)
		=> new FileStreamMock(
			FileSystem,
			FullName,
			mode,
			access,
			share);

#if FEATURE_FILESYSTEM_STREAM_OPTIONS
	/// <inheritdoc cref="IFileInfo.Open(FileStreamOptions)" />
	public FileSystemStream Open(FileStreamOptions options)
		=> FileSystem.File.Open(FullName, options);
#endif

	/// <inheritdoc cref="IFileInfo.OpenRead()" />
	public FileSystemStream OpenRead()
		=> new FileStreamMock(
			FileSystem,
			FullName,
			FileMode.Open,
			FileAccess.Read);

	/// <inheritdoc cref="IFileInfo.OpenText()" />
	public StreamReader OpenText()
		=> new(OpenRead());

	/// <inheritdoc cref="IFileInfo.OpenWrite()" />
	public FileSystemStream OpenWrite()
		=> new FileStreamMock(
			FileSystem,
			FullName,
			FileMode.OpenOrCreate,
			FileAccess.Write,
			FileShare.None);

	/// <inheritdoc cref="IFileInfo.Replace(string, string?)" />
	public IFileInfo Replace(string destinationFileName,
	                         string? destinationBackupFileName)
	{
		IStorageLocation location = FileSystem.Storage.Replace(
			                            Location,
			                            FileSystem.Storage.GetLocation(
				                            destinationFileName
					                           .EnsureValidFormat(FileSystem,
						                            nameof(destinationFileName))),
			                            FileSystem.Storage.GetLocation(
				                            destinationBackupFileName))
		                            ?? throw ExceptionFactory.FileNotFound(FullName);
		return FileSystem.FileInfo.New(location.FullPath);
	}

	/// <inheritdoc cref="IFileInfo.Replace(string, string?, bool)" />
	public IFileInfo Replace(string destinationFileName,
	                         string? destinationBackupFileName,
	                         bool ignoreMetadataErrors)
	{
		IStorageLocation location = FileSystem.Storage.Replace(
			                            Location,
			                            FileSystem.Storage.GetLocation(
				                            destinationFileName
					                           .EnsureValidFormat(FileSystem,
						                            nameof(destinationFileName))),
			                            FileSystem.Storage.GetLocation(
				                            destinationBackupFileName),
			                            ignoreMetadataErrors)
		                            ?? throw ExceptionFactory.FileNotFound(FullName);
		return FileSystem.FileInfo.New(location.FullPath);
	}

	#endregion

	[return: NotNullIfNotNull("location")]
	internal static new FileInfoMock? New(IStorageLocation? location,
	                                      MockFileSystem fileSystem)
	{
		if (location == null)
		{
			return null;
		}

		return new FileInfoMock(location, fileSystem);
	}
}