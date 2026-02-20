using System;
using System.Diagnostics;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.TimeSystem;
using IStopwatch = Testably.Abstractions.TimeSystem.IStopwatch;

namespace Testably.Abstractions.Testing.TimeSystem;

internal sealed class StopwatchFactoryMock : IStopwatchFactory
{
	private readonly MockTimeSystem _mockTimeSystem;

	internal StopwatchFactoryMock(MockTimeSystem timeSystem)
	{
		_mockTimeSystem = timeSystem;
	}

	#region IStopwatchFactory Members

	/// <inheritdoc cref="IStopwatchFactory.Frequency" />
	public long Frequency => TimeSpan.TicksPerSecond;

	/// <inheritdoc cref="IStopwatchFactory.IsHighResolution" />
	public bool IsHighResolution => true;

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem
		=> _mockTimeSystem;

#if FEATURE_STOPWATCH_GETELAPSEDTIME
	/// <inheritdoc cref="IStopwatchFactory.GetElapsedTime(long)" />
	public TimeSpan GetElapsedTime(long startingTimestamp)
		=> GetElapsedTime(startingTimestamp, GetTimestamp());
#endif

#if FEATURE_STOPWATCH_GETELAPSEDTIME
	/// <inheritdoc cref="IStopwatchFactory.GetElapsedTime(long, long)" />
	public TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp)
		=> TimeSpan.FromTicks(endingTimestamp - startingTimestamp);
#endif

	/// <inheritdoc cref="IStopwatchFactory.GetTimestamp()" />
	public long GetTimestamp()
		=> _mockTimeSystem.TimeProvider.Read().Ticks;

	/// <inheritdoc cref="IStopwatchFactory.New()" />
	public IStopwatch New()
	{
		StopwatchMock stopwatchMock = new(_mockTimeSystem);
		return stopwatchMock;
	}

	/// <inheritdoc cref="IStopwatchFactory.StartNew()" />
	public IStopwatch StartNew()
	{
		IStopwatch stopwatch = New();
		stopwatch.Start();
		return stopwatch;
	}

	/// <inheritdoc cref="IStopwatchFactory.Wrap(Stopwatch)" />
	public IStopwatch Wrap(Stopwatch stopwatch)
		=> throw ExceptionFactory.NotSupportedStopwatchWrapping();

	#endregion
}
