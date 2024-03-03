using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Testably.Abstractions.Testing.Statistics;

internal class CallStatistics : IStatistics
{
	private readonly IStatisticsGate _statisticsGate;
	private readonly ConcurrentQueue<CallStatistic> _calls = new();
	public IReadOnlyList<CallStatistic> Calls => _calls.ToList().AsReadOnly();

	public CallStatistics(IStatisticsGate statisticsGate)
	{
		_statisticsGate = statisticsGate;
	}

	/// <summary>
	///     Registers the <paramref name="name" /> callback with <paramref name="parameters" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable Register(string name, params object?[] parameters)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			_calls.Enqueue(new CallStatistic(name, parameters));
		}

		return release;
	}
}
