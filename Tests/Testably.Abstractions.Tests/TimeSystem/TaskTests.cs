using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.TimeSystem;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class TaskTests<TTimeSystem>
	: TimeSystemTestBase<TTimeSystem>
	where TTimeSystem : ITimeSystem
{
	[Fact]
	public async Task
		Delay_Milliseconds_Cancelled_ShouldThrowTaskCanceledException()
	{
		int millisecondsTimeout = 100;

		using CancellationTokenSource cts = new();
		cts.Cancel();

		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			await TimeSystem.Task.Delay(millisecondsTimeout, cts.Token)
				.ConfigureAwait(false);
		});

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[Fact]
	public async Task
		Delay_Milliseconds_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
	{
		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			await TimeSystem.Task.Delay(-2).ConfigureAwait(false);
		}).ConfigureAwait(false);

		exception.Should().BeException<ArgumentOutOfRangeException>(hResult: -2146233086);
	}

	[Fact]
	public async Task
		Delay_Milliseconds_ShouldDelayForSpecifiedMilliseconds()
	{
		int millisecondsTimeout = 100;

		DateTime before = TimeSystem.DateTime.UtcNow;
		await TimeSystem.Task.Delay(millisecondsTimeout).ConfigureAwait(false);
		DateTime after = TimeSystem.DateTime.UtcNow;

		after.Should().BeOnOrAfter(before.AddMilliseconds(millisecondsTimeout)
			.ApplySystemClockTolerance());
	}

	[Fact]
	public async Task
		Delay_Timespan_Cancelled_ShouldThrowTaskCanceledException()
	{
		TimeSpan timeout = TimeSpan.FromMilliseconds(100);
		using CancellationTokenSource cts = new();
		cts.Cancel();

		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			await TimeSystem.Task.Delay(timeout, cts.Token)
				.ConfigureAwait(false);
		});

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[Fact]
	public async Task
		Delay_Timespan_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
	{
		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			await TimeSystem.Task
				.Delay(TimeSpan.FromMilliseconds(-2))
				.ConfigureAwait(false);
		}).ConfigureAwait(false);

		exception.Should().BeException<ArgumentOutOfRangeException>(hResult: -2146233086);
	}

	[Fact]
	public async Task
		Delay_Timespan_ShouldDelayForSpecifiedMilliseconds()
	{
		TimeSpan timeout = TimeSpan.FromMilliseconds(100);

		DateTime before = TimeSystem.DateTime.UtcNow;
		await TimeSystem.Task.Delay(timeout).ConfigureAwait(false);
		DateTime after = TimeSystem.DateTime.UtcNow;

		after.Should().BeOnOrAfter(before.Add(timeout).ApplySystemClockTolerance());
	}
}
