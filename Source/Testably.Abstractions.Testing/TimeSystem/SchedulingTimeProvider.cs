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
///     a running callback, so the order holds for the timers that one thread fires. All timers that advance the clock
///     share one thread. When the wrapped provider is advanced directly, the timers that became due are fired afterward
///     on the thread pool, as the clock is already past them.
/// </remarks>
internal sealed class SchedulingTimeProvider : ITimeProvider
{
	[ThreadStatic] private static int _steppingDepth;

#if NET9_0_OR_GREATER
	private readonly Lock _advanceLock = new();
#else
	private readonly object _advanceLock = new();
#endif
	private readonly SortedSet<ScheduledTimer> _advancingTimers = new(ScheduledTimer.DueTimeComparer);
	private readonly ITimeProvider _inner;
	private bool _isAdvancing;
#if NET9_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif
	private long _nextSequence;
	private readonly SortedSet<ScheduledTimer> _timers = new(ScheduledTimer.DueTimeComparer);

	public SchedulingTimeProvider(ITimeProvider inner)
	{
		_inner = inner;
	}

	/// <summary>
	///     Whether the timers that advance the clock are moving it right now.
	/// </summary>
	internal bool IsAdvancing
	{
		get
		{
			lock (_lock)
			{
				return _isAdvancing;
			}
		}
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
	///     Schedules the <paramref name="timer" />, unless the <paramref name="cancellationToken" /> is canceled or its due
	///     time lies beyond the end of the clock. A timer that is already due fires on the thread pool.
	/// </summary>
	/// <returns><see langword="true" />, when the <paramref name="timer" /> is scheduled.</returns>
	public bool Schedule(ScheduledTimer timer, CancellationToken cancellationToken)
	{
		long now = _inner.ElapsedTicks;
		if (timer.DueTicks > AddTicks(now, DateTime.MaxValue.Ticks - _inner.Read().Ticks))
		{
			return false;
		}

		bool startAdvancing = false;
		lock (_lock)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return false;
			}

			timer.Sequence = _nextSequence++;
			timer.IsScheduled = true;
			_timers.Add(timer);
			if (timer.AdvancesClock)
			{
				_advancingTimers.Add(timer);
				startAdvancing = !_isAdvancing;
				_isAdvancing = true;
			}
		}

		if (startAdvancing)
		{
			_ = Task.Run(AdvanceToTheAdvancingTimers);
		}
		else if (timer.DueTicks <= now)
		{
			_ = Task.Run(FireDue);
		}

		return true;
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
				Remove(timer);
			}
		}
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string? ToString()
		=> _inner.ToString();

	/// <summary>
	///     Moves the clock to the earliest timer that advances the clock, one fired timer at a time, until no such timer
	///     is left.
	/// </summary>
	private void AdvanceToTheAdvancingTimers()
	{
		try
		{
			while (true)
			{
				long targetTicks;
				lock (_lock)
				{
					if (_advancingTimers.Count == 0)
					{
						_isAdvancing = false;
						return;
					}

					targetTicks = _advancingTimers.Min!.DueTicks;
				}

				TakeNext(targetTicks)?.Fire();
			}
		}
		catch (Exception)
		{
			lock (_lock)
			{
				_isAdvancing = false;
			}

			throw;
		}
	}

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

	private void Remove(ScheduledTimer timer)
	{
		timer.IsScheduled = false;
		_timers.Remove(timer);
		_advancingTimers.Remove(timer);
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
	///     Moves the clock to the earliest timer due until the <paramref name="targetTicks" /> and takes it from the
	///     schedule, or moves the clock to the <paramref name="targetTicks" /> and returns <see langword="null" />.
	/// </summary>
	/// <remarks>
	///     The schedule is not locked while the clock moves, as moving it notifies about the time change, whose callbacks
	///     may change timers. The timer stays on the schedule until the clock reached it, so that it is not lost when
	///     moving the clock throws.
	/// </remarks>
	private ScheduledTimer? TakeNext(long targetTicks)
	{
		lock (_advanceLock)
		{
			while (true)
			{
				long dueTicks;
				lock (_lock)
				{
					dueTicks = _timers.Count > 0 ? Math.Min(_timers.Min!.DueTicks, targetTicks) : targetTicks;
				}

				MoveTo(dueTicks);
				lock (_lock)
				{
					if (_timers.Count > 0 && _timers.Min!.DueTicks <= dueTicks)
					{
						ScheduledTimer next = _timers.Min;
						Remove(next);
						return next;
					}
				}

				if (dueTicks == targetTicks)
				{
					return null;
				}
			}
		}
	}

	/// <summary>
	///     A timer on the schedule of the <see cref="SchedulingTimeProvider" />.
	/// </summary>
	internal sealed class ScheduledTimer(long dueTicks, bool advancesClock, Action fire)
	{
		/// <summary>
		///     Orders the timers by their due time, and timers with the same due time by the order of scheduling.
		/// </summary>
		public static readonly IComparer<ScheduledTimer> DueTimeComparer = Comparer<ScheduledTimer>.Create(
			(x, y) => x.DueTicks != y.DueTicks
				? x.DueTicks.CompareTo(y.DueTicks)
				: x.Sequence.CompareTo(y.Sequence));

		private volatile bool _isScheduled;

		/// <summary>
		///     Whether the timer moves the clock to its due time on its own.
		/// </summary>
		public bool AdvancesClock { get; } = advancesClock;

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
