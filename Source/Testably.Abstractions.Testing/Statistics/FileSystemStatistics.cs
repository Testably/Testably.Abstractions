namespace Testably.Abstractions.Testing.Statistics;

internal sealed class FileSystemStatistics : IFileSystemStatistics
{
	internal CallStatistics FileStatistic = new();
	internal CallStatistics DirectoryStatistic = new();
	internal readonly FileSystemEntryStatistics DirectoryInfoStatistic;
	internal readonly FileSystemEntryStatistics FileInfoStatistics;
	internal readonly FileSystemEntryStatistics DriveInfoStatistics;

	public FileSystemStatistics(MockFileSystem fileSystem)
	{
		DirectoryInfoStatistic = new FileSystemEntryStatistics(fileSystem);
		FileInfoStatistics = new FileSystemEntryStatistics(fileSystem);
		DriveInfoStatistics = new FileSystemEntryStatistics(fileSystem);
	}

	/// <inheritdoc />
	public IStatistics Directory => DirectoryStatistic;

	/// <inheritdoc />
	public IFileSystemEntryStatistics DirectoryInfo => DirectoryInfoStatistic;

	/// <inheritdoc />
	public IFileSystemEntryStatistics DriveInfo => DriveInfoStatistics;

	/// <inheritdoc />
	public IStatistics File => FileStatistic;

	/// <inheritdoc />
	public IFileSystemEntryStatistics FileInfo => FileInfoStatistics;
}
