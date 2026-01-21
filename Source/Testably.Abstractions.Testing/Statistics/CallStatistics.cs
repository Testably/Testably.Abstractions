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

	#region IStatistics<TType> Members

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
	///     Registers the method <paramref name="name" /> with <paramref name="parameter1" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterMethod<T1>(string name, T1 parameter1)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			int counter = _statisticsGate.GetCounter();
			_methods.Enqueue(new MethodStatistic(counter, name,
				ParameterDescription.FromParameter(parameter1)));
		}

		return release;
	}

	/// <summary>
	///     Registers the method <paramref name="name" /> without parameters.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterMethod(string name)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			int counter = _statisticsGate.GetCounter();
			_methods.Enqueue(new MethodStatistic(counter, name));
		}

		return release;
	}

	/// <summary>
	///     Registers the method <paramref name="name" /> with <paramref name="parameter1" /> and
	///     <paramref name="parameter2" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterMethod<T1, T2>(string name, T1 parameter1, T2 parameter2)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			int counter = _statisticsGate.GetCounter();
			_methods.Enqueue(new MethodStatistic(counter, name,
				ParameterDescription.FromParameter(parameter1),
				ParameterDescription.FromParameter(parameter2)));
		}

		return release;
	}

	/// <summary>
	///     Registers the method <paramref name="name" /> with <paramref name="parameter1" />, <paramref name="parameter2" />
	///     and <paramref name="parameter3" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterMethod<T1, T2, T3>(string name, T1 parameter1, T2 parameter2,
		T3 parameter3)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			int counter = _statisticsGate.GetCounter();
			_methods.Enqueue(new MethodStatistic(counter, name,
				ParameterDescription.FromParameter(parameter1),
				ParameterDescription.FromParameter(parameter2),
				ParameterDescription.FromParameter(parameter3)));
		}

		return release;
	}

	/// <summary>
	///     Registers the method <paramref name="name" /> with <paramref name="parameter1" />, <paramref name="parameter2" />,
	///     <paramref name="parameter3" /> and <paramref name="parameter4" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterMethod<T1, T2, T3, T4>(string name, T1 parameter1, T2 parameter2,
		T3 parameter3, T4 parameter4)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			int counter = _statisticsGate.GetCounter();
			_methods.Enqueue(new MethodStatistic(counter, name,
				ParameterDescription.FromParameter(parameter1),
				ParameterDescription.FromParameter(parameter2),
				ParameterDescription.FromParameter(parameter3),
				ParameterDescription.FromParameter(parameter4)));
		}

		return release;
	}

	/// <summary>
	///     Registers the method <paramref name="name" /> with <paramref name="parameter1" />, <paramref name="parameter2" />,
	///     <paramref name="parameter3" />, <paramref name="parameter4" /> and <paramref name="parameter5" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterMethod<T1, T2, T3, T4, T5>(string name, T1 parameter1,
		T2 parameter2, T3 parameter3, T4 parameter4, T5 parameter5)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			int counter = _statisticsGate.GetCounter();
			_methods.Enqueue(new MethodStatistic(counter, name,
				ParameterDescription.FromParameter(parameter1),
				ParameterDescription.FromParameter(parameter2),
				ParameterDescription.FromParameter(parameter3),
				ParameterDescription.FromParameter(parameter4),
				ParameterDescription.FromParameter(parameter5)));
		}

		return release;
	}

	/// <summary>
	///     Registers the method <paramref name="name" /> with <paramref name="parameter1" />, <paramref name="parameter2" />,
	///     <paramref name="parameter3" />, <paramref name="parameter4" />, <paramref name="parameter5" /> and
	///     <paramref name="parameter6" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterMethod<T1, T2, T3, T4, T5, T6>(string name, T1 parameter1,
		T2 parameter2, T3 parameter3, T4 parameter4, T5 parameter5, T6 parameter6)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			int counter = _statisticsGate.GetCounter();
			_methods.Enqueue(new MethodStatistic(counter, name,
				ParameterDescription.FromParameter(parameter1),
				ParameterDescription.FromParameter(parameter2),
				ParameterDescription.FromParameter(parameter3),
				ParameterDescription.FromParameter(parameter4),
				ParameterDescription.FromParameter(parameter5),
				ParameterDescription.FromParameter(parameter6)));
		}

		return release;
	}
	#pragma warning disable S107 // Method has too many parameters
	/// <summary>
	///     Registers the method <paramref name="name" /> with <paramref name="parameter1" />, <paramref name="parameter2" />,
	///     <paramref name="parameter3" />, <paramref name="parameter4" />, <paramref name="parameter5" /> and
	///     <paramref name="parameter6" />, <paramref name="parameter7" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterMethod<T1, T2, T3, T4, T5, T6, T7>(string name, T1 parameter1,
		T2 parameter2, T3 parameter3, T4 parameter4, T5 parameter5, T6 parameter6, T7 parameter7)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			int counter = _statisticsGate.GetCounter();
			_methods.Enqueue(new MethodStatistic(counter, name,
				ParameterDescription.FromParameter(parameter1),
				ParameterDescription.FromParameter(parameter2),
				ParameterDescription.FromParameter(parameter3),
				ParameterDescription.FromParameter(parameter4),
				ParameterDescription.FromParameter(parameter5),
				ParameterDescription.FromParameter(parameter6),
				ParameterDescription.FromParameter(parameter7)));
		}

		return release;
	}
	#pragma warning restore S107 // Method has too many parameters

#if FEATURE_SPAN
	/// <summary>
	///     Registers the method <paramref name="name" /> with <paramref name="parameter1" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterMethod<T1>(string name, ReadOnlySpan<T1> parameter1)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			int counter = _statisticsGate.GetCounter();
			_methods.Enqueue(new MethodStatistic(counter, name,
				ParameterDescription.FromParameter(parameter1)));
		}

		return release;
	}
#endif

#if FEATURE_FILE_SPAN
	/// <summary>
	///     Registers the method <paramref name="name" /> with <paramref name="parameter1" /> and <paramref name="parameter2" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterMethod<T1, T2>(string name, T1 parameter1, ReadOnlySpan<T2> parameter2)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			int counter = _statisticsGate.GetCounter();
			_methods.Enqueue(new MethodStatistic(counter, name,
				ParameterDescription.FromParameter(parameter1),
				ParameterDescription.FromParameter(parameter2)));
		}

		return release;
	}
#endif

#if FEATURE_FILE_SPAN
	/// <summary>
	///     Registers the method <paramref name="name" /> with <paramref name="parameter1" />, <paramref name="parameter2" /> and <paramref name="parameter3" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterMethod<T1, T2, T3>(string name, T1 parameter1, ReadOnlySpan<T2> parameter2, T3 parameter3)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			int counter = _statisticsGate.GetCounter();
			_methods.Enqueue(new MethodStatistic(counter, name,
				ParameterDescription.FromParameter(parameter1),
				ParameterDescription.FromParameter(parameter2),
				ParameterDescription.FromParameter(parameter3)));
		}

		return release;
	}
#endif

#if FEATURE_SPAN
	/// <summary>
	///     Registers the method <paramref name="name" /> with <paramref name="parameter1" /> and
	///     <paramref name="parameter2" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterMethod<T1, T2>(string name, ReadOnlySpan<T1> parameter1,
		ReadOnlySpan<T2> parameter2)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			int counter = _statisticsGate.GetCounter();
			_methods.Enqueue(new MethodStatistic(counter, name,
				ParameterDescription.FromParameter(parameter1),
				ParameterDescription.FromParameter(parameter2)));
		}

		return release;
	}
#endif

#if FEATURE_SPAN
	/// <summary>
	///     Registers the method <paramref name="name" /> with <paramref name="parameter1" />, <paramref name="parameter2" />
	///     and <paramref name="parameter3" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterMethod<T1, T2, T3>(string name, ReadOnlySpan<T1> parameter1,
		ReadOnlySpan<T2> parameter2, ReadOnlySpan<T3> parameter3)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			int counter = _statisticsGate.GetCounter();
			_methods.Enqueue(new MethodStatistic(counter, name,
				ParameterDescription.FromParameter(parameter1),
				ParameterDescription.FromParameter(parameter2),
				ParameterDescription.FromParameter(parameter3)));
		}

		return release;
	}
#endif

#if FEATURE_SPAN
	/// <summary>
	///     Registers the method <paramref name="name" /> with <paramref name="parameter1" />, <paramref name="parameter2" />,
	///     <paramref name="parameter3" /> and <paramref name="parameter4" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterMethod<T1, T2, T3, T4>(string name, ReadOnlySpan<T1> parameter1,
		ReadOnlySpan<T2> parameter2, ReadOnlySpan<T3> parameter3, ReadOnlySpan<T4> parameter4)
	{
		if (_statisticsGate.TryGetLock(out IDisposable release))
		{
			int counter = _statisticsGate.GetCounter();
			_methods.Enqueue(new MethodStatistic(counter, name,
				ParameterDescription.FromParameter(parameter1),
				ParameterDescription.FromParameter(parameter2),
				ParameterDescription.FromParameter(parameter3),
				ParameterDescription.FromParameter(parameter4)));
		}

		return release;
	}
#endif

	/// <summary>
	///     Registers the method <paramref name="name" /> with <paramref name="parameters" />.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterMethodWithoutLock(IDisposable release, string name,
		params ParameterDescription[] parameters)
	{
		int counter = _statisticsGate.GetCounter();
		_methods.Enqueue(new MethodStatistic(counter, name, parameters));
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

	/// <summary>
	///     Registers the property <paramref name="name" /> callback with <paramref name="access" /> access.
	/// </summary>
	/// <returns>A disposable which ignores all registrations, until it is disposed.</returns>
	internal IDisposable RegisterPropertyWithoutLock(IDisposable release, string name,
		PropertyAccess access)
	{
		int counter = _statisticsGate.GetCounter();
		_properties.Enqueue(new PropertyStatistic(counter, name, access));
		return release;
	}
}
