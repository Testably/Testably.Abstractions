namespace Testably.Abstractions;

/// <summary>
///     Allows abstracting file-related system dependencies.
/// </summary>
public interface IFileSystem
{
	/// <summary>
	///     Abstractions for <see cref="System.IO.Directory" />.
	/// </summary>
	IDirectory Directory { get; }

	/// <summary>
	///     Factory for abstracting creation of <see cref="System.IO.DirectoryInfo" />.
	/// </summary>
	IDirectoryInfoFactory DirectoryInfo { get; }

	/// <summary>
	///     Factory for abstracting creation of <see cref="System.IO.DriveInfo" />.
	/// </summary>
	IDriveInfoFactory DriveInfo { get; }

	/// <summary>
	///     Abstractions for <see cref="System.IO.File" />.
	/// </summary>
	IFile File { get; }

	/// <summary>
	///     Factory for abstracting creation of <see cref="System.IO.FileInfo" />.
	/// </summary>
	IFileInfoFactory FileInfo { get; }

	/// <summary>
	///     Factory for abstracting creation of <see cref="System.IO.FileStream" />.
	/// </summary>
	IFileStreamFactory FileStream { get; }

	/// <summary>
	///     Factory for abstracting creation of <see cref="System.IO.FileSystemWatcher" />.
	/// </summary>
	IFileSystemWatcherFactory FileSystemWatcher { get; }

	/// <summary>
	///     Abstractions for <see cref="System.IO.Path" />.
	/// </summary>
	IPath Path { get; }
}