using System;
using System.Collections.Concurrent;
using System.IO;

namespace Testably.Abstractions.Testing.Statistics;

internal class PathStatistics : CallStatistics, IPathStatistics
{
	private readonly MockFileSystem _fileSystem;

	private readonly ConcurrentDictionary<string, CallStatistics> _statistics = new();
	private readonly IStatisticsGate _statisticsGate;

	public PathStatistics(
		IStatisticsGate statisticsGate,
		MockFileSystem fileSystem)
		: base(statisticsGate)
	{
		_statisticsGate = statisticsGate;
		_fileSystem = fileSystem;
	}

	#region IPathStatistics Members

	/// <inheritdoc cref="IPathStatistics.this[string]" />
	public IStatistics this[string path]
	{
		get
		{
			string key = CreateKey(_fileSystem.Storage.CurrentDirectory, path);
			return _statistics.GetOrAdd(key, _ => new CallStatistics(_statisticsGate));
		}
	}

	#endregion

	/// <summary>
	///     Registers the <paramref name="name" /> callback with <paramref name="parameters" /> under <paramref name="path" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterMethod(string path, string name,
		params ParameterDescription[] parameters)
	{
		string key = CreateKey(_fileSystem.Storage.CurrentDirectory, path);
		CallStatistics callStatistics =
			_statistics.GetOrAdd(key, _ => new CallStatistics(_statisticsGate));
		return callStatistics.RegisterMethod(name, parameters);
	}

	/// <summary>
	///     Registers the property <paramref name="name" /> callback with <paramref name="access" /> access under
	///     <paramref name="path" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterProperty(string path, string name, PropertyAccess access)
	{
		string key = CreateKey(_fileSystem.Storage.CurrentDirectory, path);
		CallStatistics callStatistics =
			_statistics.GetOrAdd(key, _ => new CallStatistics(_statisticsGate));
		return callStatistics.RegisterProperty(name, access);
	}

	private static string CreateKey(string currentDirectory, string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return string.Empty;
		}

		if (path.StartsWith("//") || path.StartsWith("\\"))
		{
			return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		}

		if (path.Length == 2 && path.EndsWith(":"))
		{
			return path;
		}

		return Path.GetFullPath(Path.Combine(currentDirectory, path))
			.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
	}
}
