using System;
using System.Collections.Concurrent;

namespace Testably.Abstractions.Testing.Statistics;

internal class CallStatistics : IStatistics
{
	private readonly IStatisticsGate _statisticsGate;
	private readonly ConcurrentQueue<MethodStatistic> _calls = new();
	public MethodStatistic[] Methods => _calls.ToArray();

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
			_calls.Enqueue(new MethodStatistic(counter, name, parameters));
		}

		return release;
	}
}
