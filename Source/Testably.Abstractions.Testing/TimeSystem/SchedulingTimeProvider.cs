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
///     <see cref="AdvanceBy(TimeSpan)" /> moves the clock from one due time to the next and runs each callback on the
///     advancing thread before it moves on. When the wrapped provider is advanced directly, the timers that became due
///     are fired afterward on the thread pool, as the clock is already past them.
/// </remarks>
internal sealed class SchedulingTimeProvider : ITimeProvider
{
	[ThreadStatic] private static int _steppingDepth;

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
			_inner.AdvanceBy(interval);
			return;
		}

		AdvanceTo(_inner.ElapsedTicks + interval.Ticks);
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
		=> _inner.SetTo(value);

	#endregion

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
	///     The due time of the earliest scheduled cancellation, or <see langword="null" /> when there is none.
	/// </summary>
	public long? NextCancellationDueTicks()
	{
		lock (_lock)
		{
			long? next = null;
			foreach (ScheduledTimer timer in _timers)
			{
				if (timer.IsCancellation && (next is null || timer.DueTicks < next))
				{
					next = timer.DueTicks;
				}
			}

			return next;
		}
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
		=> AdvanceTo(_inner.ElapsedTicks);

	/// <summary>
	///     Takes the earliest timer due until the <paramref name="targetTicks" /> from the schedule and moves the clock to
	///     its due time, or moves the clock to the <paramref name="targetTicks" /> and returns <see langword="null" />.
	/// </summary>
	private ScheduledTimer? TakeNext(long targetTicks)
	{
		lock (_lock)
		{
			ScheduledTimer? next = null;
			foreach (ScheduledTimer timer in _timers)
			{
				if (timer.DueTicks <= targetTicks && (next is null || timer.DueTicks < next.DueTicks ||
				                                      (timer.DueTicks == next.DueTicks && timer.Sequence < next.Sequence)))
				{
					next = timer;
				}
			}

			if (next is null)
			{
				MoveTo(targetTicks);
				return null;
			}

			next.IsScheduled = false;
			_timers.Remove(next);
			MoveTo(next.DueTicks);
			return next;
		}
	}

	private void MoveTo(long ticks)
	{
		long now = _inner.ElapsedTicks;
		if (ticks <= now)
		{
			return;
		}

		_steppingDepth++;
		try
		{
			_inner.AdvanceBy(TimeSpan.FromTicks(ticks - now));
		}
		finally
		{
			_steppingDepth--;
		}
	}

	/// <summary>
	///     A timer on the schedule of the <see cref="SchedulingTimeProvider" />.
	/// </summary>
	internal sealed class ScheduledTimer(long dueTicks, Action fire, bool isCancellation)
	{
		private volatile bool _isScheduled;

		/// <summary>
		///     The <see cref="ElapsedTicks" /> at which the timer is due.
		/// </summary>
		public long DueTicks { get; } = dueTicks;

		/// <summary>
		///     Whether the timer canceled its <see cref="CancellationTokenSource" /> when it fired, which it does not, when
		///     the source was already disposed.
		/// </summary>
		public bool HasCanceled { get; set; }

		/// <summary>
		///     Whether the timer only cancels a <see cref="CancellationTokenSource" />, so that it never moves the clock.
		/// </summary>
		public bool IsCancellation { get; } = isCancellation;

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
