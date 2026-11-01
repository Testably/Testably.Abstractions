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
///     share one thread. Like a real timer, the callback runs on another thread. The thread that fires it waits until
///     the callback completes, so that the clock stands still while it runs. A callback that waits for the clock
///     releases that thread, and a callback that blocks on anything else holds it for at most
///     <see cref="BlockedCallbackLimit" />, as it may wait for the thread that moves the clock.
///     The built-in <see cref="TimeProviderMock" /> moves its clock through the scheduler. When any other provider is
///     moved directly, the timers that became due are fired afterward in the background, as the clock is already past
///     them.
/// </remarks>
internal sealed class TimerScheduler
{
	/// <summary>
	///     How long the thread that fired a callback waits for it, when the callback blocks on something else than the
	///     clock, e.g. on a lock held by the code that moves the clock.
	/// </summary>
	private static readonly TimeSpan BlockedCallbackLimit = TimeSpan.FromSeconds(1);

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
	///     time lies beyond the end of the clock. A timer that is already due fires in the background.
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
	///     Lets a callback that waits for the clock release the thread that fired it, so that the clock can move on.
	/// </summary>
	/// <remarks>
	///     Only the waits of the <see cref="MockTimeSystem" /> that end when the clock moves release it. Waiting in the
	///     handshake of <see cref="ITimerMock.Wait(int, int, Action{ITimerMock})" /> does not, so that the clock stands still
	///     while its callback runs.
	/// </remarks>
	public static void ReleaseClock()
		=> FiringCallback.Current?.Release();

	/// <summary>
	///     Runs the callback of the <paramref name="timer" /> on a <see cref="CallbackThread" /> and waits until it
	///     completes, until it waits for the clock (<see cref="ReleaseClock" />), or until it blocked for the
	///     <see cref="BlockedCallbackLimit" />.
	/// </summary>
	/// <remarks>
	///     The callback does not run on the thread pool, as the thread that waits for it often is a pool thread itself,
	///     so that waiting callbacks would starve each other of threads. The limit only counts from the start of the
	///     callback.
	/// </remarks>
	private static void Fire(ScheduledTimer timer)
	{
		FiringCallback firing = new();
		TaskCompletionSource<bool> completed = new(TaskCreationOptions.RunContinuationsAsynchronously);
		CallbackThread.Run(() =>
		{
			FiringCallback.Current = firing;
			firing.Started.TrySetResult(true);
			try
			{
				timer.Fire();
			}
			finally
			{
				FiringCallback.Current = null;
				completed.TrySetResult(true);
			}
		});

		firing.Started.Task.Wait();
		Task.WaitAny([completed.Task, firing.Released.Task,], BlockedCallbackLimit);
	}

	/// <summary>
	///     A dedicated thread for timer callbacks, which is reused for the next callback once it is idle, so that a
	///     callback neither waits for the thread pool nor pays for creating a thread.
	/// </summary>
	/// <remarks>
	///     A new thread only starts when all callback threads are busy, e.g. blocked in a callback. The most recently
	///     idle thread is reused first, so that the others stay idle and end after the <see cref="IdleLimit" />.
	/// </remarks>
	private sealed class CallbackThread
	{
		private static readonly TimeSpan IdleLimit = TimeSpan.FromSeconds(10);
		private static readonly LinkedList<CallbackThread> Idle = new();
		private readonly LinkedListNode<CallbackThread> _idleNode;
		private readonly SemaphoreSlim _signal = new(0);
		private Action? _work;

		private CallbackThread()
		{
			_idleNode = new LinkedListNode<CallbackThread>(this);
		}

		/// <summary>
		///     Runs the <paramref name="work" /> on an idle callback thread, or on a new one, when all are busy.
		/// </summary>
		/// <remarks>
		///     A new thread does not capture the execution context of the code that starts it, as it outlives that code;
		///     the timer runs its callback with the execution context of its creator.
		/// </remarks>
		public static void Run(Action work)
		{
			CallbackThread? callbackThread = null;
			lock (Idle)
			{
				if (Idle.Last is { } node)
				{
					Idle.Remove(node);
					callbackThread = node.Value;
				}
			}

			if (callbackThread is null)
			{
				callbackThread = new CallbackThread();
				Thread thread = new(callbackThread.Loop) { IsBackground = true, Name = "Testably timer callback", };
				if (ExecutionContext.IsFlowSuppressed())
				{
					thread.Start();
				}
				else
				{
					using AsyncFlowControl _ = ExecutionContext.SuppressFlow();
					thread.Start();
				}
			}

			callbackThread._work = work;
			callbackThread._signal.Release();
		}

		private void Loop()
		{
			while (true)
			{
				if (!_signal.Wait(IdleLimit))
				{
					lock (Idle)
					{
						if (_idleNode.List is not null)
						{
							Idle.Remove(_idleNode);
							_signal.Dispose();
							return;
						}
					}

					continue;
				}

				Action work = _work!;
				_work = null;
				try
				{
					work();
				}
				catch (Exception)
				{
					// The timer reports the exception of its callback; nothing else must end the shared thread.
				}

				lock (Idle)
				{
					Idle.AddLast(_idleNode);
				}
			}
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
	///     A timer callback that runs on its own thread, while the thread that fired it waits.
	/// </summary>
	private sealed class FiringCallback
	{
		/// <summary>
		///     The callback that runs on the current thread.
		/// </summary>
		[ThreadStatic] public static FiringCallback? Current;

		/// <summary>
		///     Completed when the callback waits for the clock.
		/// </summary>
		public TaskCompletionSource<bool> Released { get; } =
			new(TaskCreationOptions.RunContinuationsAsynchronously);

		/// <summary>
		///     Completed when the callback got a thread and starts.
		/// </summary>
		public TaskCompletionSource<bool> Started { get; } =
			new(TaskCreationOptions.RunContinuationsAsynchronously);

		public void Release() => Released.TrySetResult(true);
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
