#if FEATURE_PERIODIC_TIMER
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Tests.TimeSystem;

[TimeSystemTests]
public class PeriodicTimerFactoryTests(TimeSystemTestData testData) : TimeSystemTestBase(testData)
{
	[Test]
	public async Task New_PeriodIsInfinite_ShouldNotThrow()
	{
		TimeSpan period = TimeSpan.FromMilliseconds(-1);

		void Act()
		{
			_ = TimeSystem.PeriodicTimer.New(period);
		}

		await That(Act).DoesNotThrow();
	}

	[Test]
	[Arguments(0)]
	[Arguments(-2)]
	public async Task New_PeriodIsZeroOrNegative_ShouldThrowArgumentOutOfRangeException(
		int milliseconds)
	{
		TimeSpan period = TimeSpan.FromMilliseconds(milliseconds);

		void Act()
		{
			_ = TimeSystem.PeriodicTimer.New(period);
		}

		await That(Act).Throws<ArgumentOutOfRangeException>().WithParamName("period");
	}

	[Test]
	public async Task New_ShouldCreatePeriodicTimerWithGivenPeriod()
	{
		TimeSpan period = TimeSpan.FromSeconds(1);
		using IPeriodicTimer timer = TimeSystem.PeriodicTimer.New(period);

		await That(timer.Period).IsEqualTo(period);
	}
}
#endif
