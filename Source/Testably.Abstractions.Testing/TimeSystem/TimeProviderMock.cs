using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
#if NETSTANDARD2_0
using Testably.Abstractions.TimeSystem;
#endif

namespace Testably.Abstractions.Testing.TimeSystem;

internal sealed class TimeProviderMock : ITimeProvider
{
	private DateTime _now;
	private long _elapsedTicks;
	private readonly Action<DateTime> _onTimeChanged;
	private readonly string _description;
	private readonly Dictionary<string, TimeZoneInfo> _timeZones;
	private TimeZoneInfo _localTimeZone;
#if NET9_0_OR_GREATER
	private readonly System.Threading.Lock _lock = new();
#else
	private readonly object _lock = new();
#endif

	public TimeProviderMock(Action<DateTime> onTimeChanged, DateTime now, string description,
		TimeZoneInfo localTimeZone)
	{
		_now = now.Kind == DateTimeKind.Unspecified
			? DateTime.SpecifyKind(now, DateTimeKind.Utc)
			: now;
		_elapsedTicks = _now.Ticks;
		StartTime = _now;
		_onTimeChanged = onTimeChanged;
		_description = description;
		_timeZones = TimeZoneInfo.GetSystemTimeZones()
			.GroupBy(timeZone => timeZone.Id, StringComparer.Ordinal)
			.ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
		_localTimeZone = localTimeZone;
		_timeZones[localTimeZone.Id] = localTimeZone;
	}

	#region ITimeProvider Members

	/// <inheritdoc cref="ITimeProvider.LocalTimeZone" />
	public TimeZoneInfo LocalTimeZone
	{
		get
		{
			lock (_lock) { return _localTimeZone; }
		}
		set
		{
			lock (_lock)
			{
				_localTimeZone = value;
				_timeZones[value.Id] = value;
			}
		}
	}

	/// <inheritdoc cref="ITimeProvider.MaxValue" />
	public DateTime MaxValue { get; set; } = DateTime.MaxValue;

	/// <inheritdoc cref="ITimeProvider.MinValue" />
	public DateTime MinValue { get; set; } = DateTime.MinValue;

#if NETSTANDARD2_0
	/// <inheritdoc cref="IDateTime.UnixEpoch" />
	public DateTime UnixEpoch { get; set; } =
		new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
#else
	/// <inheritdoc cref="ITimeProvider.UnixEpoch" />
	public DateTime UnixEpoch { get; set; } = DateTime.UnixEpoch;
#endif

	/// <inheritdoc cref="ITimeProvider.StartTime" />
	public DateTime StartTime { get; }

	/// <inheritdoc cref="ITimeProvider.ElapsedTicks" />
	public long ElapsedTicks
	{
		get
		{
			lock (_lock) { return _elapsedTicks; }
		}
	}

	/// <inheritdoc cref="ITimeProvider.AdvanceBy(TimeSpan)" />
	public void AdvanceBy(TimeSpan interval)
	{
		lock (_lock)
		{
			_now = _now.Add(interval);
			_elapsedTicks += interval.Ticks;
			_onTimeChanged.Invoke(_now);
		}
	}

	/// <inheritdoc cref="ITimeProvider.FindSystemTimeZoneById(string)" />
	public TimeZoneInfo FindSystemTimeZoneById(string id)
	{
		lock (_lock)
		{
			if (_timeZones.TryGetValue(id, out TimeZoneInfo? timeZone))
			{
				return timeZone;
			}
		}

		throw new TimeZoneNotFoundException(
			$"The time zone ID '{id}' was not found on the local computer.");
	}

	/// <inheritdoc cref="ITimeProvider.GetSystemTimeZones()" />
	public ReadOnlyCollection<TimeZoneInfo> GetSystemTimeZones()
	{
		lock (_lock)
		{
			return new ReadOnlyCollection<TimeZoneInfo>(_timeZones.Values
				.OrderBy(timeZone => timeZone.BaseUtcOffset)
				.ThenBy(timeZone => timeZone.Id, StringComparer.Ordinal)
				.ToList());
		}
	}

	/// <inheritdoc cref="ITimeProvider.Read()" />
	public DateTime Read()
	{
		lock (_lock)
		{
			return _now;
		}
	}

	/// <inheritdoc cref="ITimeProvider.RegisterTimeZone(TimeZoneInfo)" />
	public void RegisterTimeZone(TimeZoneInfo timeZone)
	{
		lock (_lock)
		{
			_timeZones[timeZone.Id] = timeZone;
		}
	}

	/// <inheritdoc cref="ITimeProvider.SetTo(DateTime)" />
	public void SetTo(DateTime value)
	{
		lock (_lock)
		{
			_now = value;
			_onTimeChanged.Invoke(_now);
		}
	}

	#endregion

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> _description;
}
