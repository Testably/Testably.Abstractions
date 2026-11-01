using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Testing.TimeSystem;

/// <summary>
///     Schedules the timers of the <see cref="MockTimeSystem" /> and moves its <see cref="ITimeProvider" /> from one due
///     time to the next, so that the timers fire in the order of their due time, with the clock at the due time of each
///     timer while its callback runs.
/// </summary>
/// <remarks>
///     Taking the next timer and moving the clock to its due time is one step, but the callback runs after it, so that a
///     callback may wait for another thread that moves the clock. Such a thread may move the clock past the due time of
///     a running callback, so the order holds for the timers that one thread fires. All timers that advance the clock
///     share one thread. Like a real timer, the callback runs on the thread pool. The thread that fires it waits until
///     the callback completes or blocks, as a blocked callback may wait for the clock or for the thread that moves it.
///     The built-in <see cref="TimeProviderMock" /> moves its clock through the scheduler. When any other provider is
///     moved directly, the timers that became due are fired afterward on the thread pool, as the clock is already past
///     them.
/// </remarks>
internal sealed class TimerScheduler
{
	/// <summary>
	///     The scheduler that moves the clock on the current thread and did not get notified about the change yet.
	/// </summary>
	[ThreadStatic] private static TimerScheduler? _movingClock;

#if NET9_0_OR_GREATER
	private readonly Lock _advanceLock = new();
#else
	private readonly object _advanceLock = new();
#endif
	private readonly SortedSet<ScheduledTimer> _advancingTimers = new(ScheduledTimer.DueTimeComparer);
	private readonly ITimeProvider _clock;
	private bool _isAdvancing;
	private int _isFiringDuePending;
#if NET9_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif
	private long _nextSequence;
	private readonly SortedSet<ScheduledTimer> _timers = new(ScheduledTimer.DueTimeComparer);

	public TimerScheduler(ITimeProvider clock)
	{
		_clock = clock;
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

	/// <summary>
	///     Whether this scheduler moves the clock on the current thread, so that the clock must change without it.
	/// </summary>
	internal bool IsMovingClock
		=> _movingClock == this;

	/// <summary>
	///     Moves the clock by the <paramref name="interval" />. The timers that are due within it fire in the order of
	///     their due time, while the clock is at their due time, and the calling thread waits for each callback until it
	///     completes or blocks.
	/// </summary>
	public void AdvanceBy(TimeSpan interval)
	{
		if (interval < TimeSpan.Zero)
		{
			lock (_advanceLock)
			{
				Step(() => _clock.AdvanceBy(interval));
			}

			return;
		}

		AdvanceBy(interval, null);
	}

	/// <summary>
	///     Sets the clock to the <paramref name="value" />, which does not change the elapsed ticks the timers depend on.
	/// </summary>
	public void SetTo(DateTime value)
	{
		lock (_advanceLock)
		{
			Step(() => _clock.SetTo(value));
		}
	}

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
		=> AdvanceTo(AddTicks(_clock.ElapsedTicks, interval.Ticks), stopAfter);

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
			Fire(next);
			if (stopAfter?.Invoke(next) == true)
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	///     Fires the timers that became due while the clock was moved without the scheduler.
	/// </summary>
	/// <remarks>
	///     The first notification while the scheduler moves the clock is about its own change. Any later one comes from a
	///     time change callback that moves the clock again, which therefore has to go through the scheduler.
	/// </remarks>
	public void OnTimeChanged()
	{
		if (IsMovingClock)
		{
			_movingClock = null;
			return;
		}

		StartFiringDue();
	}

	/// <summary>
	///     Schedules the <paramref name="timer" />, unless the <paramref name="cancellationToken" /> is canceled or its due
	///     time lies beyond the end of the clock. A timer that is already due fires on the thread pool.
	/// </summary>
	/// <returns><see langword="true" />, when the <paramref name="timer" /> is scheduled.</returns>
	public bool Schedule(ScheduledTimer timer, CancellationToken cancellationToken)
	{
		long now = _clock.ElapsedTicks;
		if (timer.DueTicks > AddTicks(now, DateTime.MaxValue.Ticks - _clock.Read().Ticks))
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
			StartOnDedicatedThread(AdvanceToTheAdvancingTimers);
		}
		else if (timer.DueTicks <= _clock.ElapsedTicks)
		{
			StartFiringDue();
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

	/// <summary>
	///     Moves the clock to the earliest timer that advances the clock, one fired timer at a time, until no such timer
	///     is left.
	/// </summary>
	/// <remarks>
	///     An exception that a time change callback throws while the clock moves has no caller to report to, so it is
	///     swallowed, unless the clock stopped moving, which would otherwise repeat it forever.
	/// </remarks>
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

				long ticksBefore = _clock.ElapsedTicks;
				try
				{
					if (TakeNext(targetTicks) is { } next)
					{
						Fire(next);
					}
				}
				catch (Exception) when (_clock.ElapsedTicks != ticksBefore)
				{
					// The timer stays scheduled and fires in the next iteration, as the clock already reached it.
				}
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

	/// <summary>
	///     Runs the callback of the <paramref name="timer" /> on the thread pool and waits until it completes, or until its
	///     thread blocks, e.g. on another timer or on a lock held by the thread that moves the clock.
	/// </summary>
	/// <remarks>
	///     The thread has to block on two checks in a row, so that briefly waiting for a contended lock does not move the
	///     clock while the callback still runs.
	/// </remarks>
	private static void Fire(ScheduledTimer timer)
	{
		Thread? callbackThread = null;
		Task callback = Task.Run(() =>
		{
			Volatile.Write(ref callbackThread, Thread.CurrentThread);
			timer.Fire();
		});

		int blockedChecks = 0;
		while (!callback.Wait(1) && blockedChecks < 2)
		{
			Thread? thread = Volatile.Read(ref callbackThread);
			blockedChecks = thread is not null && (thread.ThreadState & ThreadState.WaitSleepJoin) != 0
				? blockedChecks + 1
				: 0;
		}
	}

	private void FireDue()
	{
		Volatile.Write(ref _isFiringDuePending, 0);
		AdvanceBy(TimeSpan.Zero, null);
	}

	/// <summary>
	///     Fires the due timers in the background, unless that is already pending, as one run fires all due timers.
	/// </summary>
	private void StartFiringDue()
	{
		if (Interlocked.Exchange(ref _isFiringDuePending, 1) == 0)
		{
			StartOnDedicatedThread(FireDue);
		}
	}

	/// <summary>
	///     Starts the <paramref name="action" />, which waits for timer callbacks, on its own thread instead of the thread
	///     pool, so that it does not starve the callbacks it waits for of pool threads.
	/// </summary>
	private static void StartOnDedicatedThread(Action action)
		=> _ = Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.LongRunning,
			TaskScheduler.Default);

	private void MoveTo(long ticks)
	{
		long now = _clock.ElapsedTicks;
		if (ticks > now)
		{
			Step(() => _clock.AdvanceBy(TimeSpan.FromTicks(ticks - now)));
		}
	}

	private void Remove(ScheduledTimer timer)
	{
		timer.IsScheduled = false;
		_timers.Remove(timer);
		_advancingTimers.Remove(timer);
	}

	private void Step(Action changeTime)
	{
		TimerScheduler? previous = _movingClock;
		_movingClock = this;
		try
		{
			changeTime();
		}
		finally
		{
			_movingClock = previous;
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
	///     A timer on the schedule of the <see cref="TimerScheduler" />.
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
		///     The <see cref="ITimeProvider.ElapsedTicks" /> at which the timer is due.
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
