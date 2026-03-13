#if FEATURE_PERIODIC_TIMER
using System;
using System.Threading;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.TimeSystem;

internal sealed class PeriodicTimerFactoryMock : IPeriodicTimerFactory
{
	private readonly MockTimeSystem _mockTimeSystem;

	internal PeriodicTimerFactoryMock(MockTimeSystem timeSystem)
	{
		_mockTimeSystem = timeSystem;
	}

	#region IPeriodicTimerFactory Members

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem => _mockTimeSystem;

	/// <inheritdoc cref="IPeriodicTimerFactory.New(TimeSpan)" />
	public IPeriodicTimer New(TimeSpan period)
		=> new PeriodicTimerMock(_mockTimeSystem, period);

	/// <inheritdoc cref="IPeriodicTimerFactory.Wrap(PeriodicTimer)" />
	public IPeriodicTimer Wrap(PeriodicTimer timer)
		=> throw ExceptionFactory.NotSupportedPeriodicTimerWrapping();

	#endregion
}
#endif
