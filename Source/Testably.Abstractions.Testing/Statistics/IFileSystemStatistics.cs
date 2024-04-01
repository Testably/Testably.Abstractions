namespace Testably.Abstractions.Testing.Statistics;

/// <summary>
///     Contains statistical information about the usage of the <see cref="MockFileSystem" />.
/// </summary>
public interface IFileSystemStatistics
{
	/// <summary>
	///     Statistical information about calls to <see cref="IFileSystem.Directory" />.
	/// </summary>
	IStatistics<IDirectory> Directory { get; }

	/// <summary>
	///     Statistical information about calls to <see cref="IFileSystem.DirectoryInfo" />.
	/// </summary>
	IPathStatistics<IDirectoryInfoFactory, IDirectoryInfo> DirectoryInfo { get; }

	/// <summary>
	///     Statistical information about calls to <see cref="IFileSystem.DriveInfo" />.
	/// </summary>
	IPathStatistics<IDriveInfoFactory, IDriveInfo> DriveInfo { get; }

	/// <summary>
	///     Statistical information about calls to <see cref="IFileSystem.File" />.
	/// </summary>
	IStatistics<IFile> File { get; }

	/// <summary>
	///     Statistical information about calls to <see cref="IFileSystem.FileInfo" />.
	/// </summary>
	IPathStatistics<IFileInfoFactory, IFileInfo> FileInfo { get; }

	/// <summary>
	///     Statistical information about calls to <see cref="IFileSystem.FileStream" />.
	/// </summary>
	IPathStatistics<IFileStreamFactory, FileSystemStream> FileStream { get; }

	/// <summary>
	///     Statistical information about calls to <see cref="IFileSystem.FileSystemWatcher" />.
	/// </summary>
	IPathStatistics<IFileSystemWatcherFactory, IFileSystemWatcher> FileSystemWatcher { get; }

	/// <summary>
	///     Statistical information about calls to <see cref="IFileSystem.Path" />.
	/// </summary>
	IStatistics<IPath> Path { get; }
}
