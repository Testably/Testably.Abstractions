using System;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.TimeSystem;

/// <summary>
///     The time provider for the <see cref="MockTimeSystem" />
/// </summary>
public interface ITimeProvider
{
#if FEATURE_PERIODIC_TIMER
	/// <summary>
	///     The elapsed ticks represent a monotonic clock that is not affected by changes to the system time.
	/// </summary>
	/// <remarks>
	///     It is used internally for the <see cref="IPeriodicTimer" />, the <see cref="IStopwatch" /> and the <see cref="ITimer" />.<br />
	///     The value is not affected by changes to the system time (see <see cref="SetTo(DateTime)" />), but it is affected by the <see cref="AdvanceBy(TimeSpan)" /> method.<br />
	/// </remarks>
#else
	/// <summary>
	///     The elapsed ticks represent a monotonic clock that is not affected by changes to the system time.
	/// </summary>
	/// <remarks>
	///     It is used internally for the <see cref="IStopwatch" /> and the <see cref="ITimer" />.<br />
	///     The value is not affected by changes to the system time (see <see cref="SetTo(DateTime)" />), but it is affected by the <see cref="AdvanceBy(TimeSpan)" /> method.<br />
	/// </remarks>
#endif
	long ElapsedTicks { get; }

	/// <summary>
	///     Gets or sets the <see cref="IDateTime.MaxValue" />
	/// </summary>
	DateTime MaxValue { get; set; }

	/// <summary>
	///     Gets or sets the <see cref="IDateTime.MinValue" />
	/// </summary>
	DateTime MinValue { get; set; }

	/// <summary>
	///     The start time of the time provider.
	/// </summary>
	/// <remarks>
	///     This is the time the time provider was initialized at the beginning of the test.
	/// </remarks>
	DateTime StartTime { get; }

	/// <summary>
	///     Gets or sets the <see cref="IDateTime.UnixEpoch" />
	/// </summary>
	DateTime UnixEpoch { get; set; }

	/// <summary>
	///     Advances the currently simulated system time by the <paramref name="interval" />
	/// </summary>
	/// <param name="interval"></param>
	void AdvanceBy(TimeSpan interval);

	/// <summary>
	///     Reads the currently simulated system time.
	/// </summary>
	DateTime Read();

	/// <summary>
	///     Sets the currently simulated system time to the specified <paramref name="value" />.
	/// </summary>
	void SetTo(DateTime value);
}
