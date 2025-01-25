namespace Testably.Abstractions.Tests.TimeSystem;

[TimeSystemTests]
public partial class ThreadTests
{
	[Fact]
	public void Sleep_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
	{
		Exception? exception = Record.Exception(() => TimeSystem.Thread.Sleep(-2));

		exception.Should().BeException<ArgumentOutOfRangeException>(hResult: -2146233086);
	}

	[Fact]
	public void Sleep_Milliseconds_ShouldSleepForSpecifiedMilliseconds()
	{
		int millisecondsTimeout = 100;

		DateTime before = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(millisecondsTimeout);
		DateTime after = TimeSystem.DateTime.UtcNow;

		after.Should().BeOnOrAfter(
			before.AddMilliseconds(millisecondsTimeout).ApplySystemClockTolerance());
	}

	[Fact]
	public void
		Sleep_Timespan_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
	{
		Exception? exception = Record.Exception(() =>
			TimeSystem.Thread.Sleep(TimeSpan.FromMilliseconds(-2)));

		exception.Should().BeException<ArgumentOutOfRangeException>(hResult: -2146233086);
	}

	[Fact]
	public void Sleep_Timespan_ShouldSleepForSpecifiedMilliseconds()
	{
		TimeSpan timeout = TimeSpan.FromMilliseconds(100);

		DateTime before = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(timeout);
		DateTime after = TimeSystem.DateTime.UtcNow;

		after.Should().BeOnOrAfter(
			before.Add(timeout).ApplySystemClockTolerance());
	}
}
