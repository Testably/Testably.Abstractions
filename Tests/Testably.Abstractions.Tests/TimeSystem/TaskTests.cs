using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.TimeSystem;

[TimeSystemTests]
public partial class TaskTests
{
	[SkippableFact]
	public async Task
		Delay_Milliseconds_Cancelled_ShouldThrowTaskCanceledException()
	{
		int millisecondsTimeout = 100;

		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			await TimeSystem.Task.Delay(millisecondsTimeout, cts.Token);
		});

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[SkippableFact]
	public async Task
		Delay_Milliseconds_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
	{
		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			await TimeSystem.Task.Delay(-2);
		});

		exception.Should().BeException<ArgumentOutOfRangeException>(hResult: -2146233086);
	}

	[SkippableFact]
	public async Task
		Delay_Milliseconds_ShouldDelayForSpecifiedMilliseconds()
	{
		int millisecondsTimeout = 100;

		DateTime before = TimeSystem.DateTime.UtcNow;
		await TimeSystem.Task.Delay(millisecondsTimeout);
		DateTime after = TimeSystem.DateTime.UtcNow;

		after.Should().BeOnOrAfter(
			before.AddMilliseconds(millisecondsTimeout).ApplySystemClockTolerance());
	}

	[SkippableFact]
	public async Task
		Delay_Timespan_Cancelled_ShouldThrowTaskCanceledException()
	{
		TimeSpan timeout = TimeSpan.FromMilliseconds(100);
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			await TimeSystem.Task.Delay(timeout, cts.Token);
		});

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[SkippableFact]
	public async Task
		Delay_Timespan_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
	{
		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			await TimeSystem.Task
				.Delay(TimeSpan.FromMilliseconds(-2));
		});

		exception.Should().BeException<ArgumentOutOfRangeException>(hResult: -2146233086);
	}

	[SkippableFact]
	public async Task
		Delay_Timespan_ShouldDelayForSpecifiedMilliseconds()
	{
		TimeSpan timeout = TimeSpan.FromMilliseconds(100);

		DateTime before = TimeSystem.DateTime.UtcNow;
		await TimeSystem.Task.Delay(timeout);
		DateTime after = TimeSystem.DateTime.UtcNow;

		after.Should().BeOnOrAfter(before.Add(timeout).ApplySystemClockTolerance());
	}
}
