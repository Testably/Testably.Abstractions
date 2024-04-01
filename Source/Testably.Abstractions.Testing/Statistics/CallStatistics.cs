using System;
using System.Collections.Concurrent;

namespace Testably.Abstractions.Testing.Statistics;

internal class CallStatistics<TType> : IStatistics<TType>
{
	private readonly ConcurrentQueue<MethodStatistic> _methods = new();
	private readonly string _name;
	private readonly ConcurrentQueue<PropertyStatistic> _properties = new();
	private readonly IStatisticsGate _statisticsGate;

	public CallStatistics(IStatisticsGate statisticsGate, string name)
	{
		_statisticsGate = statisticsGate;
		_name = name;
	}

	#region IStatistics Members

	/// <inheritdoc cref="IStatistics.Methods" />
	public MethodStatistic[] Methods => _methods.ToArray();

	/// <inheritdoc cref="IStatistics.Properties" />
	public PropertyStatistic[] Properties => _properties.ToArray();

	#endregion

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> _name;

	/// <summary>
	///     Registers the method <paramref name="name" /> with <paramref name="parameters" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterMethod(string name, params ParameterDescription[] parameters)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			int counter = _statisticsGate.GetCounter();
			_methods.Enqueue(new MethodStatistic(counter, name, parameters));
		}

		return release;
	}

	/// <summary>
	///     Registers the property <paramref name="name" /> callback with <paramref name="access" /> access.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterProperty(string name, PropertyAccess access)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			int counter = _statisticsGate.GetCounter();
			_properties.Enqueue(new PropertyStatistic(counter, name, access));
		}

		return release;
	}
}
