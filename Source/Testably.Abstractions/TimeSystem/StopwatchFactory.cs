using System.Diagnostics;
#if FEATURE_STOPWATCH_GETELAPSEDTIME
using System;
#endif

namespace Testably.Abstractions.TimeSystem;

internal sealed class StopwatchFactory : IStopwatchFactory
{
	internal StopwatchFactory(RealTimeSystem timeSystem)
	{
		TimeSystem = timeSystem;
	}

	#region IStopwatchFactory Members

	/// <inheritdoc cref="IStopwatchFactory.Frequency" />
	public long Frequency
		=> Stopwatch.Frequency;

	/// <inheritdoc cref="IStopwatchFactory.IsHighResolution" />
	public bool IsHighResolution
		=> Stopwatch.IsHighResolution;

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem { get; }

#if FEATURE_STOPWATCH_GETELAPSEDTIME
	/// <inheritdoc cref="IStopwatchFactory.GetElapsedTime(long)" />
	public TimeSpan GetElapsedTime(long startingTimestamp)
		=> Stopwatch.GetElapsedTime(startingTimestamp);
#endif

#if FEATURE_STOPWATCH_GETELAPSEDTIME
	/// <inheritdoc cref="IStopwatchFactory.GetElapsedTime(long, long)" />
	public TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp)
		=> Stopwatch.GetElapsedTime(startingTimestamp, endingTimestamp);
#endif

	/// <inheritdoc cref="IStopwatchFactory.GetTimestamp()" />
	public long GetTimestamp()
		=> Stopwatch.GetTimestamp();

	/// <inheritdoc cref="IStopwatchFactory.New()" />
	public IStopwatch New()
		=> Wrap(new Stopwatch());

	/// <inheritdoc cref="IStopwatchFactory.StartNew()" />
	public IStopwatch StartNew()
		=> Wrap(Stopwatch.StartNew());

	/// <inheritdoc cref="IStopwatchFactory.Wrap(Stopwatch)" />
	public IStopwatch Wrap(Stopwatch stopwatch)
		=> new StopwatchWrapper(TimeSystem, stopwatch);

	#endregion
}
