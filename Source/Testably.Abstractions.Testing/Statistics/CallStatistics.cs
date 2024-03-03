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

	internal IDisposable Register(string name, params object?[] parameters)
	{
		if (_statisticsGate.TryGetLock(out var release))
		{
			_calls.Enqueue(new CallStatistic(name, parameters));
		}

		return release;
	}
}
