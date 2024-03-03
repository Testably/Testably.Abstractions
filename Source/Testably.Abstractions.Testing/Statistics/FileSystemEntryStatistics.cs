using System;
using System.Collections.Concurrent;

namespace Testably.Abstractions.Testing.Statistics;

internal class FileSystemEntryStatistics : CallStatistics, IFileSystemEntryStatistics
{
	private readonly IStatisticsGate _statisticsGate;
	private readonly MockFileSystem _fileSystem;

	public FileSystemEntryStatistics(
		IStatisticsGate statisticsGate,
		MockFileSystem fileSystem)
		: base(statisticsGate)
	{
		_statisticsGate = statisticsGate;
		_fileSystem = fileSystem;
	}

	private readonly ConcurrentDictionary<string, CallStatistics> _statistics = new();

	/// <inheritdoc />
	public IStatistics this[string path]
		=> _statistics.GetOrAdd(_fileSystem.Path.GetFullPath(path), _ => new CallStatistics(_statisticsGate));

	internal IDisposable Register(string path, string name, params object?[] parameters)
	{
		CallStatistics callStatistics = _statistics.GetOrAdd(_fileSystem.Path.GetFullPath(path),
			_ => new CallStatistics(_statisticsGate));
		return callStatistics.Register(name, parameters);
	}
}
