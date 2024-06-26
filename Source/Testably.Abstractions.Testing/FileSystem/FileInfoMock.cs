﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Statistics;
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
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileInfo.RegisterPathProperty(Location.FullPath,
					nameof(Directory), PropertyAccess.Get);

			return DirectoryInfoMock.New(Location.GetParent(),
				_fileSystem);
		}
	}

	/// <inheritdoc cref="IFileInfo.DirectoryName" />
	public string? DirectoryName
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileInfo.RegisterPathProperty(Location.FullPath,
					nameof(DirectoryName), PropertyAccess.Get);

			return Directory?.FullName;
		}
	}

	/// <inheritdoc cref="IFileSystemInfo.Exists" />
	public override bool Exists
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileInfo.RegisterPathProperty(Location.FullPath,
					nameof(Exists), PropertyAccess.Get);

			return base.Exists && FileSystemType == FileSystemTypes.File;
		}
	}

	/// <inheritdoc cref="IFileInfo.IsReadOnly" />
	public bool IsReadOnly
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileInfo.RegisterPathProperty(Location.FullPath,
					nameof(IsReadOnly), PropertyAccess.Get);

			return (Attributes & FileAttributes.ReadOnly) != 0;
		}
		set
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileInfo.RegisterPathProperty(Location.FullPath,
					nameof(IsReadOnly), PropertyAccess.Set);

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
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileInfo.RegisterPathProperty(Location.FullPath,
					nameof(Length), PropertyAccess.Get);

			if (Container is NullContainer ||
			    Container.Type != FileSystemTypes.File)
			{
				throw ExceptionFactory.FileNotFound(
					_fileSystem.Execute.IsNetFramework
						? Location.FriendlyName
						: Location.FullPath);
			}

			return Container.GetBytes().Length;
		}
	}

	/// <inheritdoc cref="IFileSystemInfo.Name" />
	public override string Name
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.FileInfo.RegisterPathProperty(Location.FullPath,
					nameof(Name), PropertyAccess.Get);

			if (Location.FullPath.EndsWith(_fileSystem.Execute.Path.DirectorySeparatorChar))
			{
				return string.Empty;
			}

			return base.Name;
		}
	}

	/// <inheritdoc cref="IFileInfo.AppendText()" />
	public StreamWriter AppendText()
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.FileInfo.RegisterPathMethod(Location.FullPath, nameof(AppendText));

		return new StreamWriter(Open(FileMode.Append, FileAccess.Write));
	}

	/// <inheritdoc cref="IFileInfo.CopyTo(string)" />
	public IFileInfo CopyTo(string destFileName)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.FileInfo.RegisterPathMethod(Location.FullPath, nameof(CopyTo),
				destFileName);

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
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.FileInfo.RegisterPathMethod(Location.FullPath, nameof(CopyTo),
				destFileName, overwrite);

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
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.FileInfo.RegisterPathMethod(Location.FullPath, nameof(Create));

		if (!_fileSystem.Execute.IsNetFramework)
		{
			Refresh();
		}

		return _fileSystem.File.Create(FullName);
	}

	/// <inheritdoc cref="IFileInfo.CreateText()" />
	public StreamWriter CreateText()
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.FileInfo.RegisterPathMethod(Location.FullPath, nameof(CreateText));

		StreamWriter streamWriter = new(_fileSystem.File.Create(FullName));
#if NET8_0_OR_GREATER
		Refresh();
#endif
		return streamWriter;
	}

	/// <inheritdoc cref="IFileInfo.Decrypt()" />
	[SupportedOSPlatform("windows")]
	public void Decrypt()
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.FileInfo.RegisterPathMethod(Location.FullPath, nameof(Decrypt));

		Container.Decrypt();
	}

	/// <inheritdoc cref="IFileInfo.Encrypt()" />
	[SupportedOSPlatform("windows")]
	public void Encrypt()
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.FileInfo.RegisterPathMethod(Location.FullPath, nameof(Encrypt));

		Container.Encrypt();
	}

	/// <inheritdoc cref="IFileInfo.MoveTo(string)" />
	public void MoveTo(string destFileName)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.FileInfo.RegisterPathMethod(Location.FullPath, nameof(MoveTo),
				destFileName);

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
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.FileInfo.RegisterPathMethod(Location.FullPath, nameof(MoveTo),
				destFileName, overwrite);

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
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.FileInfo.RegisterPathMethod(Location.FullPath, nameof(Open),
				mode);

		if (_fileSystem.Execute.IsNetFramework && mode == FileMode.Append)
		{
			throw ExceptionFactory.AppendAccessOnlyInWriteOnlyMode();
		}

		return new FileStreamMock(
			_fileSystem,
			FullName,
			mode,
			mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite,
			FileShare.None);
	}

	/// <inheritdoc cref="IFileInfo.Open(FileMode, FileAccess)" />
	public FileSystemStream Open(FileMode mode, FileAccess access)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.FileInfo.RegisterPathMethod(Location.FullPath, nameof(Open),
				mode, access);

		return new FileStreamMock(
			_fileSystem,
			FullName,
			mode,
			access,
			FileShare.None);
	}

	/// <inheritdoc cref="IFileInfo.Open(FileMode, FileAccess, FileShare)" />
	public FileSystemStream Open(FileMode mode, FileAccess access, FileShare share)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.FileInfo.RegisterPathMethod(Location.FullPath, nameof(Open),
				mode, access, share);

		return new FileStreamMock(
			_fileSystem,
			FullName,
			mode,
			access,
			share);
	}

#if FEATURE_FILESYSTEM_STREAM_OPTIONS
	/// <inheritdoc cref="IFileInfo.Open(FileStreamOptions)" />
	public FileSystemStream Open(FileStreamOptions options)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.FileInfo.RegisterPathMethod(Location.FullPath, nameof(Open),
				options);

		return _fileSystem.File.Open(FullName, options);
	}
#endif

	/// <inheritdoc cref="IFileInfo.OpenRead()" />
	public FileSystemStream OpenRead()
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.FileInfo.RegisterPathMethod(Location.FullPath, nameof(OpenRead));

		return new FileStreamMock(
			_fileSystem,
			FullName,
			FileMode.Open,
			FileAccess.Read);
	}

	/// <inheritdoc cref="IFileInfo.OpenText()" />
	public StreamReader OpenText()
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.FileInfo.RegisterPathMethod(Location.FullPath, nameof(OpenText));

		return new StreamReader(OpenRead());
	}

	/// <inheritdoc cref="IFileInfo.OpenWrite()" />
	public FileSystemStream OpenWrite()
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.FileInfo.RegisterPathMethod(Location.FullPath, nameof(OpenWrite));

		return new FileStreamMock(
			_fileSystem,
			FullName,
			FileMode.OpenOrCreate,
			FileAccess.Write,
			FileShare.None);
	}

	/// <inheritdoc cref="IFileInfo.Replace(string, string?)" />
	public IFileInfo Replace(string destinationFileName,
		string? destinationBackupFileName)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.FileInfo.RegisterPathMethod(Location.FullPath, nameof(Replace),
				destinationFileName, destinationBackupFileName);

		IStorageLocation location =
			_fileSystem
				.Storage
				.Replace(
					Location.ThrowIfNotFound(_fileSystem,
						() => { },
						() =>
						{
							if (_fileSystem.Execute.IsWindows)
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
								if (_fileSystem.Execute.IsWindows)
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
								if (_fileSystem.Execute.IsWindows)
								{
									throw ExceptionFactory.FileNotFound(FullName);
								}

								throw ExceptionFactory.DirectoryNotFound(FullName);
							}),
					!_fileSystem.Execute.IsWindows)
			?? throw ExceptionFactory.FileNotFound(FullName);
		return _fileSystem.FileInfo.New(location.FullPath);
	}

	/// <inheritdoc cref="IFileInfo.Replace(string, string?, bool)" />
	public IFileInfo Replace(string destinationFileName,
		string? destinationBackupFileName,
		bool ignoreMetadataErrors)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.FileInfo.RegisterPathMethod(Location.FullPath, nameof(Replace),
				destinationFileName, destinationBackupFileName, ignoreMetadataErrors);

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

	protected override IDisposable RegisterPathMethod(string name)
		=> _fileSystem.StatisticsRegistration.FileInfo.RegisterPathMethod(Location.FullPath, name);

	protected override IDisposable RegisterPathMethod<T1>(string name, T1 parameter1)
		=> _fileSystem.StatisticsRegistration.FileInfo.RegisterPathMethod(Location.FullPath, name,
			parameter1);

	protected override IDisposable RegisterPathProperty(string name, PropertyAccess access)
		=> _fileSystem.StatisticsRegistration.FileInfo.RegisterPathProperty(Location.FullPath, name,
			access);
}
