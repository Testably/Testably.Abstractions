namespace Testably.Abstractions.Testing.Statistics;

/// <summary>
///     Contains statistical information about the usage of the <see cref="MockFileSystem" />.
/// </summary>
public interface IFileSystemStatistics
{
	/// <summary>
	///     Statistical information about calls to <see cref="IFileSystem.Directory" />.
	/// </summary>
	IStatistics Directory { get; }

	/// <summary>
	///     Statistical information about calls to <see cref="IFileSystem.DirectoryInfo" />.
	/// </summary>
	IFileSystemEntryStatistics DirectoryInfo { get; }

	/// <summary>
	///     Statistical information about calls to <see cref="IFileSystem.DriveInfo" />.
	/// </summary>
	IFileSystemEntryStatistics DriveInfo { get; }

	/// <summary>
	///     Statistical information about calls to <see cref="IFileSystem.File" />.
	/// </summary>
	IStatistics File { get; }

	/// <summary>
	///     Statistical information about calls to <see cref="IFileSystem.FileInfo" />.
	/// </summary>
	IFileSystemEntryStatistics FileInfo { get; }

	/// <summary>
	///     Statistical information about calls to <see cref="IFileSystem.FileStream" />.
	/// </summary>
	IFileSystemEntryStatistics FileStream { get; }

	/// <summary>
	///     Statistical information about calls to <see cref="IFileSystem.FileSystemWatcher" />.
	/// </summary>
	IFileSystemEntryStatistics FileSystemWatcher { get; }

	/// <summary>
	///     Statistical information about calls to <see cref="IFileSystem.Path" />.
	/// </summary>
	IStatistics Path { get; }
}
