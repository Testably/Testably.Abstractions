namespace Testably.Abstractions.Testing.Statistics;

internal sealed class FileSystemStatistics : IFileSystemStatistics
{
	internal CallStatistics FileStatistic = new CallStatistics();
	internal CallStatistics DirectoryStatistic = new CallStatistics();
	internal readonly FileSystemEntryStatistics DirectoryInfoStatistic;

	public FileSystemStatistics(MockFileSystem fileSystem)
	{
		DirectoryInfoStatistic = new FileSystemEntryStatistics(fileSystem);
	}

	/// <inheritdoc />
	public IStatistics Directory => DirectoryStatistic;

	/// <inheritdoc />
	public IFileSystemEntryStatistics DirectoryInfo => DirectoryInfoStatistic;

	/// <inheritdoc />
	public IStatistics File => FileStatistic;
}
