using System;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.TimeSystem;

namespace Testably.Abstractions.Testing;

/// <summary>
///     <see cref="ITimeProvider" />s for use in the constructor of <see cref="MockTimeSystem" />.
/// </summary>
public static class TimeProviderFactory
{
	/// <summary>
	///     A curated, cross-platform-stable set of time zones used by <see cref="Random()" /> to pick a random local time
	///     zone. Built with <see cref="TimeZoneInfo.CreateCustomTimeZone(string, TimeSpan, string, string)" /> so it does not
	///     depend on the host's time zone database.
	/// </summary>
	private static readonly TimeZoneInfo[] SampleTimeZones =
	[
		TimeZoneInfo.Utc,
		TimeZoneInfo.CreateCustomTimeZone(
			"Sample/Plus0530", TimeSpan.FromMinutes(330), "Sample +05:30", "Sample +05:30"),
		TimeZoneInfo.CreateCustomTimeZone(
			"Sample/Minus0800", TimeSpan.FromHours(-8), "Sample -08:00", "Sample -08:00"),
		CreateDaylightSavingTimeZone(),
	];

	/// <summary>
	///     Initializes the <see cref="MockTimeSystem.TimeProvider" /> with the current time.
	///     <para />
	///     The local time zone is set to <see cref="TimeZoneInfo.Local" />.
	/// </summary>
	public static ITimeProviderFactory Now()
	{
		return new Factory(onTimeChanged
			=> new TimeProviderMock(onTimeChanged, DateTime.UtcNow, "Now", TimeZoneInfo.Local));
	}

	/// <summary>
	///     Initializes the <see cref="MockTimeSystem.TimeProvider" /> with a random time.
	///     <para />
	///     The random time increments the unix epoch by a random integer of seconds and the local time zone is picked at
	///     random from a curated, cross-platform-stable set (including a zone with daylight-saving-time transitions).
	/// </summary>
	public static ITimeProviderFactory Random()
	{
		#pragma warning disable MA0113 // Use DateTime.UnixEpoch
		DateTime randomTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
			.AddSeconds(RandomFactory.Shared.Next());
		#pragma warning restore MA0113
		var randomTimeZone = SampleTimeZones[RandomFactory.Shared.Next(SampleTimeZones.Length)];
		return new Factory(onTimeChanged
			=> new TimeProviderMock(onTimeChanged, randomTime, "Random", randomTimeZone));
	}

	/// <summary>
	///     Initializes the <see cref="MockTimeSystem.TimeProvider" /> with the specified <paramref name="time" />.
	/// </summary>
	/// <remarks>
	///     If the <paramref name="time" /> has Kind DateTimeKind.Unspecified it will be treated as if it had Kind
	///     DateTimeKind.Utc.<br />
	///     The local time zone is set to <see cref="TimeZoneInfo.Local" />.
	/// </remarks>
	public static ITimeProviderFactory Use(DateTime time)
		=> Use(time, TimeZoneInfo.Local);

	/// <summary>
	///     Initializes the <see cref="MockTimeSystem.TimeProvider" /> with the specified <paramref name="time" /> and
	///     <paramref name="localTimeZone" />.
	/// </summary>
	/// <remarks>
	///     If the <paramref name="time" /> has Kind DateTimeKind.Unspecified it will be treated as if it had Kind
	///     DateTimeKind.Utc.
	/// </remarks>
	public static ITimeProviderFactory Use(DateTime time, TimeZoneInfo localTimeZone)
	{
		_ = localTimeZone ?? throw new ArgumentNullException(nameof(localTimeZone));
		return new Factory(onTimeChanged
			=> new TimeProviderMock(onTimeChanged, time, "Fixed", localTimeZone));
	}

	private static TimeZoneInfo CreateDaylightSavingTimeZone()
	{
		var start = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(
			new DateTime(1, 1, 1, 2, 0, 0), 3, 5, DayOfWeek.Sunday);
		var end = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(
			new DateTime(1, 1, 1, 3, 0, 0), 10, 5, DayOfWeek.Sunday);
		var rule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
			DateTime.MinValue.Date, DateTime.MaxValue.Date, TimeSpan.FromHours(1), start, end);
		return TimeZoneInfo.CreateCustomTimeZone(
			"Sample/DaylightSaving", TimeSpan.FromHours(1),
			"Sample Daylight Saving Time", "Sample Standard Time", "Sample Daylight Time",
			[rule,]);
	}

	internal sealed class Factory(Func<Action<DateTime>, ITimeProvider> createCallback)
		: ITimeProviderFactory
	{
		#region ITimeProviderFactory Members

		/// <inheritdoc cref="ITimeProviderFactory.Create(Action{DateTime})" />
		public ITimeProvider Create(Action<DateTime> onTimeChanged)
			=> createCallback(onTimeChanged);

		#endregion
	}
}
