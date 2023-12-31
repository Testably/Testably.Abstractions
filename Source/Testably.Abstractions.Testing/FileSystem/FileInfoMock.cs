using System.Diagnostics.CodeAnalysis;
using System.IO;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     A mocked file in the <see cref="InMemoryStorage" />.
/// </summary>
internal sealed class FileInfoMock
	: FileSystemInfoMock, IFileInfo
{
	private readonly MockFileSystem _fileSystem;

	private FileInfoMock(IStorageLocation location,
		MockFileSystem fileSystem)
		: base(fileSystem, location, FileSystemTypes.File)
	{
		_fileSystem = fileSystem;
	}

	#region IFileInfo Members

	/// <inheritdoc cref="IFileInfo.Directory" />
	public IDirectoryInfo? Directory
		=> DirectoryInfoMock.New(Location.GetParent(),
			_fileSystem);

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
			if (Container is NullContainer ||
			    Container.Type != FileSystemTypes.File)
			{
				throw ExceptionFactory.FileNotFound(
					Execute.OnNetFramework(
						() => Location.FriendlyName,
						() => Location.FullPath));
			}

			return Container.GetBytes().Length;
		}
	}

	/// <inheritdoc cref="IFileInfo.Name" />
	public override string Name
	{
		get
		{
			if (Location.FullPath.EndsWith(FileSystem.Path.DirectorySeparatorChar))
			{
				return string.Empty;
			}

			return base.Name;
		}
	}

	/// <inheritdoc cref="IFileInfo.AppendText()" />
	public StreamWriter AppendText()
		=> new(Open(FileMode.Append, FileAccess.Write));

	/// <inheritdoc cref="IFileInfo.CopyTo(string)" />
	public IFileInfo CopyTo(string destFileName)
	{
		destFileName.EnsureValidArgument(_fileSystem, nameof(destFileName));
		IStorageLocation destinationLocation = _fileSystem.Storage.GetLocation(destFileName);
		Location.ThrowExceptionIfNotFound(_fileSystem);
		IStorageLocation location = _fileSystem.Storage
			                            .Copy(Location, destinationLocation)
		                            ?? throw ExceptionFactory.FileNotFound(FullName);
		return _fileSystem.FileInfo.New(location.FullPath);
	}

	/// <inheritdoc cref="IFileInfo.CopyTo(string, bool)" />
	public IFileInfo CopyTo(string destFileName, bool overwrite)
	{
		destFileName.EnsureValidArgument(_fileSystem, nameof(destFileName));
		IStorageLocation location = _fileSystem.Storage.Copy(
			                            Location,
			                            _fileSystem.Storage.GetLocation(destFileName),
			                            overwrite)
		                            ?? throw ExceptionFactory.FileNotFound(FullName);
		return _fileSystem.FileInfo.New(location.FullPath);
	}

	/// <inheritdoc cref="IFileInfo.Create()" />
	public FileSystemStream Create()
	{
		Execute.NotOnNetFramework(Refresh);
		return _fileSystem.File.Create(FullName);
	}

	/// <inheritdoc cref="IFileInfo.CreateText()" />
	public StreamWriter CreateText()
	{
		StreamWriter streamWriter = new(_fileSystem.File.Create(FullName));
#if NET8_0_OR_GREATER
		Refresh();
#endif
		return streamWriter;
	}

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
		Location = _fileSystem.Storage.Move(
			           Location,
			           _fileSystem.Storage.GetLocation(destFileName
				           .EnsureValidArgument(_fileSystem, nameof(destFileName))))
		           ?? throw ExceptionFactory.FileNotFound(FullName);
	}

#if FEATURE_FILE_MOVETO_OVERWRITE
	/// <inheritdoc cref="IFileInfo.MoveTo(string, bool)" />
	public void MoveTo(string destFileName, bool overwrite)
	{
		Location = _fileSystem.Storage.Move(
			           Location,
			           _fileSystem.Storage.GetLocation(destFileName
				           .EnsureValidArgument(_fileSystem, nameof(destFileName))),
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
			_fileSystem,
			FullName,
			mode,
			mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite,
			FileShare.None);
	}

	/// <inheritdoc cref="IFileInfo.Open(FileMode, FileAccess)" />
	public FileSystemStream Open(FileMode mode, FileAccess access)
		=> new FileStreamMock(
			_fileSystem,
			FullName,
			mode,
			access,
			FileShare.None);

	/// <inheritdoc cref="IFileInfo.Open(FileMode, FileAccess, FileShare)" />
	public FileSystemStream Open(FileMode mode, FileAccess access, FileShare share)
		=> new FileStreamMock(
			_fileSystem,
			FullName,
			mode,
			access,
			share);

#if FEATURE_FILESYSTEM_STREAM_OPTIONS
	/// <inheritdoc cref="IFileInfo.Open(FileStreamOptions)" />
	public FileSystemStream Open(FileStreamOptions options)
		=> _fileSystem.File.Open(FullName, options);
#endif

	/// <inheritdoc cref="IFileInfo.OpenRead()" />
	public FileSystemStream OpenRead()
		=> new FileStreamMock(
			_fileSystem,
			FullName,
			FileMode.Open,
			FileAccess.Read);

	/// <inheritdoc cref="IFileInfo.OpenText()" />
	public StreamReader OpenText()
		=> new(OpenRead());

	/// <inheritdoc cref="IFileInfo.OpenWrite()" />
	public FileSystemStream OpenWrite()
		=> new FileStreamMock(
			_fileSystem,
			FullName,
			FileMode.OpenOrCreate,
			FileAccess.Write,
			FileShare.None);

	/// <inheritdoc cref="IFileInfo.Replace(string, string?)" />
	public IFileInfo Replace(string destinationFileName,
		string? destinationBackupFileName)
	{
		IStorageLocation location =
			_fileSystem
				.Storage
				.Replace(
					Location.ThrowIfNotFound(_fileSystem,
						() => { },
						() =>
						{
							if (Execute.IsWindows)
							{
								throw ExceptionFactory.FileNotFound(FullName);
							}

							throw ExceptionFactory.DirectoryNotFound(FullName);
						}),
					_fileSystem.Storage
						.GetLocation(destinationFileName
							.EnsureValidFormat(_fileSystem, nameof(destinationFileName)))
						.ThrowIfNotFound(_fileSystem,
							() => { },
							() =>
							{
								if (Execute.IsWindows)
								{
									throw ExceptionFactory.DirectoryNotFound(FullName);
								}

								throw ExceptionFactory.FileNotFound(FullName);
							}),
					_fileSystem.Storage
						.GetLocation(destinationBackupFileName)
						.ThrowIfNotFound(_fileSystem,
							() => { },
							() =>
							{
								if (Execute.IsWindows)
								{
									throw ExceptionFactory.FileNotFound(FullName);
								}

								throw ExceptionFactory.DirectoryNotFound(FullName);
							}),
					!Execute.IsWindows)
			?? throw ExceptionFactory.FileNotFound(FullName);
		return _fileSystem.FileInfo.New(location.FullPath);
	}

	/// <inheritdoc cref="IFileInfo.Replace(string, string?, bool)" />
	public IFileInfo Replace(string destinationFileName,
		string? destinationBackupFileName,
		bool ignoreMetadataErrors)
	{
		IStorageLocation location = _fileSystem.Storage.Replace(
			                            Location,
			                            _fileSystem.Storage.GetLocation(
				                            destinationFileName
					                            .EnsureValidFormat(_fileSystem,
						                            nameof(destinationFileName))),
			                            _fileSystem.Storage.GetLocation(
				                            destinationBackupFileName),
			                            ignoreMetadataErrors)
		                            ?? throw ExceptionFactory.FileNotFound(FullName);
		return _fileSystem.FileInfo.New(location.FullPath);
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
