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
	private WaitHandshake? _waitHandshake;

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

		WaitHandshake handshake = new(executionCount);
		Volatile.Write(ref _waitHandshake, handshake);
		if (_timerStrategy.Mode != TimerMode.StartImmediately)
		{
			Start();
		}

		try
		{
			if (!handshake.Reached.Task.Wait(timeout))
			{
				throw ExceptionFactory.TimerWaitTimeoutException(executionCount, timeout);
			}

			if (_exception != null)
			{
				throw _exception;
			}

			callback?.Invoke(this);
		}
		finally
		{
			Interlocked.CompareExchange(ref _waitHandshake, null, handshake);
			handshake.Continue.TrySetResult(true);
		}

		return this;
	}

	#endregion

	internal void RegisterOnDispose(Action? onDispose)
	{
		_onDispose = onDispose;
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

		if (Volatile.Read(ref _waitHandshake) is { } handshake && handshake.Signal())
		{
			try
			{
				handshake.Continue.Task.Wait(cancellationToken);
			}
			catch (OperationCanceledException)
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
		scheduled = new SchedulingTimeProvider.ScheduledTimer(dueTicks, _autoAdvance,
			() => Execute(scheduled!, cancellationToken));
		lock (_lock)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return false;
			}

			_scheduled = scheduled;
		}

		return _mockTimeSystem.Scheduler.Schedule(scheduled, cancellationToken);
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

	/// <summary>
	///     Lets a <see cref="Wait(int, int, Action{ITimerMock})" /> run its callback while the timer is blocked after the
	///     expected execution. Each wait has its own handshake, so that a late or canceled execution cannot release a later
	///     wait.
	/// </summary>
	private sealed class WaitHandshake(int executionCount)
	{
		private int _remaining = executionCount;

		/// <summary>
		///     Completed by the wait, so that the blocked timer continues.
		/// </summary>
		public TaskCompletionSource<bool> Continue { get; } =
			new(TaskCreationOptions.RunContinuationsAsynchronously);

		/// <summary>
		///     Completed by the timer at the expected execution.
		/// </summary>
		public TaskCompletionSource<bool> Reached { get; } =
			new(TaskCreationOptions.RunContinuationsAsynchronously);

		/// <summary>
		///     Counts an execution and returns <see langword="true" /> for the expected one, after which the timer has to
		///     wait for <see cref="Continue" />.
		/// </summary>
		public bool Signal()
		{
			if (Interlocked.Decrement(ref _remaining) != 0)
			{
				return false;
			}

			Reached.TrySetResult(true);
			return true;
		}
	}
}
