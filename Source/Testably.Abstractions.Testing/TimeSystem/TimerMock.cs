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
	private bool _isDisposed;
	private readonly object _lock = new();
	private readonly MockTimeSystem _mockTimeSystem;
	private Action? _onDispose;
	private TimeSpan _period;
	private readonly object? _state;
	private readonly ITimerStrategy _timerStrategy;

	internal TimerMock(MockTimeSystem timeSystem,
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

	#region ITimerMock Members

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem
		=> _mockTimeSystem;

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
			throw new ObjectDisposedException(nameof(Change), "Cannot access a disposed object.");
#endif
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

		if (_countdownEvent?.Wait(timeout) == false)
		{
			throw ExceptionFactory.TimerWaitTimeoutException(executionCount, timeout);
		}

		if (_exception != null)
		{
			throw _exception;
		}

		callback?.Invoke(this);
		_continueEvent.Set();

		return this;
	}

	#endregion

	internal void RegisterOnDispose(Action? onDispose)
	{
		_onDispose = onDispose;
	}

	private async Task RunTimer(CancellationToken cancellationToken = default)
	{
		await _mockTimeSystem.Task.Delay(_dueTime, cancellationToken).ConfigureAwait(false);
		if (_dueTime.TotalMilliseconds < 0)
		{
			cancellationToken.WaitHandle.WaitOne(_dueTime);
		}

		DateTime nextPlannedExecution = _mockTimeSystem.DateTime.UtcNow;
		while (!cancellationToken.IsCancellationRequested)
		{
			nextPlannedExecution += _period;
			try
			{
				_callback(_state);
			}
			catch (Exception swallowedException)
			{
				_exception = swallowedException;
			}

			if (_countdownEvent?.Signal() == true)
			{
				_continueEvent.Wait(cancellationToken);
				_continueEvent.Reset();
			}

			if (_exception != null && !_timerStrategy.SwallowExceptions)
			{
				break;
			}

			if (_period.TotalMilliseconds <= 0 ||
			    cancellationToken.IsCancellationRequested)
			{
				return;
			}

			TimeSpan delay = nextPlannedExecution - _mockTimeSystem.DateTime.UtcNow;
			if (delay > TimeSpan.Zero)
			{
				await _mockTimeSystem.Task.Delay(delay, cancellationToken).ConfigureAwait(false);
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
		using ManualResetEventSlim startCreateTimerThreads = new();
		_ = Task.Run(
				async () =>
				{
					// ReSharper disable once AccessToDisposedClosure
					try
					{
						startCreateTimerThreads.Set();
					}
					catch (ObjectDisposedException)
					{
						// Ignore any ObjectDisposedException
					}

					await RunTimer(token).ConfigureAwait(false);
				},
				cancellationToken: token)
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
			}, TaskScheduler.Current);
		startCreateTimerThreads.Wait(token);
	}

	private void Stop()
	{
		try
		{
			lock (_lock)
			{
				if (_cancellationTokenSource is { IsCancellationRequested: false })
				{
					_cancellationTokenSource?.Cancel();
				}
			}
		}
		catch (ObjectDisposedException)
		{
			// Ignore if the cancellationTokenSource is already disposed.
		}
	}
}
