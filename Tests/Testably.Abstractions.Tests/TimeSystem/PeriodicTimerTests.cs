#if FEATURE_PERIODIC_TIMER
using aweXpect.Chronology;
using System.Threading;
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
		bool result = await timer.WaitForNextTickAsync(CancellationToken);
		DateTime after = TimeSystem.DateTime.UtcNow;

		await That(result).IsTrue();
		await That(after - before).IsGreaterThanOrEqualTo(period).Within(50.Milliseconds());
	}

	[Test]
	public async Task
		WaitForNextTickAsync_WhenCallbackTakesLongerThanPeriod_ShouldContinueWithNewTime()
	{
		Skip.If(TimeSystem is RealTimeSystem);

		TimeSpan period = 1.Seconds();
		using IPeriodicTimer timer = TimeSystem.PeriodicTimer.New(period);

		DateTime before1 = TimeSystem.DateTime.UtcNow;
		await timer.WaitForNextTickAsync(CancellationToken);
		DateTime after1 = TimeSystem.DateTime.UtcNow;
		await TimeSystem.Task.Delay(0.5.Seconds(), CancellationToken);
		DateTime before2 = TimeSystem.DateTime.UtcNow;
		await timer.WaitForNextTickAsync(CancellationToken);
		DateTime after2 = TimeSystem.DateTime.UtcNow;
		await TimeSystem.Task.Delay(2.Seconds(), CancellationToken);
		DateTime before3 = TimeSystem.DateTime.UtcNow;
		await timer.WaitForNextTickAsync(CancellationToken);
		DateTime after3 = TimeSystem.DateTime.UtcNow;

		await That(after1 - before1).IsEqualTo(1.Seconds()).Within(100.Milliseconds());
		await That(after2 - before2).IsEqualTo(500.Milliseconds()).Within(100.Milliseconds());
		await That(after3 - before3).IsEqualTo(0.Milliseconds()).Within(100.Milliseconds());
	}

	[Test]
	public async Task WaitForNextTickAsync_WhenCancelled_ShouldThrowTaskCanceledException()
	{
		TimeSpan period = 200.Milliseconds();
		IPeriodicTimer timer = TimeSystem.PeriodicTimer.New(period);
		CancellationToken cancellationToken = new(true);

		async Task Act()
		{
			await timer.WaitForNextTickAsync(cancellationToken);
		}

		await That(Act).Throws<TaskCanceledException>()
			.WithMessage("A task was canceled.");
	}

	[Test]
	public async Task
		WaitForNextTickAsync_WhenCancelledAndDisposed_ShouldThrowTaskCanceledException()
	{
		TimeSpan period = 200.Milliseconds();
		IPeriodicTimer timer = TimeSystem.PeriodicTimer.New(period);
		CancellationToken cancellationToken = new(true);
		timer.Dispose();

		async Task Act()
		{
			await timer.WaitForNextTickAsync(cancellationToken);
		}

		await That(Act).Throws<TaskCanceledException>()
			.WithMessage("A task was canceled.");
	}

	[Test]
	public async Task WaitForNextTickAsync_WhenDisposed_ShouldReturnFalse()
	{
		TimeSpan period = 200.Milliseconds();
		IPeriodicTimer timer = TimeSystem.PeriodicTimer.New(period);
		timer.Dispose();

		#pragma warning disable MA0040 // Use an overload with a CancellationToken
		bool result = await timer.WaitForNextTickAsync();
		#pragma warning restore MA0040

		await That(result).IsFalse();
	}
}
#endif
