#if FEATURE_PERIODIC_TIMER
using System;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.TimeSystem;

internal sealed class PeriodicTimerMock : IPeriodicTimer
{
	private bool _isDisposed;
	private DateTime _lastTime;
	private readonly MockTimeSystem _timeSystem;

	internal PeriodicTimerMock(MockTimeSystem timeSystem,
		TimeSpan period)
	{
		ThrowIfPeriodIsInvalid(period, nameof(period));

		_timeSystem = timeSystem;
		_lastTime = _timeSystem.DateTime.UtcNow;
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
		if (_isDisposed)
		{
			return false;
		}

		DateTime now = _timeSystem.DateTime.UtcNow;
		DateTime nextTime = _lastTime + Period;
		if (nextTime > now)
		{
			_timeSystem.TimeProvider.AdvanceBy(nextTime - now);
		}

		_lastTime = now;
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
