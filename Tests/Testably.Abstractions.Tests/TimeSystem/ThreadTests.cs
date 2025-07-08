namespace Testably.Abstractions.Tests.TimeSystem;

[TimeSystemTests]
public partial class ThreadTests
{
	[Fact]
	public async Task Sleep_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
	{
		void Act() => TimeSystem.Thread.Sleep(-2);

		await That(Act).Throws<ArgumentOutOfRangeException>().WithHResult(-2146233086);
	}

	[Fact]
	public async Task Sleep_Milliseconds_ShouldSleepForSpecifiedMilliseconds()
	{
		int millisecondsTimeout = 100;

		DateTime before = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(millisecondsTimeout);
		DateTime after = TimeSystem.DateTime.UtcNow;

		await That(after).IsOnOrAfter(before.AddMilliseconds(millisecondsTimeout).ApplySystemClockTolerance());
	}

	[Fact]
	public async Task Sleep_Timespan_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
	{
		void Act() =>
			TimeSystem.Thread.Sleep(TimeSpan.FromMilliseconds(-2));

		await That(Act).Throws<ArgumentOutOfRangeException>().WithHResult(-2146233086);
	}

	[Fact]
	public async Task Sleep_Timespan_ShouldSleepForSpecifiedMilliseconds()
	{
		TimeSpan timeout = TimeSpan.FromMilliseconds(100);

		DateTime before = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(timeout);
		DateTime after = TimeSystem.DateTime.UtcNow;

		await That(after).IsOnOrAfter(before.Add(timeout).ApplySystemClockTolerance());
	}
}
