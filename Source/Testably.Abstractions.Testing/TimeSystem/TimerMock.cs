using System;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.TimeSystem;

internal sealed class TimerMock : ITimer, ITimerMock
{
	private readonly NotificationHandler _callbackHandler;
	private readonly ITimerStrategy _timerStrategy;
	private readonly TimerCallback _callback;
	private readonly object? _state;
	private TimeSpan _dueTime;
	private TimeSpan _period;
	private Action? _onDispose;
	private readonly MockTimeSystem _mockTimeSystem;
	private bool _isDisposed;
	private readonly object _lock = new();
	private CancellationTokenSource? _cancellationTokenSource;

	internal TimerMock(MockTimeSystem timeSystem,
		NotificationHandler callbackHandler,
		ITimerStrategy timerStrategy,
		TimerCallback callback,
		object? state,
		TimeSpan dueTime,
		TimeSpan period)
	{
		if (dueTime.TotalMilliseconds < -1)
		{
			throw ExceptionFactory.TimerArgumentOutOfRange(nameof(dueTime));
		}

		if (period.TotalMilliseconds < -1)
		{
			throw new ArgumentOutOfRangeException(nameof(period));
		}

		_mockTimeSystem = timeSystem;
		_callbackHandler = callbackHandler;
		_timerStrategy = timerStrategy;
		_callback = callback;
		_state = state;
		_dueTime = dueTime;
		_period = period;
		if (_timerStrategy.Mode == TimerMode.StartImmediately)
		{
			Start();
		}
	}

	private void Stop()
	{
		lock (_lock)
		{
			if (_cancellationTokenSource is { IsCancellationRequested: false })
			{
				_cancellationTokenSource?.Cancel();
			}
		}
	}

	private void Start()
	{
		Stop();
		CancellationTokenSource runningCancellationTokenSource;
		lock (_lock)
		{
			_cancellationTokenSource = new CancellationTokenSource();
			runningCancellationTokenSource = _cancellationTokenSource;
		}

		CancellationToken token = runningCancellationTokenSource.Token;
		Task.Factory.StartNew(
				() => RunTimer(token),
				cancellationToken: token,
				TaskCreationOptions.LongRunning, TaskScheduler.Default)
			.ContinueWith(_ =>
			{
				runningCancellationTokenSource.Dispose();
				lock (_lock)
				{
					if (_cancellationTokenSource == runningCancellationTokenSource)
					{
						_cancellationTokenSource.Dispose();
						_cancellationTokenSource = null;
					}
				}
			}, TaskScheduler.Default);
	}

	internal void OnDispose(Action? onDispose)
	{
		_onDispose = onDispose;
	}

	private void RunTimer(CancellationToken cancellationToken = default)
	{
		TryDelay(_mockTimeSystem.Task, _dueTime, cancellationToken);
		DateTime nextPlannedExecution = _mockTimeSystem.DateTime.UtcNow;
		while (!cancellationToken.IsCancellationRequested)
		{
			nextPlannedExecution += _period;
			_callback(_state);

			if (_period.TotalMilliseconds <= 0)
			{
				return;
			}

			TimeSpan delay = nextPlannedExecution - _mockTimeSystem.DateTime.UtcNow;
			if (delay < TimeSpan.Zero)
			{
				delay = TimeSpan.Zero;
			}

			TryDelay(_mockTimeSystem.Task, delay, cancellationToken);
		}
	}

	/// <summary>
	///     Tries to delay executing by the specified <paramref name="delay" />.
	/// </summary>
	/// <param name="taskSystem">The time system.</param>
	/// <param name="delay">The delay interval.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>
	///     <c>True</c> if the Task delayed for the complete <paramref name="delay" />,
	///     <c>False</c> if the delay
	///     was aborted via the <paramref name="cancellationToken" />.
	/// </returns>
	private static void TryDelay(ITask taskSystem, TimeSpan delay,
		CancellationToken cancellationToken = default)
	{
		try
		{
			if (delay.TotalMilliseconds < 0)
			{
				Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken)
					.Wait(cancellationToken);
			}

			taskSystem.Delay(delay, cancellationToken).Wait(cancellationToken);
		}
		catch (Exception)
		{
			// Ignore all exceptions:
			// https://stackoverflow.com/a/39885850
		}
	}

	#region ITimer Members

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem
		=> _mockTimeSystem;

	#endregion

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
			throw new ObjectDisposedException("Cannot access a disposed object.");
		}

		if (dueTime.TotalMilliseconds < -1)
		{
			throw ExceptionFactory.TimerArgumentOutOfRange(nameof(dueTime));
		}

		if (period.TotalMilliseconds < -1)
		{
			throw new ArgumentOutOfRangeException(nameof(period));
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

	/// <inheritdoc cref="ITimerMock.Wait(int, int)" />
	public void Wait(int executionCount = 1, int timeout = 10000)
	{
		if (_timerStrategy.Mode != TimerMode.StartImmediately)
		{
			Start();
		}
	}
}
