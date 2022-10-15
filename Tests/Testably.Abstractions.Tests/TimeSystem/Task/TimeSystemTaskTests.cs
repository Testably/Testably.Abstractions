namespace Testably.Abstractions.Tests.TimeSystem.Task;

public abstract class TimeSystemTaskTests<TTimeSystem>
	where TTimeSystem : ITimeSystem
{
	#region Test Setup

	public TTimeSystem TimeSystem { get; }

	protected TimeSystemTaskTests(TTimeSystem timeSystem)
	{
		TimeSystem = timeSystem;
	}

	#endregion

	[Fact]
	public async System.Threading.Tasks.Task
		Delay_Milliseconds_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
	{
		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			await TimeSystem.Task.Delay(-2).ConfigureAwait(false);
		}).ConfigureAwait(false);

		exception.Should().BeOfType<ArgumentOutOfRangeException>();
	}

	[Fact]
	public async System.Threading.Tasks.Task
		Delay_Milliseconds_ShouldDelayForSpecifiedMilliseconds()
	{
		int millisecondsTimeout = 100;

		System.DateTime before = TimeSystem.DateTime.UtcNow;
		await TimeSystem.Task.Delay(millisecondsTimeout).ConfigureAwait(false);
		System.DateTime after = TimeSystem.DateTime.UtcNow;

		after.Should().BeOnOrAfter(before.AddMilliseconds(millisecondsTimeout)
		   .ApplySystemClockTolerance());
	}

	[Fact]
	public async System.Threading.Tasks.Task
		Delay_Timespan_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
	{
		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			await TimeSystem.Task
			   .Delay(TimeSpan.FromMilliseconds(-2))
			   .ConfigureAwait(false);
		}).ConfigureAwait(false);

		exception.Should().BeOfType<ArgumentOutOfRangeException>();
	}

	[Fact]
	public async System.Threading.Tasks.Task
		Delay_Timespan_ShouldDelayForSpecifiedMilliseconds()
	{
		TimeSpan timeout = TimeSpan.FromMilliseconds(100);

		System.DateTime before = TimeSystem.DateTime.UtcNow;
		await TimeSystem.Task.Delay(timeout).ConfigureAwait(false);
		System.DateTime after = TimeSystem.DateTime.UtcNow;

		after.Should().BeOnOrAfter(before.Add(timeout).ApplySystemClockTolerance());
	}
}