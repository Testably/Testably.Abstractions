using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Testably.Abstractions.Testing.Statistics;

internal class CallStatistics : IStatistics
{
	private readonly IStatisticsGate _statisticsGate;
	private readonly ConcurrentDictionary<int, MethodStatistic> _calls = new();
	public IReadOnlyDictionary<int, MethodStatistic> Methods
#if NET7_0_OR_GREATER
		=> _calls.AsReadOnly();
#else
		=> _calls;
#endif

	public CallStatistics(IStatisticsGate statisticsGate)
	{
		_statisticsGate = statisticsGate;
	}

	/// <summary>
	///     Registers the <paramref name="name" /> callback with <paramref name="parameters" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable Register(string name, params ParameterDescription[] parameters)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			int counter = _statisticsGate.GetCounter();
			_calls[counter] = new MethodStatistic(name, parameters);
		}

		return release;
	}
}
