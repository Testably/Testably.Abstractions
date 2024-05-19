using System;
using System.Collections.Concurrent;
using System.IO;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Statistics;

internal class PathStatistics<TFactory, TType> : CallStatistics<TFactory>,
	IPathStatistics<TFactory, TType>
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

	#region IPathStatistics<TFactory,TType> Members

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
	///     Registers the <paramref name="name" /> callback without parameters under <paramref name="path" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterPathMethod(string path, string name)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			string key = CreateKey(_fileSystem.Storage.CurrentDirectory, path);
			CallStatistics<TType> callStatistics =
				_statistics.GetOrAdd(key,
					k => new CallStatistics<TType>(_statisticsGate, $"{ToString()}[{k}]"));
			return callStatistics.RegisterMethodWithoutLock(release, name);
		}

		return release;
	}

	/// <summary>
	///     Registers the <paramref name="name" /> callback with <paramref name="parameter1" /> under <paramref name="path" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterPathMethod<T1>(string path, string name,
		T1 parameter1)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			string key = CreateKey(_fileSystem.Storage.CurrentDirectory, path);
			CallStatistics<TType> callStatistics =
				_statistics.GetOrAdd(key,
					k => new CallStatistics<TType>(_statisticsGate, $"{ToString()}[{k}]"));
			return callStatistics.RegisterMethodWithoutLock(release, name,
				ParameterDescription.FromParameter(parameter1));
		}

		return release;
	}

#if FEATURE_SPAN
	/// <summary>
	///     Registers the <paramref name="name" /> callback with <paramref name="parameter1" /> under <paramref name="path" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterPathMethod<T1>(string path, string name,
		ReadOnlySpan<T1> parameter1)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			string key = CreateKey(_fileSystem.Storage.CurrentDirectory, path);
			CallStatistics<TType> callStatistics =
				_statistics.GetOrAdd(key,
					k => new CallStatistics<TType>(_statisticsGate, $"{ToString()}[{k}]"));
			return callStatistics.RegisterMethodWithoutLock(release, name,
				ParameterDescription.FromParameter(parameter1));
		}

		return release;
	}
#endif

#if FEATURE_SPAN
	/// <summary>
	///     Registers the <paramref name="name" /> callback with <paramref name="parameter1" /> under <paramref name="path" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterPathMethod<T1>(string path, string name,
		Span<T1> parameter1)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			string key = CreateKey(_fileSystem.Storage.CurrentDirectory, path);
			CallStatistics<TType> callStatistics =
				_statistics.GetOrAdd(key,
					k => new CallStatistics<TType>(_statisticsGate, $"{ToString()}[{k}]"));
			return callStatistics.RegisterMethodWithoutLock(release, name,
				ParameterDescription.FromParameter(parameter1));
		}

		return release;
	}
#endif

	/// <summary>
	///     Registers the <paramref name="name" /> callback with <paramref name="parameter1" /> and
	///     <paramref name="parameter2" /> under <paramref name="path" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterPathMethod<T1, T2>(string path, string name,
		T1 parameter1, T2 parameter2)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			string key = CreateKey(_fileSystem.Storage.CurrentDirectory, path);
			CallStatistics<TType> callStatistics =
				_statistics.GetOrAdd(key,
					k => new CallStatistics<TType>(_statisticsGate, $"{ToString()}[{k}]"));
			return callStatistics.RegisterMethodWithoutLock(release, name,
				ParameterDescription.FromParameter(parameter1),
				ParameterDescription.FromParameter(parameter2));
		}

		return release;
	}

	/// <summary>
	///     Registers the <paramref name="name" /> callback with <paramref name="parameter1" />, <paramref name="parameter2" />
	///     and <paramref name="parameter3" /> under <paramref name="path" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterPathMethod<T1, T2, T3>(string path, string name,
		T1 parameter1, T2 parameter2, T3 parameter3)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			string key = CreateKey(_fileSystem.Storage.CurrentDirectory, path);
			CallStatistics<TType> callStatistics =
				_statistics.GetOrAdd(key,
					k => new CallStatistics<TType>(_statisticsGate, $"{ToString()}[{k}]"));
			return callStatistics.RegisterMethodWithoutLock(release, name,
				ParameterDescription.FromParameter(parameter1),
				ParameterDescription.FromParameter(parameter2),
				ParameterDescription.FromParameter(parameter3));
		}

		return release;
	}

	/// <summary>
	///     Registers the <paramref name="name" /> callback with <paramref name="parameter1" />, <paramref name="parameter2" />
	///     , <paramref name="parameter3" /> and <paramref name="parameter4" /> under <paramref name="path" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterPathMethod<T1, T2, T3, T4>(string path, string name,
		T1 parameter1, T2 parameter2, T3 parameter3, T4 parameter4)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			string key = CreateKey(_fileSystem.Storage.CurrentDirectory, path);
			CallStatistics<TType> callStatistics =
				_statistics.GetOrAdd(key,
					k => new CallStatistics<TType>(_statisticsGate, $"{ToString()}[{k}]"));
			return callStatistics.RegisterMethodWithoutLock(release, name,
				ParameterDescription.FromParameter(parameter1),
				ParameterDescription.FromParameter(parameter2),
				ParameterDescription.FromParameter(parameter3),
				ParameterDescription.FromParameter(parameter4));
		}

		return release;
	}

	/// <summary>
	///     Registers the <paramref name="name" /> callback with <paramref name="parameter1" />, <paramref name="parameter2" />
	///     , <paramref name="parameter3" />, <paramref name="parameter4" /> and <paramref name="parameter5" /> under
	///     <paramref name="path" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterPathMethod<T1, T2, T3, T4, T5>(string path, string name,
		T1 parameter1, T2 parameter2, T3 parameter3, T4 parameter4, T5 parameter5)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			string key = CreateKey(_fileSystem.Storage.CurrentDirectory, path);
			CallStatistics<TType> callStatistics =
				_statistics.GetOrAdd(key,
					k => new CallStatistics<TType>(_statisticsGate, $"{ToString()}[{k}]"));
			return callStatistics.RegisterMethodWithoutLock(release, name,
				ParameterDescription.FromParameter(parameter1),
				ParameterDescription.FromParameter(parameter2),
				ParameterDescription.FromParameter(parameter3),
				ParameterDescription.FromParameter(parameter4),
				ParameterDescription.FromParameter(parameter5));
		}

		return release;
	}

	/// <summary>
	///     Registers the property <paramref name="name" /> callback with <paramref name="access" /> access under
	///     <paramref name="path" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterPathProperty(string path, string name, PropertyAccess access)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			string key = CreateKey(_fileSystem.Storage.CurrentDirectory, path);
			CallStatistics<TType> callStatistics =
				_statistics.GetOrAdd(key,
					k => new CallStatistics<TType>(_statisticsGate, $"{ToString()}[{k}]"));
			return callStatistics.RegisterPropertyWithoutLock(release, name, access);
		}

		return release;
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
