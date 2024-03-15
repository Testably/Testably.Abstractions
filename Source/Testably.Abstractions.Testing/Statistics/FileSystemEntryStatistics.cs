using System;
using System.Collections.Concurrent;

namespace Testably.Abstractions.Testing.Statistics;

internal class FileSystemEntryStatistics : CallStatistics, IPathStatistics
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

	/// <inheritdoc cref="IPathStatistics.this[string]" />
	public IStatistics this[string path]
		=> _statistics.GetOrAdd(_fileSystem.Path.GetFullPath(path),
			_ => new CallStatistics(_statisticsGate));

	/// <summary>
	///     Registers the <paramref name="name" /> callback with <paramref name="parameters" /> under <paramref name="path" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterMethod(string path, string name, params ParameterDescription[] parameters)
	{
		CallStatistics callStatistics = _statistics.GetOrAdd(_fileSystem.Path.GetFullPath(path),
			_ => new CallStatistics(_statisticsGate));
		return callStatistics.RegisterMethod(name, parameters);
	}

	/// <summary>
	///     Registers the property <paramref name="name" /> callback with <paramref name="access" /> access under <paramref name="path" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterProperty(string path, string name, PropertyAccess access)
	{
		CallStatistics callStatistics = _statistics.GetOrAdd(_fileSystem.Path.GetFullPath(path),
			_ => new CallStatistics(_statisticsGate));
		return callStatistics.RegisterProperty(name, access);
	}
}
