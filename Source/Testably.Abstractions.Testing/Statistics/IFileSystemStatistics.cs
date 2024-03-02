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
	///     Statistical information about calls to <see cref="IFileSystem.File" />.
	/// </summary>
	IStatistics File { get; }
}
