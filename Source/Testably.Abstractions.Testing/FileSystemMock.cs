using System;
using Testably.Abstractions.Testing.Internal;
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
	internal IStorage Storage { get; }

	private readonly DirectoryMock _directoryMock;
	private readonly FileMock _fileMock;
	private readonly PathMock _pathMock;

	/// <summary>
	///     Initializes the <see cref="FileSystemMock" />.
	/// </summary>
	public FileSystemMock()
	{
		RandomSystem = new RandomSystem();
		TimeSystem = new TimeSystemMock(TimeProvider.Now());
		_pathMock = new PathMock(this);
		Storage = new InMemoryStorage(this);
		ChangeHandler = new ChangeHandlerImplementation(this);
		_directoryMock = new DirectoryMock(this);
		_fileMock = new FileMock(this);
		DirectoryInfo = new DirectoryInfoFactoryMock(this);
		DriveInfo = new DriveInfoFactoryMock(this);
		FileInfo = new FileInfoFactoryMock(this);
		FileStream = new FileStreamFactoryMock(this);
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

	/// <inheritdoc cref="IFileSystem.Path" />
	public IFileSystem.IPath Path
		=> _pathMock;

	#endregion

	/// <summary>
	///     Changes the parameters of the specified <paramref name="drive" />.
	///     <para />
	///     If the <paramref name="drive" /> does not exist, it will be created/mounted.
	/// </summary>
	public FileSystemMock WithDrive(string? drive,
	                                Action<IStorageDrive>? driveCallback = null)
	{
		IStorageDrive driveInfoMock = Storage.GetOrAddDrive(
			drive ?? "".PrefixRoot());
		driveCallback?.Invoke(driveInfoMock);
		return this;
	}
}

/// <summary>
///     Extension methods for the <see cref="FileSystemMock" />
/// </summary>
public static class FileSystemMockExtensions
{
	/// <summary>
	///     Changes the parameters of the default drive ('C:\' on Windows, '/' on Linux)
	/// </summary>
	public static FileSystemMock WithDrive(
		this FileSystemMock fileSystemMock,
		Action<IStorageDrive> driveCallback)
		=> fileSystemMock.WithDrive(null, driveCallback);
}