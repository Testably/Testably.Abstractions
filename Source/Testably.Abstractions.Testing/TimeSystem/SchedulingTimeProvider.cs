using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Testing.TimeSystem;

/// <summary>
///     Wraps the <see cref="ITimeProvider" /> of the <see cref="MockTimeSystem" /> and fires the scheduled timers in the
///     order of their due time, with the clock at the due time of each timer while its callback runs.
/// </summary>
/// <remarks>
///     Taking the next timer and moving the clock to its due time is one step, but the callback runs after it, so that a
///     callback may wait for another thread that moves the clock. Such a thread may move the clock past the due time of
///     a running callback. When the wrapped provider is advanced directly, the timers that became due are fired
///     afterward on the thread pool, as the clock is already past them.
/// </remarks>
internal sealed class SchedulingTimeProvider : ITimeProvider
{
	[ThreadStatic] private static int _steppingDepth;

#if NET9_0_OR_GREATER
	private readonly Lock _advanceLock = new();
#else
	private readonly object _advanceLock = new();
#endif
	private readonly ITimeProvider _inner;
#if NET9_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif
	private readonly List<ScheduledTimer> _timers = [];
	private long _nextSequence;

	public SchedulingTimeProvider(ITimeProvider inner)
	{
		_inner = inner;
	}

	#region ITimeProvider Members

	/// <inheritdoc cref="ITimeProvider.ElapsedTicks" />
	public long ElapsedTicks
		=> _inner.ElapsedTicks;

	/// <inheritdoc cref="ITimeProvider.LocalTimeZone" />
	public TimeZoneInfo LocalTimeZone
	{
		get => _inner.LocalTimeZone;
		set => _inner.LocalTimeZone = value;
	}

	/// <inheritdoc cref="ITimeProvider.MaxValue" />
	public DateTime MaxValue
	{
		get => _inner.MaxValue;
		set => _inner.MaxValue = value;
	}

	/// <inheritdoc cref="ITimeProvider.MinValue" />
	public DateTime MinValue
	{
		get => _inner.MinValue;
		set => _inner.MinValue = value;
	}

	/// <inheritdoc cref="ITimeProvider.StartTime" />
	public DateTime StartTime
		=> _inner.StartTime;

	/// <inheritdoc cref="ITimeProvider.UnixEpoch" />
	public DateTime UnixEpoch
	{
		get => _inner.UnixEpoch;
		set => _inner.UnixEpoch = value;
	}

	/// <inheritdoc cref="ITimeProvider.AdvanceBy(TimeSpan)" />
	/// <remarks>
	///     The timers that are due within the <paramref name="interval" /> fire on the calling thread, in the order of
	///     their due time, while the clock is at their due time.
	/// </remarks>
	public void AdvanceBy(TimeSpan interval)
	{
		if (interval < TimeSpan.Zero)
		{
			lock (_advanceLock)
			{
				Step(() => _inner.AdvanceBy(interval));
			}

			return;
		}

		AdvanceBy(interval, null);
	}

	/// <inheritdoc cref="ITimeProvider.FindSystemTimeZoneById(string)" />
	public TimeZoneInfo FindSystemTimeZoneById(string id)
		=> _inner.FindSystemTimeZoneById(id);

	/// <inheritdoc cref="ITimeProvider.GetSystemTimeZones()" />
	public ReadOnlyCollection<TimeZoneInfo> GetSystemTimeZones()
		=> _inner.GetSystemTimeZones();

	/// <inheritdoc cref="ITimeProvider.Read()" />
	public DateTime Read()
		=> _inner.Read();

	/// <inheritdoc cref="ITimeProvider.RegisterTimeZone(TimeZoneInfo)" />
	public void RegisterTimeZone(TimeZoneInfo timeZone)
		=> _inner.RegisterTimeZone(timeZone);

	/// <inheritdoc cref="ITimeProvider.SetTo(DateTime)" />
	public void SetTo(DateTime value)
	{
		lock (_advanceLock)
		{
			Step(() => _inner.SetTo(value));
		}
	}

	#endregion

	/// <summary>
	///     Adds the <paramref name="ticks" /> to the <paramref name="dueTicks" />, without overflowing past the end of the
	///     clock.
	/// </summary>
	public static long AddTicks(long dueTicks, long ticks)
		=> ticks > long.MaxValue - dueTicks ? long.MaxValue : dueTicks + ticks;

	/// <summary>
	///     Moves the clock by the <paramref name="interval" /> and fires the timers due until then, in the order of their
	///     due time.
	/// </summary>
	/// <returns>
	///     <see langword="false" />, when the <paramref name="stopAfter" /> predicate stopped the clock after a fired
	///     timer, otherwise <see langword="true" />.
	/// </returns>
	public bool AdvanceBy(TimeSpan interval, Func<ScheduledTimer, bool>? stopAfter)
		=> AdvanceTo(AddTicks(_inner.ElapsedTicks, interval.Ticks), stopAfter);

	/// <summary>
	///     Moves the clock to the <paramref name="targetTicks" /> and fires the timers due until then, in the order of
	///     their due time.
	/// </summary>
	/// <returns>
	///     <see langword="false" />, when the <paramref name="stopAfter" /> predicate stopped the clock after a fired
	///     timer, otherwise <see langword="true" />.
	/// </returns>
	public bool AdvanceTo(long targetTicks, Func<ScheduledTimer, bool>? stopAfter = null)
	{
		while (TakeNext(targetTicks) is { } next)
		{
			next.Fire();
			if (stopAfter?.Invoke(next) == true)
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	///     Fires the timers that became due while the wrapped provider was advanced directly.
	/// </summary>
	public void OnTimeChanged()
	{
		if (_steppingDepth == 0)
		{
			_ = Task.Run(FireDue);
		}
	}

	/// <summary>
	///     Schedules the <paramref name="timer" />. A timer that is already due fires on the thread pool.
	/// </summary>
	public void Schedule(ScheduledTimer timer)
	{
		lock (_lock)
		{
			timer.Sequence = _nextSequence++;
			timer.IsScheduled = true;
			_timers.Add(timer);
		}

		if (timer.DueTicks <= _inner.ElapsedTicks)
		{
			_ = Task.Run(FireDue);
		}
	}

	/// <summary>
	///     Removes the <paramref name="timer" /> from the schedule, if it did not fire yet.
	/// </summary>
	public void Unschedule(ScheduledTimer timer)
	{
		lock (_lock)
		{
			if (timer.IsScheduled)
			{
				timer.IsScheduled = false;
				_timers.Remove(timer);
			}
		}
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string? ToString()
		=> _inner.ToString();

	private void FireDue()
		=> AdvanceBy(TimeSpan.Zero, null);

	private void MoveTo(long ticks)
	{
		long now = _inner.ElapsedTicks;
		if (ticks > now)
		{
			Step(() => _inner.AdvanceBy(TimeSpan.FromTicks(ticks - now)));
		}
	}

	private static void Step(Action changeTime)
	{
		_steppingDepth++;
		try
		{
			changeTime();
		}
		finally
		{
			_steppingDepth--;
		}
	}

	/// <summary>
	///     Takes the earliest timer due until the <paramref name="targetTicks" /> from the schedule and moves the clock to
	///     its due time, or moves the clock to the <paramref name="targetTicks" /> and returns <see langword="null" />.
	/// </summary>
	/// <remarks>
	///     The schedule is not locked while the clock moves, as moving it notifies about the time change, whose callbacks
	///     may change timers.
	/// </remarks>
	private ScheduledTimer? TakeNext(long targetTicks)
	{
		lock (_advanceLock)
		{
			ScheduledTimer? next;
			lock (_lock)
			{
				next = null;
				foreach (ScheduledTimer timer in _timers)
				{
					if (timer.DueTicks <= targetTicks && (next is null || timer.DueTicks < next.DueTicks ||
					                                      (timer.DueTicks == next.DueTicks &&
					                                       timer.Sequence < next.Sequence)))
					{
						next = timer;
					}
				}

				if (next is not null)
				{
					next.IsScheduled = false;
					_timers.Remove(next);
				}
			}

			MoveTo(next?.DueTicks ?? targetTicks);
			return next;
		}
	}

	/// <summary>
	///     A timer on the schedule of the <see cref="SchedulingTimeProvider" />.
	/// </summary>
	internal sealed class ScheduledTimer(long dueTicks, Action fire)
	{
		private volatile bool _isScheduled;

		/// <summary>
		///     The <see cref="ElapsedTicks" /> at which the timer is due.
		/// </summary>
		public long DueTicks { get; } = dueTicks;

		/// <summary>
		///     Whether the timer is on the schedule, i.e. neither fired nor removed.
		/// </summary>
		public bool IsScheduled
		{
			get => _isScheduled;
			set => _isScheduled = value;
		}

		/// <summary>
		///     The order of timers with the same due time.
		/// </summary>
		public long Sequence { get; set; }

		/// <summary>
		///     Runs the callback of the timer.
		/// </summary>
		public void Fire() => fire();
	}
}
