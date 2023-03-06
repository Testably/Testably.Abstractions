using System;
using System.Threading;

namespace Testably.Abstractions.TimeSystem;

/// <summary>
///     Factory for abstracting creation of <see cref="Timer" />.
/// </summary>
public interface ITimerFactory : ITimeSystemEntity
{
#if FEATURE_TIMER_COUNT
	/// <inheritdoc cref="Timer.ActiveCount" />
	long ActiveCount { get; }
#endif

	/// <inheritdoc cref="Timer(TimerCallback)" />
	ITimer New(TimerCallback callback);

	/// <inheritdoc cref="Timer(TimerCallback, object?, int, int)" />
	ITimer New(TimerCallback callback, object? state, int dueTime, int period);

	/// <inheritdoc cref="Timer(TimerCallback, object?, long, long)" />
	ITimer New(TimerCallback callback, object? state, long dueTime, long period);

	/// <inheritdoc cref="Timer(TimerCallback, object?, TimeSpan, TimeSpan)" />
	ITimer New(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period);

	/// <summary>
	///     Wraps the <paramref name="timer" /> to the testable <see cref="ITimer" />.
	/// </summary>
	ITimer Wrap(Timer timer);
}
