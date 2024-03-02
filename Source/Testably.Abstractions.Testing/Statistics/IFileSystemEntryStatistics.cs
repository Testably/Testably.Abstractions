namespace Testably.Abstractions.Testing.Statistics;

public interface IFileSystemEntryStatistics : IStatistics
{
	IStatistics this[string path]
	{
		get;
	}
}
