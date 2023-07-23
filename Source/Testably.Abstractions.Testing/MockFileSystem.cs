using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using Testably.Abstractions.Testing.FileSystem;
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

	/// <summary>
	///     The registered containers in the in-Memory <see cref="Storage" />.
	/// </summary>
	internal IReadOnlyList<IStorageContainer> Containers
		=> _storage.Containers
			.OrderBy(x => x.Key.FullPath)
			.Select(x => x.Value)
			.ToList();

	private readonly DirectoryMock _directoryMock;
	private readonly FileMock _fileMock;
	private readonly PathMock _pathMock;
	private readonly InMemoryStorage _storage;

	internal IAccessControlStrategy AccessControlStrategy
	{
		get;
		private set;
	}

	internal ISafeFileHandleStrategy SafeFileHandleStrategy
	{
		get;
		private set;
	}

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
		SafeFileHandleStrategy = new NullSafeFileHandleStrategy();
		AccessControlStrategy = new NullAccessControlStrategy();
		AddDriveFromCurrentDirectory();
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

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"MockFileSystem (directories: {_storage.Containers.Count(x => x.Value.Type == FileSystemTypes.Directory)}, files: {_storage.Containers.Count(x => x.Value.Type == FileSystemTypes.File)})";

	/// <summary>
	///     Implements a custom access control (ACL) mechanism.
	///     <para />
	///     The <see cref="IAccessControlStrategy" /> defines a method that receives two values and allows or denies access:
	///     <br />
	///     - The full path of the file or directory as first parameter<br />
	///     - The <see cref="Testably.Abstractions.Helpers.IFileSystemExtensibility" /> as second parameter
	/// </summary>
	public MockFileSystem WithAccessControlStrategy(IAccessControlStrategy accessControlStrategy)
	{
		AccessControlStrategy = accessControlStrategy;
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
	///     Registers the strategy how to deal with <see cref="SafeFileHandle" />s in the <see cref="MockFileSystem" />.
	///     <para />
	///     Defaults to <see cref="NullSafeFileHandleStrategy" />, if nothing is provided.
	/// </summary>
	public MockFileSystem WithSafeFileHandleStrategy(
		ISafeFileHandleStrategy safeFileHandleStrategy)
	{
		SafeFileHandleStrategy = safeFileHandleStrategy;
		return this;
	}

	private void AddDriveFromCurrentDirectory()
	{
		string? root = Path.GetPathRoot(System.IO.Directory.GetCurrentDirectory());
		if (root != null &&
		    root[0] != _storage.MainDrive.Name[0])
		{
			Storage.GetOrAddDrive(root);
		}
	}
}
