using System;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing;

/// <summary>
///     A test helper for simulating the file system. Implements <see cref="IFileSystem" />.
/// </summary>
public sealed partial class FileSystemMock : IFileSystem
{
	/// <summary>
	///     Intercept events in the <see cref="FileSystemMock" /> before they occur.
	/// </summary>
	public IInterceptionHandler Intercept => ChangeHandler;

	/// <summary>
	///     Get notified of events in the <see cref="FileSystemMock" /> after they occurred.
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
	///     The change handler used to notify about events occurring in the <see cref="FileSystemMock" />.
	/// </summary>
	internal ChangeHandlerImplementation ChangeHandler { get; }

	/// <summary>
	///     The underlying storage of directories and files.
	/// </summary>
	internal IStorage Storage => _storage;

	private readonly DirectoryMock _directoryMock;
	private readonly FileMock _fileMock;
	private readonly PathMock _pathMock;
	private readonly InMemoryStorage _storage;

	/// <summary>
	///     Initializes the <see cref="FileSystemMock" />.
	/// </summary>
	public FileSystemMock()
	{
		RandomSystem = new RandomSystemMock();
		TimeSystem = new TimeSystemMock(TimeProvider.Now());
		_pathMock = new PathMock(this);
		_storage = new InMemoryStorage(this);
		ChangeHandler = new ChangeHandlerImplementation(this);
		_directoryMock = new DirectoryMock(this);
		_fileMock = new FileMock(this);
		DirectoryInfo = new DirectoryInfoFactoryMock(this);
		DriveInfo = new DriveInfoFactoryMock(this);
		FileInfo = new FileInfoFactoryMock(this);
		FileStream = new FileStreamFactoryMock(this);
		FileSystemWatcher = new FileSystemWatcherFactoryMock(this);
	}

	#region IFileSystem Members

	/// <inheritdoc cref="IFileSystem.Directory" />
	public IFileSystem.IDirectory Directory
		=> _directoryMock;

	/// <inheritdoc cref="IFileSystem.DirectoryInfo" />
	public IFileSystem.IDirectoryInfoFactory DirectoryInfo { get; }

	/// <inheritdoc cref="IFileSystem.DriveInfo" />
	public IFileSystem.IDriveInfoFactory DriveInfo { get; }

	/// <inheritdoc cref="IFileSystem.File" />
	public IFileSystem.IFile File
		=> _fileMock;

	/// <inheritdoc cref="IFileSystem.FileInfo" />
	public IFileSystem.IFileInfoFactory FileInfo { get; }

	/// <inheritdoc cref="IFileSystem.FileStream" />
	public IFileSystem.IFileStreamFactory FileStream { get; }

	/// <inheritdoc cref="IFileSystem.FileSystemWatcher" />
	public IFileSystem.IFileSystemWatcherFactory FileSystemWatcher { get; }

	/// <inheritdoc cref="IFileSystem.Path" />
	public IFileSystem.IPath Path
		=> _pathMock;

	#endregion

	/// <summary>
	///     Implements a custom access control (ACL) mechanism.
	///     <para />
	///     The callback enables granting or rejecting access to arbitrary containers and receives
	///     the following input parameters:<br />
	///     - The full path of the file or directory as first parameter<br />
	///     - The <see cref="IFileSystem.IFileSystemExtensionContainer" /> as second parameter
	/// </summary>
	/// <param name="grantRequestCallback">
	///     A callback that receives the full path as first parameter and the
	///     <see cref="IFileSystem.IFileSystemExtensionContainer" /> as second parameter and returns <see langword="true" /> if
	///     the request was granted and <see langword="false" /> if the request was rejected.
	/// </param>
	public FileSystemMock WithAccessControl(
		Func<string, IFileSystem.IFileSystemExtensionContainer, bool>?
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
	public FileSystemMock WithDrive(string? drive,
	                                Action<IStorageDrive>? driveCallback = null)
	{
		IStorageDrive driveInfoMock =
			drive == null
				? Storage.MainDrive
				: Storage.GetOrAddDrive(drive);
		driveCallback?.Invoke(driveInfoMock);
		return this;
	}
}