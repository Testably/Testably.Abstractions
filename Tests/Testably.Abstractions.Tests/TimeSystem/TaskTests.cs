using System.Threading;

namespace Testably.Abstractions.Tests.TimeSystem;

[TimeSystemTests]
public class TaskTests(TimeSystemTestData testData) : TimeSystemTestBase(testData)
{
	[Test]
	public async Task
		Delay_Milliseconds_Cancelled_ShouldThrowTaskCanceledException()
	{
		int millisecondsTimeout = 100;

		using CancellationTokenSource cts = new();
		cts.Cancel();

		async Task Act()
			=> await TimeSystem.Task.Delay(millisecondsTimeout, cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Test]
	public async Task
		Delay_Milliseconds_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
	{
		async Task Act()
			=> await TimeSystem.Task.Delay(-2, CancellationToken);

		await That(Act).Throws<ArgumentOutOfRangeException>().WithHResult(-2146233086);
	}

	[Test]
	public async Task
		Delay_Milliseconds_ShouldDelayForSpecifiedMilliseconds()
	{
		int millisecondsTimeout = 100;

		DateTime before = TimeSystem.DateTime.UtcNow;
		await TimeSystem.Task.Delay(millisecondsTimeout, CancellationToken);
		DateTime after = TimeSystem.DateTime.UtcNow;

		await That(after)
			.IsOnOrAfter(before.AddMilliseconds(millisecondsTimeout).ApplySystemClockTolerance());
	}

	[Test]
	public async Task
		Delay_Timespan_Cancelled_ShouldThrowTaskCanceledException()
	{
		TimeSpan timeout = TimeSpan.FromMilliseconds(100);
		using CancellationTokenSource cts = new();
		cts.Cancel();

		async Task Act()
			=> await TimeSystem.Task.Delay(timeout, cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Test]
	public async Task
		Delay_Timespan_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
	{
		async Task Act()
			=> await TimeSystem.Task.Delay(TimeSpan.FromMilliseconds(-2),
				CancellationToken);

		await That(Act).Throws<ArgumentOutOfRangeException>().WithHResult(-2146233086);
	}

	[Test]
	public async Task
		Delay_Timespan_ShouldDelayForSpecifiedMilliseconds()
	{
		TimeSpan timeout = TimeSpan.FromMilliseconds(100);

		DateTime before = TimeSystem.DateTime.UtcNow;
		await TimeSystem.Task.Delay(timeout, CancellationToken);
		DateTime after = TimeSystem.DateTime.UtcNow;

		await That(after).IsOnOrAfter(before.Add(timeout).ApplySystemClockTolerance());
	}
}
