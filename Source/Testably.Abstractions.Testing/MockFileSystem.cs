﻿using Microsoft.Win32.SafeHandles;
using System;
using Testably.Abstractions.FileSystem;
using Testably.Abstractions.Testing.FileSystem;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing;

/// <summary>
///     A test helper for simulating the file system. Implements <see cref="IFileSystem" />.
/// </summary>
public sealed class MockFileSystem : IFileSystem
{
	/// <summary>
	///     Intercept events in the <see cref="MockFileSystem" /> before they occur.
	/// </summary>
	public IInterceptionHandler Intercept => ChangeHandler;

	/// <summary>
	///     Get notified of events in the <see cref="MockFileSystem" /> after they occurred.
	/// </summary>
	public INotificationHandler Notify => ChangeHandler;

	/// <summary>
	///     The used random system.
	/// </summary>
	public IRandomSystem RandomSystem { get; }

	/// <summary>
	///     The used time system.
	/// </summary>
	public ITimeSystem TimeSystem { get; }

	/// <summary>
	///     The change handler used to notify about events occurring in the <see cref="MockFileSystem" />.
	/// </summary>
	internal ChangeHandler ChangeHandler { get; }

	/// <summary>
	///     The underlying storage of directories and files.
	/// </summary>
	internal IStorage Storage => _storage;

	private readonly DirectoryMock _directoryMock;
	private readonly FileMock _fileMock;
	private readonly PathMock _pathMock;
	private readonly InMemoryStorage _storage;

	internal Func<SafeFileHandle, SafeFileHandleMock> SafeFileHandleMapper { get; private set; }

	/// <summary>
	///     Initializes the <see cref="MockFileSystem" />.
	/// </summary>
	public MockFileSystem()
	{
		RandomSystem = new MockRandomSystem();
		TimeSystem = new MockTimeSystem(TimeProvider.Now());
		_pathMock = new PathMock(this);
		_storage = new InMemoryStorage(this);
		ChangeHandler = new ChangeHandler(this);
		_directoryMock = new DirectoryMock(this);
		_fileMock = new FileMock(this);
		DirectoryInfo = new DirectoryInfoFactoryMock(this);
		DriveInfo = new DriveInfoFactoryMock(this);
		FileInfo = new FileInfoFactoryMock(this);
		FileStream = new FileStreamFactoryMock(this);
		FileSystemWatcher = new FileSystemWatcherFactoryMock(this);
		SafeFileHandleMapper = _ => throw ExceptionFactory.NotSupportedSafeFileHandle();
	}

	#region IFileSystem Members

	/// <inheritdoc cref="IFileSystem.Directory" />
	public IDirectory Directory
		=> _directoryMock;

	/// <inheritdoc cref="IFileSystem.DirectoryInfo" />
	public IDirectoryInfoFactory DirectoryInfo { get; }

	/// <inheritdoc cref="IFileSystem.DriveInfo" />
	public IDriveInfoFactory DriveInfo { get; }

	/// <inheritdoc cref="IFileSystem.File" />
	public IFile File
		=> _fileMock;

	/// <inheritdoc cref="IFileSystem.FileInfo" />
	public IFileInfoFactory FileInfo { get; }

	/// <inheritdoc cref="IFileSystem.FileStream" />
	public IFileStreamFactory FileStream { get; }

	/// <inheritdoc cref="IFileSystem.FileSystemWatcher" />
	public IFileSystemWatcherFactory FileSystemWatcher { get; }

	/// <inheritdoc cref="IFileSystem.Path" />
	public IPath Path
		=> _pathMock;

	#endregion

	/// <summary>
	///     Implements a custom access control (ACL) mechanism.
	///     <para />
	///     The callback enables granting or rejecting access to arbitrary containers and receives
	///     the following input parameters:<br />
	///     - The full path of the file or directory as first parameter<br />
	///     - The <see cref="IFileSystemExtensionContainer" /> as second parameter
	/// </summary>
	/// <param name="grantRequestCallback">
	///     A callback that receives the full path as first parameter and the
	///     <see cref="IFileSystemExtensionContainer" /> as second parameter and returns <see langword="true" /> if
	///     the request was granted and <see langword="false" /> if the request was rejected.
	/// </param>
	public MockFileSystem WithAccessControl(
		Func<string, IFileSystemExtensionContainer, bool>?
			grantRequestCallback)
	{
		_storage.WithAccessControl(grantRequestCallback);
		return this;
	}

	/// <summary>
	///     Changes the parameters of the specified <paramref name="drive" />.
	///     <para />
	///     If the <paramref name="drive" /> does not exist, it will be created/mounted.
	/// </summary>
	public MockFileSystem WithDrive(string? drive,
	                                Action<IStorageDrive>? driveCallback = null)
	{
		IStorageDrive driveInfoMock =
			drive == null
				? Storage.MainDrive
				: Storage.GetOrAddDrive(drive);
		driveCallback?.Invoke(driveInfoMock);
		return this;
	}

	/// <summary>
	///     Registers a callback to map a <see cref="SafeFileHandle"/>
	///     to a <see cref="SafeFileHandleMock"/>.
	/// </summary>
	public MockFileSystem MapSafeFileHandle(Func<SafeFileHandle,SafeFileHandleMock> safeFileHandleMapper)
	{
		SafeFileHandleMapper = safeFileHandleMapper;
		return this;
	}
}