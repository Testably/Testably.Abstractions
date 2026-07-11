using System;
using System.Collections.ObjectModel;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.TimeSystem;

/// <summary>
///     The time provider for the <see cref="MockTimeSystem" />
/// </summary>
public interface ITimeProvider
{
#pragma warning disable MA0202 // Branches differ only in XML doc comments
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
#pragma warning restore MA0202
	long ElapsedTicks { get; }

	/// <summary>
	///     Gets or sets the local time zone used when converting the currently simulated system time to local time
	///     (e.g. for <see cref="IDateTime.Now" /> or <see cref="IDateTimeOffset.Now" />).
	/// </summary>
	/// <remarks>
	///     Setting the local time zone also registers it (see <see cref="RegisterTimeZone(TimeZoneInfo)" />), so that it can
	///     be resolved via <see cref="ITimeZoneInfo.FindSystemTimeZoneById(string)" />.
	/// </remarks>
	TimeZoneInfo LocalTimeZone { get; set; }

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
	///     Returns the registered time zone with the given <paramref name="id" />.
	/// </summary>
	/// <exception cref="TimeZoneNotFoundException">
	///     No time zone is registered under the given <paramref name="id" />.
	/// </exception>
	TimeZoneInfo FindSystemTimeZoneById(string id);

	/// <summary>
	///     Returns all registered time zones.
	/// </summary>
	/// <remarks>
	///     The registry is initially seeded from the host's <see cref="TimeZoneInfo.GetSystemTimeZones()" /> and can be
	///     extended via <see cref="RegisterTimeZone(TimeZoneInfo)" />.
	/// </remarks>
	ReadOnlyCollection<TimeZoneInfo> GetSystemTimeZones();

	/// <summary>
	///     Reads the currently simulated system time.
	/// </summary>
	DateTime Read();

	/// <summary>
	///     Registers the <paramref name="timeZone" /> so that it can be resolved via
	///     <see cref="FindSystemTimeZoneById(string)" /> and is included in <see cref="GetSystemTimeZones()" />.
	/// </summary>
	/// <remarks>
	///     An already registered time zone with the same <see cref="TimeZoneInfo.Id" /> is replaced.
	/// </remarks>
	void RegisterTimeZone(TimeZoneInfo timeZone);

	/// <summary>
	///     Sets the currently simulated system time to the specified <paramref name="value" />.
	/// </summary>
	void SetTo(DateTime value);
}
