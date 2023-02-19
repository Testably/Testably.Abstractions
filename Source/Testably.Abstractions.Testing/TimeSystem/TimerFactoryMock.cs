using System;
using System.IO;
using System.Threading;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.TimeSystem;

internal sealed class TimerFactoryMock : ITimerFactory
{
	private readonly NotificationHandler _callbackHandler;
	private readonly MockTimeSystem _mockTimeSystem;
	private int _activeCount;

	internal TimerFactoryMock(MockTimeSystem timeSystem,
		NotificationHandler callbackHandler)
	{
		_mockTimeSystem = timeSystem;
		_callbackHandler = callbackHandler;
	}

	#region ITimerFactory Members

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem
		=> _mockTimeSystem;

	#endregion

#if FEATURE_TIMER_COUNT
	/// <inheritdoc cref="ITimerFactory.ActiveCount" />
	public long ActiveCount => _activeCount;
#endif

	/// <inheritdoc cref="ITimerFactory.New(TimerCallback)" />
	public ITimer New(TimerCallback callback)
	{
		Interlocked.Increment(ref _activeCount);
		return new TimerMock(_mockTimeSystem, _callbackHandler, callback, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan, DecrementActiveCount);
	}

	/// <inheritdoc cref="ITimerFactory.New(TimerCallback, object?, int, int)" />
	public ITimer New(TimerCallback callback, object? state, int dueTime, int period)
	{
		Interlocked.Increment(ref _activeCount);
		return new TimerMock(_mockTimeSystem, _callbackHandler, callback, state, TimeSpan.FromMilliseconds(dueTime), TimeSpan.FromMilliseconds(period), DecrementActiveCount);
	}

	/// <inheritdoc cref="ITimerFactory.New(TimerCallback, object?, long, long)" />
	public ITimer New(TimerCallback callback, object? state, long dueTime, long period)
	{
		Interlocked.Increment(ref _activeCount);
		return new TimerMock(_mockTimeSystem, _callbackHandler, callback, state, TimeSpan.FromMilliseconds(dueTime), TimeSpan.FromMilliseconds(period), DecrementActiveCount);
	}

	/// <inheritdoc cref="ITimerFactory.New(TimerCallback, object?, TimeSpan, TimeSpan)" />
	public ITimer New(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
	{
		Interlocked.Increment(ref _activeCount);
		return new TimerMock(_mockTimeSystem, _callbackHandler, callback, state, dueTime, period, DecrementActiveCount);
	}

	/// <inheritdoc cref="IFileStreamFactory.Wrap(FileStream)" />
	public ITimer Wrap(Timer timer)
		=> throw ExceptionFactory.NotSupportedTimerWrapping();

	private void DecrementActiveCount()
	{
		Interlocked.Decrement(ref _activeCount);
	}
}
