using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.TimeSystem;
using ITimer = Testably.Abstractions.TimeSystem.ITimer;

namespace Testably.Abstractions.Testing.TimeSystem;

internal sealed class TimerFactoryMock : ITimerFactory, ITimerHandler
{
	private readonly MockTimeSystem _mockTimeSystem;
	private ITimerStrategy _timerStrategy;
	private readonly ConcurrentDictionary<int, TimerMock> _timers = new();
	private int _nextIndex = -1;

	internal TimerFactoryMock(MockTimeSystem timeSystem)
	{
		_mockTimeSystem = timeSystem;
		_timerStrategy = TimerStrategy.Default;
	}

	#region ITimerFactory Members

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem
		=> _mockTimeSystem;

	#endregion

#if FEATURE_TIMER_COUNT
	/// <inheritdoc cref="ITimerFactory.ActiveCount" />
	public long ActiveCount => _timers.Count;
#endif

	/// <inheritdoc cref="ITimerFactory.New(TimerCallback)" />
	public ITimer New(TimerCallback callback)
	{
		TimerMock timerMock = new(_mockTimeSystem, _timerStrategy,
			callback, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
		return RegisterTimerMock(timerMock);
	}

	/// <inheritdoc cref="ITimerFactory.New(TimerCallback, object?, int, int)" />
	public ITimer New(TimerCallback callback, object? state, int dueTime, int period)
	{
		TimerMock timerMock = new(_mockTimeSystem, _timerStrategy,
			callback, state, TimeSpan.FromMilliseconds(dueTime), TimeSpan.FromMilliseconds(period));
		return RegisterTimerMock(timerMock);
	}

	/// <inheritdoc cref="ITimerFactory.New(TimerCallback, object?, long, long)" />
	public ITimer New(TimerCallback callback, object? state, long dueTime, long period)
	{
		TimerMock timerMock = new(_mockTimeSystem, _timerStrategy,
			callback, state, TimeSpan.FromMilliseconds(dueTime), TimeSpan.FromMilliseconds(period));
		return RegisterTimerMock(timerMock);
	}

	/// <inheritdoc cref="ITimerFactory.New(TimerCallback, object?, TimeSpan, TimeSpan)" />
	public ITimer New(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
	{
		TimerMock timerMock = new(_mockTimeSystem, _timerStrategy,
			callback, state, dueTime, period);
		return RegisterTimerMock(timerMock);
	}

	/// <inheritdoc cref="IFileStreamFactory.Wrap(FileStream)" />
	public ITimer Wrap(Timer timer)
		=> throw ExceptionFactory.NotSupportedTimerWrapping();

	private TimerMock RegisterTimerMock(TimerMock timerMock)
	{
		int index = Interlocked.Increment(ref _nextIndex);
		if (_timers.TryAdd(index, timerMock))
		{
			timerMock.RegisterOnDispose(() => _timers.TryRemove(index, out _));
		}

		return timerMock;
	}

	internal ITimerHandler SetTimerStrategy(ITimerStrategy timerStrategy)
	{
		_timerStrategy = timerStrategy;
		return this;
	}

	/// <inheritdoc cref="ITimerHandler.this[int]" />
	public ITimerMock this[int index]
		=> _timers[index];
}
