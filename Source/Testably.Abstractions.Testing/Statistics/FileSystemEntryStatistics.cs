using System;
using System.Collections.Concurrent;

namespace Testably.Abstractions.Testing.Statistics;

internal class FileSystemEntryStatistics : CallStatistics, IFileSystemEntryStatistics
{
	private readonly MockFileSystem _fileSystem;

	public FileSystemEntryStatistics(MockFileSystem fileSystem)
	{
		_fileSystem = fileSystem;
	}

	private readonly ConcurrentDictionary<string, CallStatistics> _statistics = new();

	/// <inheritdoc />
	public IStatistics this[string path]
		=> _statistics.GetOrAdd(_fileSystem.Path.GetFullPath(path), _ => new CallStatistics());

	internal IDisposable Register(string path, string name, params object?[] parameters)
	{
		var callStatistics = _statistics.GetOrAdd(_fileSystem.Path.GetFullPath(path), _ => new CallStatistics());
		return callStatistics.Register(name, parameters);
	}
}
