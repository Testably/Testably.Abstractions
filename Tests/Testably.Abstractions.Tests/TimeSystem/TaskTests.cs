using System.Threading;

namespace Testably.Abstractions.Tests.TimeSystem;

[TimeSystemTests]
public partial class TaskTests
{
	[Fact]
	public async Task
		Delay_Milliseconds_Cancelled_ShouldThrowTaskCanceledException()
	{
		int millisecondsTimeout = 100;

		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		async Task Act()
			=> await TimeSystem.Task.Delay(millisecondsTimeout, cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Fact]
	public async Task
		Delay_Milliseconds_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
	{
		async Task Act()
			=> await TimeSystem.Task.Delay(-2, TestContext.Current.CancellationToken);

		await That(Act).Throws<ArgumentOutOfRangeException>().WithHResult(-2146233086);
	}

	[Fact]
	public async Task
		Delay_Milliseconds_ShouldDelayForSpecifiedMilliseconds()
	{
		int millisecondsTimeout = 100;

		DateTime before = TimeSystem.DateTime.UtcNow;
		await TimeSystem.Task.Delay(millisecondsTimeout, TestContext.Current.CancellationToken);
		DateTime after = TimeSystem.DateTime.UtcNow;

		await That(after)
			.IsOnOrAfter(before.AddMilliseconds(millisecondsTimeout).ApplySystemClockTolerance());
	}

	[Fact]
	public async Task
		Delay_Timespan_Cancelled_ShouldThrowTaskCanceledException()
	{
		TimeSpan timeout = TimeSpan.FromMilliseconds(100);
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		async Task Act()
			=> await TimeSystem.Task.Delay(timeout, cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Fact]
	public async Task
		Delay_Timespan_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
	{
		async Task Act()
			=> await TimeSystem.Task.Delay(TimeSpan.FromMilliseconds(-2),
				TestContext.Current.CancellationToken);

		await That(Act).Throws<ArgumentOutOfRangeException>().WithHResult(-2146233086);
	}

	[Fact]
	public async Task
		Delay_Timespan_ShouldDelayForSpecifiedMilliseconds()
	{
		TimeSpan timeout = TimeSpan.FromMilliseconds(100);

		DateTime before = TimeSystem.DateTime.UtcNow;
		await TimeSystem.Task.Delay(timeout, TestContext.Current.CancellationToken);
		DateTime after = TimeSystem.DateTime.UtcNow;

		await That(after).IsOnOrAfter(before.Add(timeout).ApplySystemClockTolerance());
	}
}
