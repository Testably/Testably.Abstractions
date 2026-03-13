#if FEATURE_PERIODIC_TIMER
using System;
using System.Threading;

namespace Testably.Abstractions.TimeSystem;

internal sealed class PeriodicTimerFactory : IPeriodicTimerFactory
{
	internal PeriodicTimerFactory(RealTimeSystem timeSystem)
	{
		TimeSystem = timeSystem;
	}

	#region IPeriodicTimerFactory Members

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem { get; }

	/// <inheritdoc cref="IPeriodicTimerFactory.New(TimeSpan)" />
	public IPeriodicTimer New(TimeSpan period)
		=> Wrap(new PeriodicTimer(period));

	/// <inheritdoc cref="IPeriodicTimerFactory.Wrap(PeriodicTimer)" />
	public IPeriodicTimer Wrap(PeriodicTimer timer)
		=> new PeriodicTimerWrapper(TimeSystem, timer);

	#endregion
}
#endif
