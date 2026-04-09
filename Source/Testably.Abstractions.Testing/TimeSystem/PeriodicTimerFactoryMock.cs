#if FEATURE_PERIODIC_TIMER
using System;
using System.Threading;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.TimeSystem;

internal sealed class PeriodicTimerFactoryMock : IPeriodicTimerFactory
{
	private readonly MockTimeSystem _mockTimeSystem;
	private readonly NotificationHandler _callbackHandler;
	private readonly bool _autoAdvance;

	internal PeriodicTimerFactoryMock(MockTimeSystem timeSystem,
		NotificationHandler callbackHandler, bool autoAdvance)
	{
		_mockTimeSystem = timeSystem;
		_callbackHandler = callbackHandler;
		_autoAdvance = autoAdvance;
	}

	#region IPeriodicTimerFactory Members

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem => _mockTimeSystem;

	/// <inheritdoc cref="IPeriodicTimerFactory.New(TimeSpan)" />
	public IPeriodicTimer New(TimeSpan period)
		=> new PeriodicTimerMock(_mockTimeSystem, _callbackHandler, period, _autoAdvance);

	/// <inheritdoc cref="IPeriodicTimerFactory.Wrap(PeriodicTimer)" />
	public IPeriodicTimer Wrap(PeriodicTimer timer)
		=> throw ExceptionFactory.NotSupportedPeriodicTimerWrapping();

	#endregion
}
#endif
