#if FEATURE_PERIODIC_TIMER
using System;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.TimeSystem;

internal sealed class PeriodicTimerMock : IPeriodicTimer
{
	private readonly bool _autoAdvance;
	private bool _isDisposed;
	private long _lastTime;
	private readonly MockTimeSystem _timeSystem;
	private readonly NotificationHandler _callbackHandler;

	internal PeriodicTimerMock(
		MockTimeSystem timeSystem,
		NotificationHandler callbackHandler,
		TimeSpan period,
		bool autoAdvance)
	{
		ThrowIfPeriodIsInvalid(period, nameof(period));

		_timeSystem = timeSystem;
		_callbackHandler = callbackHandler;
		_autoAdvance = autoAdvance;
		_lastTime = _timeSystem.TimeProvider.ElapsedTicks;
		Period = period;
	}

	#region IPeriodicTimer Members

	/// <inheritdoc cref="IPeriodicTimer.Period" />
	public TimeSpan Period
	{
		get;
		set
		{
			ThrowIfPeriodIsInvalid(value, nameof(value));
			field = value;
		}
	}

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem => _timeSystem;

	/// <inheritdoc cref="IDisposable.Dispose()" />
	public void Dispose()
		=> _isDisposed = true;

	/// <inheritdoc cref="IPeriodicTimer.WaitForNextTickAsync(CancellationToken)" />
	public async ValueTask<bool> WaitForNextTickAsync(CancellationToken cancellationToken = new())
	{
		if (cancellationToken.IsCancellationRequested)
		{
			throw ExceptionFactory.TaskWasCanceled();
		}

		if (_isDisposed)
		{
			return false;
		}

		_callbackHandler.InvokePeriodicTimerWaitingForNextTick(this);
		long now = _timeSystem.TimeProvider.ElapsedTicks;
		long nextTime = _lastTime + Period.Ticks;
		if (nextTime > now)
		{
			if (_autoAdvance)
			{
				_timeSystem.TimeProvider.AdvanceBy(TimeSpan.FromTicks(nextTime - now));
				_lastTime = nextTime;
			}
			else
			{
				using IAwaitableCallback<DateTime> onTimeChanged = _timeSystem.On
					.TimeChanged(predicate: _ => _timeSystem.TimeProvider.ElapsedTicks >= nextTime);
				await onTimeChanged.WaitAsync(
					timeout: Timeout.InfiniteTimeSpan,
					cancellationToken: cancellationToken).ConfigureAwait(false);
				_lastTime = _timeSystem.TimeProvider.ElapsedTicks;
			}
		}
		else
		{
			_lastTime = now;
		}

		await Task.Yield();
		return true;
	}

	#endregion

	private static void ThrowIfPeriodIsInvalid(TimeSpan period, string paramName)
	{
		if (period.TotalMilliseconds < 1 && period != Timeout.InfiniteTimeSpan)
		{
			throw new ArgumentOutOfRangeException(paramName);
		}
	}
}
#endif
