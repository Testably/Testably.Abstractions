using System;
using System.Collections.Concurrent;
using System.IO;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Statistics;

internal class PathStatistics<TFactory, TType> : CallStatistics<TFactory>, IPathStatistics<TFactory, TType>
{
	private readonly MockFileSystem _fileSystem;

	private readonly ConcurrentDictionary<string, CallStatistics<TType>> _statistics;
	private readonly IStatisticsGate _statisticsGate;

	public PathStatistics(
		IStatisticsGate statisticsGate,
		MockFileSystem fileSystem,
		string name)
		: base(statisticsGate, name)
	{
		_statistics = new ConcurrentDictionary<string, CallStatistics<TType>>(
			fileSystem.Execute.StringComparisonMode == StringComparison.Ordinal
				? StringComparer.Ordinal
				: StringComparer.OrdinalIgnoreCase);
		_statisticsGate = statisticsGate;
		_fileSystem = fileSystem;
	}

	#region IPathStatistics Members

	/// <inheritdoc cref="IPathStatistics{TFactory,TType}.this[string]" />
	public IStatistics<TType> this[string path]
	{
		get
		{
			string key = CreateKey(_fileSystem.Storage.CurrentDirectory, path);
			return _statistics.GetOrAdd(key,
				k => new CallStatistics<TType>(_statisticsGate, $"{ToString()}[{k}]"));
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
		CallStatistics<TType> callStatistics =
			_statistics.GetOrAdd(key,
				k => new CallStatistics<TType>(_statisticsGate, $"{ToString()}[{k}]"));
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
		CallStatistics<TType> callStatistics =
			_statistics.GetOrAdd(key,
				k => new CallStatistics<TType>(_statisticsGate, $"{ToString()}[{k}]"));
		return callStatistics.RegisterProperty(name, access);
	}

	private static string CreateKey(string currentDirectory, string path)
	{
		string key = string.Empty;
		if (string.IsNullOrEmpty(path))
		{
			return key;
		}

		if (path.StartsWith("//", StringComparison.Ordinal) ||
		    path.StartsWith(@"\\", StringComparison.Ordinal) ||
		    (path.Length >= 2 && path[1] == ':' && path[0].IsAsciiLetter()))
		{
			key = path;
		}
		else
		{
			key = Path.GetFullPath(Path.Combine(currentDirectory, path));
		}

		return key.TrimEnd('/', '\\');
	}
}
