using System;
using System.IO;
using System.Threading;

namespace Testably.Abstractions.TimeSystem;

internal sealed class TimerFactory : ITimerFactory
{
	internal TimerFactory(RealTimeSystem timeSystem)
	{
		TimeSystem = timeSystem;
	}

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem { get; }

#if FEATURE_TIMER_COUNT
	/// <inheritdoc cref="ITimerFactory.ActiveCount" />
	public long ActiveCount => Timer.ActiveCount;
#endif

	/// <inheritdoc cref="ITimerFactory.New(TimerCallback)" />
	public ITimer New(TimerCallback callback)
		=> Wrap(new Timer(callback));

	/// <inheritdoc cref="ITimerFactory.New(TimerCallback, object?, int, int)" />
	public ITimer New(TimerCallback callback, object? state, int dueTime, int period)
		=> Wrap(new Timer(callback, state, dueTime, period));

	/// <inheritdoc cref="ITimerFactory.New(TimerCallback, object?, long, long)" />
	public ITimer New(TimerCallback callback, object? state, long dueTime, long period)
		=> Wrap(new Timer(callback, state, dueTime, period));

	/// <inheritdoc cref="ITimerFactory.New(TimerCallback, object?, TimeSpan, TimeSpan)" />
	public ITimer New(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
		=> Wrap(new Timer(callback, state, dueTime, period));

	/// <inheritdoc cref="IFileStreamFactory.Wrap(FileStream)" />
	public ITimer Wrap(Timer timer)
		=> new TimerWrapper(TimeSystem, timer);
}
