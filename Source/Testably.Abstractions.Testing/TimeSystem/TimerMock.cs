using System;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.TimeSystem;
using ITimer = Testably.Abstractions.TimeSystem.ITimer;

namespace Testably.Abstractions.Testing.TimeSystem;

internal sealed class TimerMock : ITimerMock
{
	private readonly TimerCallback _callback;
	private CancellationTokenSource? _cancellationTokenSource;
	private readonly ManualResetEventSlim _continueEvent = new();
	private CountdownEvent? _countdownEvent;
	private TimeSpan _dueTime;
	private Exception? _exception;
	private long _executionCount;
	private bool _isDisposed;
#if NET9_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif
	private readonly MockTimeSystem _mockTimeSystem;
	private Action? _onDispose;
	private TimeSpan _period;
	private readonly bool _autoAdvance;
	private SchedulingTimeProvider.ScheduledTimer? _scheduled;
	private readonly object? _state;
	private readonly TimerStrategy _timerStrategy;

	internal TimerMock(MockTimeSystem timeSystem,
		TimerStrategy timerStrategy,
		TimerCallback callback,
		object? state,
		TimeSpan dueTime,
		TimeSpan period,
		bool autoAdvance)
	{
		if (dueTime.TotalMilliseconds < -1)
		{
			throw ExceptionFactory.TimerArgumentOutOfRange(nameof(dueTime),
				(long)dueTime.TotalMilliseconds);
		}

		if (period.TotalMilliseconds < -1)
		{
			throw ExceptionFactory.TimerArgumentOutOfRange(nameof(period),
				(long)period.TotalMilliseconds);
		}

		_mockTimeSystem = timeSystem;
		_timerStrategy = timerStrategy;
		_callback = callback;
		_state = state;
		_dueTime = dueTime;
		_period = period;
		_autoAdvance = autoAdvance;
		if (_timerStrategy.Mode == TimerMode.StartImmediately)
		{
			Start();
		}
	}

	#region ITimerMock Members

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem
		=> _mockTimeSystem;

	/// <inheritdoc cref="ITimerMock.ExecutionCount" />
	public long ExecutionCount
		=> Volatile.Read(ref _executionCount);

	/// <inheritdoc cref="ITimer.Change(int, int)" />
	public bool Change(int dueTime, int period)
		=> Change(TimeSpan.FromMilliseconds(dueTime), TimeSpan.FromMilliseconds(period));

	/// <inheritdoc cref="ITimer.Change(long, long)" />
	public bool Change(long dueTime, long period)
		=> Change(TimeSpan.FromMilliseconds(dueTime), TimeSpan.FromMilliseconds(period));

	/// <inheritdoc cref="ITimer.Change(TimeSpan, TimeSpan)" />
	public bool Change(TimeSpan dueTime, TimeSpan period)
	{
		if (_isDisposed)
		{
#if NET8_0_OR_GREATER
			return false;
#else
			throw ExceptionFactory.ObjectDisposed();
#endif
		}

		if (dueTime.TotalMilliseconds < -1)
		{
			throw ExceptionFactory.TimerArgumentOutOfRange(nameof(dueTime),
				(long)dueTime.TotalMilliseconds);
		}

		if (period.TotalMilliseconds < -1)
		{
			throw ExceptionFactory.TimerArgumentOutOfRange(nameof(period),
				(long)period.TotalMilliseconds);
		}

		try
		{
			Stop();
			_dueTime = dueTime;
			_period = period;
			Start();
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	/// <inheritdoc cref="IDisposable.Dispose()" />
	public void Dispose()
	{
		if (!_isDisposed)
		{
			_isDisposed = true;
			Stop();
			_onDispose?.Invoke();
			lock (_lock)
			{
				_cancellationTokenSource?.Dispose();
				_cancellationTokenSource = null;
				_continueEvent.Dispose();
			}
		}
	}

	/// <inheritdoc cref="ITimer.Dispose(WaitHandle)" />
	public bool Dispose(WaitHandle notifyObject)
	{
		if (_isDisposed)
		{
			return false;
		}

		switch (notifyObject)
		{
#if FEATURE_TIMER_DISPOSE_SIGNALS_ONLY_EVENTS_ON_UNIX
			case Mutex or Semaphore when !OperatingSystem.IsWindows():
				// Since .NET 11, Unix disposes the timer and then fails to signal anything but an event.
				Dispose();
				throw new InvalidOperationException("The handle is invalid.");
#endif
			case Mutex m:
				m.WaitOne();
				Dispose();
				m.ReleaseMutex();
				break;
			case Semaphore s:
				s.WaitOne();
				Dispose();
				s.Release();
				break;
			case EventWaitHandle e:
				Dispose();
				e.Set();
				break;
			default:
				throw new NotSupportedException(
					$"The wait handle '{nameof(notifyObject)}' is of type '{notifyObject.GetType()}' which is not supported!");
		}

		return true;
	}

#if FEATURE_ASYNC_DISPOSABLE
	/// <inheritdoc cref="IAsyncDisposable.DisposeAsync()" />
	public ValueTask DisposeAsync()
	{
		Dispose();
#if NETSTANDARD2_1
		return new ValueTask();
#else
		return ValueTask.CompletedTask;
#endif
	}
#endif

	/// <inheritdoc cref="ITimerMock.Wait(int, int, Action{ITimerMock})" />
	public ITimerMock Wait(
		int executionCount = 1,
		int timeout = 10000,
		Action<ITimerMock>? callback = null)
	{
		if (executionCount <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(executionCount),
				"Execution count must be greater than zero.");
		}

		if (timeout < -1)
		{
			throw new ArgumentOutOfRangeException(nameof(timeout));
		}

		try
		{
			_countdownEvent = new CountdownEvent(executionCount);
		}
		catch (ArgumentOutOfRangeException)
		{
			// In case of an ArgumentOutOfRangeException, the executionCount is already reached.
		}

		if (_timerStrategy.Mode != TimerMode.StartImmediately)
		{
			Start();
		}

		CountdownEvent? countdownEvent = _countdownEvent;
		if (countdownEvent?.Wait(timeout, CancellationToken.None) == false)
		{
			_countdownEvent = null;
			if (countdownEvent.IsSet)
			{
				_continueEvent.Set();
			}

			throw ExceptionFactory.TimerWaitTimeoutException(executionCount, timeout);
		}

		try
		{
			if (_exception != null)
			{
				throw _exception;
			}

			callback?.Invoke(this);
		}
		finally
		{
			_countdownEvent = null;
			_continueEvent.Set();
		}

		return this;
	}

	#endregion

	internal void RegisterOnDispose(Action? onDispose)
	{
		_onDispose = onDispose;
	}

	/// <summary>
	///     Moves the clock to the due time of the timer, firing it and the timers due before it on the way, for as long as
	///     the timer runs.
	/// </summary>
	/// <remarks>
	///     When a timer on the way disposes or changes this timer, the clock stops at that timer.
	/// </remarks>
	private async Task AdvanceClock(CancellationToken cancellationToken)
	{
		SchedulingTimeProvider scheduler = _mockTimeSystem.Scheduler;
		while (!cancellationToken.IsCancellationRequested &&
		       Volatile.Read(ref _scheduled) is { } scheduled)
		{
			if (scheduled.IsScheduled)
			{
				scheduler.AdvanceTo(scheduled.DueTicks,
					fired => fired != scheduled && !scheduled.IsScheduled);
			}
			else
			{
				await WaitForNextRun(cancellationToken).ConfigureAwait(false);
			}
		}
	}

	/// <summary>
	///     Waits while another thread fires the timer, until it is scheduled for its next run or done.
	/// </summary>
	private static async Task WaitForNextRun(CancellationToken cancellationToken)
	{
		try
		{
			await Task.Delay(1, cancellationToken).ConfigureAwait(false);
		}
		catch (OperationCanceledException)
		{
			// The loop of the caller ends with the cancellation.
		}
	}

	private void Execute(SchedulingTimeProvider.ScheduledTimer scheduled, CancellationToken cancellationToken)
	{
		if (!TryExecute(scheduled.DueTicks, cancellationToken))
		{
			lock (_lock)
			{
				if (_scheduled == scheduled)
				{
					_scheduled = null;
				}
			}
		}
	}

	/// <summary>
	///     Runs the callback and schedules the next run, if there is one.
	/// </summary>
	private bool TryExecute(long dueTicks, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return false;
		}

		try
		{
			_callback(_state);
		}
		catch (Exception swallowedException)
		{
			_exception = swallowedException;
		}

		Interlocked.Increment(ref _executionCount);

		if (_countdownEvent?.Signal() == true)
		{
			try
			{
				_continueEvent.Wait(cancellationToken);
				_continueEvent.Reset();
			}
			catch (Exception ex) when (ex is OperationCanceledException or ObjectDisposedException)
			{
				return false;
			}
		}

		if ((_exception != null && !_timerStrategy.SwallowExceptions) ||
		    _period.TotalMilliseconds <= 0)
		{
			return false;
		}

		return Schedule(SchedulingTimeProvider.AddTicks(dueTicks, _period.Ticks), cancellationToken);
	}

	private bool Schedule(long dueTicks, CancellationToken cancellationToken)
	{
		SchedulingTimeProvider.ScheduledTimer? scheduled = null;
		scheduled = new SchedulingTimeProvider.ScheduledTimer(dueTicks,
			() => Execute(scheduled!, cancellationToken));
		lock (_lock)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return false;
			}

			_scheduled = scheduled;
		}

		_mockTimeSystem.Scheduler.Schedule(scheduled);
		return true;
	}

	private void Start()
	{
		CancellationTokenSource cancellationTokenSource = new();
		CancellationToken token = cancellationTokenSource.Token;
		Stop(cancellationTokenSource);
		if (token.IsCancellationRequested || _dueTime.TotalMilliseconds < 0)
		{
			return;
		}

		Schedule(SchedulingTimeProvider.AddTicks(_mockTimeSystem.TimeProvider.ElapsedTicks, _dueTime.Ticks),
			token);
		if (_autoAdvance)
		{
			_ = Task.Run(() => AdvanceClock(token), token);
		}
	}

	/// <summary>
	///     Stops the current run and installs the <paramref name="next" /> run in the same step, so that concurrent
	///     restarts cannot leave a run behind.
	/// </summary>
	private void Stop(CancellationTokenSource? next = null)
	{
		SchedulingTimeProvider.ScheduledTimer? scheduled;
		lock (_lock)
		{
			try
			{
				_cancellationTokenSource?.Cancel();
			}
			catch (ObjectDisposedException)
			{
				// Ignore if the cancellationTokenSource is already disposed.
			}

			_cancellationTokenSource?.Dispose();
			_cancellationTokenSource = next;
			if (_isDisposed && next is not null)
			{
				next.Cancel();
				next.Dispose();
				_cancellationTokenSource = null;
			}

			scheduled = _scheduled;
			_scheduled = null;
		}

		if (scheduled is not null)
		{
			_mockTimeSystem.Scheduler.Unschedule(scheduled);
		}
	}
}
