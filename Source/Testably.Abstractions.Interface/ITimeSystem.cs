using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions;

/// <summary>
///     Allows abstracting time-related system dependencies.
/// </summary>
public interface ITimeSystem
{
	/// <summary>
	///     Abstractions for <see cref="System.DateTime" />.
	/// </summary>
	IDateTime DateTime { get; }

#if FEATURE_PERIODIC_TIMER
	/// <summary>
	///     Abstractions for <see cref="System.Threading.PeriodicTimer" />.
	/// </summary>
	IPeriodicTimerFactory PeriodicTimer { get; }
#endif

	/// <summary>
	///     Abstractions for <see cref="System.Diagnostics.Stopwatch" />.
	/// </summary>
	IStopwatchFactory Stopwatch { get; }

	/// <summary>
	///     Abstractions for <see cref="System.Threading.Tasks.Task" />.
	/// </summary>
	ITask Task { get; }

	/// <summary>
	///     Abstractions for <see cref="System.Threading.Thread" />.
	/// </summary>
	IThread Thread { get; }

	/// <summary>
	///     Abstractions for <see cref="System.Threading.Timer" />.
	/// </summary>
	ITimerFactory Timer { get; }
}
