using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions;

/// <summary>
///     Default implementation for time-related system dependencies.
///     <para />
///     Implements <seealso cref="ITimeSystem" />
/// </summary>
public sealed class RealTimeSystem : ITimeSystem
{
	/// <summary>
	///     Initializes a new instance of <see cref="RealTimeSystem" />
	///     which wraps the time-related system dependencies from <see cref="ITimeSystem" />.
	/// </summary>
	public RealTimeSystem()
	{
		DateTime = new DateTimeWrapper(this);
		DateTimeOffset = new DateTimeOffsetWrapper(this);
#if FEATURE_PERIODIC_TIMER
		PeriodicTimer = new PeriodicTimerFactory(this);
#endif
		Stopwatch = new StopwatchFactory(this);
		Task = new TaskWrapper(this);
		Thread = new ThreadWrapper(this);
		Timer = new TimerFactory(this);
		TimeZoneInfo = new TimeZoneInfoWrapper(this);
	}

	#region ITimeSystem Members

	/// <inheritdoc cref="ITimeSystem.DateTime" />
	public IDateTime DateTime { get; }

	/// <inheritdoc cref="ITimeSystem.DateTimeOffset" />
	public IDateTimeOffset DateTimeOffset { get; }

#if FEATURE_PERIODIC_TIMER
	/// <inheritdoc cref="ITimeSystem.PeriodicTimer" />
	public IPeriodicTimerFactory PeriodicTimer { get; }
#endif

	/// <inheritdoc cref="ITimeSystem.Stopwatch" />
	public IStopwatchFactory Stopwatch { get; }

	/// <inheritdoc cref="ITimeSystem.Task" />
	public ITask Task { get; }

	/// <inheritdoc cref="ITimeSystem.Thread" />
	public IThread Thread { get; }

	/// <inheritdoc cref="ITimeSystem.Timer" />
	public ITimerFactory Timer { get; }

	/// <inheritdoc cref="ITimeSystem.TimeZoneInfo" />
	public ITimeZoneInfo TimeZoneInfo { get; }

	#endregion
}
