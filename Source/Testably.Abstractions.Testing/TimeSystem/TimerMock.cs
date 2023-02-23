﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.TimeSystem;

internal sealed class TimerMock : ITimerMock
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
	private CountdownEvent? _countdownEvent;
	private readonly ManualResetEventSlim _continueEvent = new();

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
		Task.Run(
				() => RunTimer(token),
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
			}, TaskScheduler.Default);
	}

	internal void RegisterOnDispose(Action? onDispose)
	{
		_onDispose = onDispose;
	}

	private async Task RunTimer(CancellationToken cancellationToken = default)
	{
		await TryDelay(_mockTimeSystem.Task, _dueTime, cancellationToken);
		DateTime nextPlannedExecution = _mockTimeSystem.DateTime.UtcNow;
		while (!cancellationToken.IsCancellationRequested)
		{
			nextPlannedExecution += _period;
			_callback(_state);
			_callbackHandler.InvokeTimerExecutedCallbacks(
				new TimerExecution(_mockTimeSystem.DateTime.UtcNow, this));
			if (_countdownEvent?.Signal() == true)
			{
				_continueEvent.Wait(cancellationToken);
				_continueEvent.Reset();
			}

			if (_period.TotalMilliseconds <= 0)
			{
				return;
			}

			TimeSpan delay = nextPlannedExecution - _mockTimeSystem.DateTime.UtcNow;
			if (delay < TimeSpan.Zero)
			{
				delay = TimeSpan.Zero;
			}

			await TryDelay(_mockTimeSystem.Task, delay, cancellationToken);
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
	private static async Task TryDelay(ITask taskSystem, TimeSpan delay,
		CancellationToken cancellationToken = default)
	{
		try
		{
			if (delay.TotalMilliseconds < 0)
			{
				await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
			}

			await taskSystem.Delay(delay, cancellationToken);
		}
		catch (Exception)
		{
			// Ignore all exceptions:
			// https://stackoverflow.com/a/39885850
		}
	}

	#region ITimerMock Members

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem
		=> _mockTimeSystem;

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
				throw new NotSupportedException("The wait handle is not of any supported type!");
		}
		return true;
	}

	/// <inheritdoc cref="ITimerMock.Wait(int, int, Action{ITimerMock})" />
	public ITimerMock Wait(int executionCount = 1, int timeout = 10000, Action<ITimerMock>? callback = null)
	{
		if (_timerStrategy.Mode != TimerMode.StartImmediately)
		{
			Start();
		}

		_countdownEvent = new CountdownEvent(executionCount);
		if (!_countdownEvent.Wait(timeout))
		{
			throw new TimeoutException(
				$"The execution count {executionCount} was not reached in {timeout}ms.");
		}

		callback?.Invoke(this);
		_continueEvent.Set();

		return this;
	}

	#endregion
}
