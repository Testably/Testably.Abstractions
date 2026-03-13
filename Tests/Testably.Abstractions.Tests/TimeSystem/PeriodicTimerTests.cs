#if FEATURE_PERIODIC_TIMER
using aweXpect.Chronology;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Tests.TimeSystem;

[TimeSystemTests]
public class PeriodicTimerTests(TimeSystemTestData testData) : TimeSystemTestBase(testData)
{
	[Test]
	public async Task Dispose_Twice_ShouldNotThrow()
	{
		TimeSpan period = 200.Milliseconds();
		IPeriodicTimer timer = TimeSystem.PeriodicTimer.New(period);
		timer.Dispose();

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Dispose();
		}

		await That(Act).DoesNotThrow();
	}

	[Test]
	public async Task SetPeriod_PeriodIsInfinite_ShouldNotThrow()
	{
		TimeSpan period = TimeSpan.FromMilliseconds(-1);
		using IPeriodicTimer timer = TimeSystem.PeriodicTimer.New(5.Seconds());

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Period = period;
		}

		await That(Act).DoesNotThrow();
	}

	[Test]
	[Arguments(0)]
	[Arguments(-2)]
	public async Task SetPeriod_PeriodIsZeroOrNegative_ShouldThrowArgumentOutOfRangeException(
		int milliseconds)
	{
		TimeSpan period = TimeSpan.FromMilliseconds(milliseconds);
		using IPeriodicTimer timer = TimeSystem.PeriodicTimer.New(5.Seconds());

		void Act()
		{
			// ReSharper disable once AccessToDisposedClosure
			timer.Period = period;
		}

		await That(Act).Throws<ArgumentOutOfRangeException>().WithParamName("value");
	}

	[Test]
	public async Task WaitForNextTickAsync_ShouldWaitForPeriodOnFirstCall()
	{
		TimeSpan period = 200.Milliseconds();
		using IPeriodicTimer timer = TimeSystem.PeriodicTimer.New(period);

		DateTime before = TimeSystem.DateTime.UtcNow;
		bool result = await timer.WaitForNextTickAsync();
		DateTime after = TimeSystem.DateTime.UtcNow;

		await That(result).IsTrue();
		await That(after - before).IsGreaterThanOrEqualTo(period);
	}

	[Test]
	public async Task
		WaitForNextTickAsync_WhenCallbackTakesLongerThanPeriod_ShouldContinueWithNewTime()
	{
		Skip.If(TimeSystem is RealTimeSystem);

		TimeSpan period = 1.Seconds();
		using IPeriodicTimer timer = TimeSystem.PeriodicTimer.New(period);

		DateTime before1 = TimeSystem.DateTime.UtcNow;
		await timer.WaitForNextTickAsync();
		DateTime after1 = TimeSystem.DateTime.UtcNow;
		await TimeSystem.Task.Delay(2.Seconds());
		DateTime before2 = TimeSystem.DateTime.UtcNow;
		await timer.WaitForNextTickAsync();
		DateTime after2 = TimeSystem.DateTime.UtcNow;
		await TimeSystem.Task.Delay(2.Seconds());
		DateTime before3 = TimeSystem.DateTime.UtcNow;
		await timer.WaitForNextTickAsync();
		DateTime after3 = TimeSystem.DateTime.UtcNow;

		await That(after1 - before1).IsGreaterThanOrEqualTo(500.Milliseconds());
		await That(after2 - before2).IsLessThan(500.Milliseconds());
		await That(after3 - before3).IsLessThan(500.Milliseconds());
	}

	[Test]
	public async Task WaitForNextTickAsync_WhenDisposed_ShouldReturnFalse()
	{
		TimeSpan period = 200.Milliseconds();
		IPeriodicTimer timer = TimeSystem.PeriodicTimer.New(period);
		timer.Dispose();

		bool result = await timer.WaitForNextTickAsync();

		await That(result).IsFalse();
	}
}
#endif
