#if FEATURE_PERIODIC_TIMER
using System;
using System.Threading;

namespace Testably.Abstractions.TimeSystem;

/// <summary>
///     Factory for abstracting creation of <see cref="PeriodicTimer" />.
/// </summary>
public interface IPeriodicTimerFactory : ITimeSystemEntity
{
	/// <inheritdoc cref="PeriodicTimer(TimeSpan)" />
	IPeriodicTimer New(TimeSpan period);

	/// <summary>
	///     Wraps the <paramref name="timer" /> to the testable <see cref="IPeriodicTimer" />.
	/// </summary>
	IPeriodicTimer Wrap(PeriodicTimer timer);
}
#endif
