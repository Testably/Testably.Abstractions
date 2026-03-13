#if FEATURE_PERIODIC_TIMER
using System.Threading;

namespace Testably.Abstractions.Testing.Tests.TimeSystem;

public class PeriodicTimerFactoryMockTests
{
	[Test]
	public async Task Wrap_ShouldThrowNotSupportedException()
	{
		MockTimeSystem timeSystem = new();
		using PeriodicTimer periodicTimer = new(TimeSpan.FromSeconds(5));

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			_ = timeSystem.PeriodicTimer.Wrap(periodicTimer);
		}

		await That(Act).Throws<NotSupportedException>().WithMessage(
			"You cannot wrap an existing PeriodicTimer in the MockTimeSystem instance!");
	}
}
#endif
