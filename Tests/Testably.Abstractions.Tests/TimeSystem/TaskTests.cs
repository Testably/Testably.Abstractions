using System.Threading;
using System.Threading.Tasks;

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
		
		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[Fact]
	public async Task
		Delay_Milliseconds_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
	{
		async Task Act()
			=> await TimeSystem.Task.Delay(-2);
		
		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<ArgumentOutOfRangeException>(hResult: -2146233086);
	}

	[Fact]
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

	[Fact]
	public async Task
		Delay_Timespan_Cancelled_ShouldThrowTaskCanceledException()
	{
		TimeSpan timeout = TimeSpan.FromMilliseconds(100);
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		async Task Act()
			=> await TimeSystem.Task.Delay(timeout, cts.Token);
		
		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[Fact]
	public async Task
		Delay_Timespan_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
	{
		async Task Act()
			=> await TimeSystem.Task.Delay(TimeSpan.FromMilliseconds(-2));
		
		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<ArgumentOutOfRangeException>(hResult: -2146233086);
	}

	[Fact]
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
