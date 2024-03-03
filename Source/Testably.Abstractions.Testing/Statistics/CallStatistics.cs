using System;
using System.Collections.Generic;

namespace Testably.Abstractions.Testing.Statistics;

internal class CallStatistics : IStatistics
{
	private readonly IStatisticsGate _statisticsGate;
	private readonly List<CallStatistic> _calls = new();
	public IReadOnlyList<CallStatistic> Calls => _calls.AsReadOnly();

	public CallStatistics(IStatisticsGate statisticsGate)
	{
		_statisticsGate = statisticsGate;
	}

	internal IDisposable Register(string name, params object?[] parameters)
	{
		if (_statisticsGate.TryGetLock(out var release))
		{
			_calls.Add(new CallStatistic(name, parameters));
		}

		return release;
	}
}
