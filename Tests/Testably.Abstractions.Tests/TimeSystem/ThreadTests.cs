namespace Testably.Abstractions.Tests.TimeSystem;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ThreadTests<TTimeSystem>
	: TimeSystemTestBase<TTimeSystem>
	where TTimeSystem : ITimeSystem
{
	[Fact]
	public void Sleep_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
	{
		Exception? exception = Record.Exception(() => TimeSystem.Thread.Sleep(-2));

		exception.Should().BeOfType<ArgumentOutOfRangeException>()
		   .Which.HResult.Should().Be(-2146233086);
	}

	[Fact]
	public void Sleep_Milliseconds_ShouldSleepForSpecifiedMilliseconds()
	{
		int millisecondsTimeout = 10;

		DateTime before = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(millisecondsTimeout);
		DateTime after = TimeSystem.DateTime.UtcNow;

		after.Should().BeOnOrAfter(before.AddMilliseconds(millisecondsTimeout));
	}

	[Fact]
	public void
		Sleep_Timespan_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
	{
		Exception? exception = Record.Exception(() =>
			TimeSystem.Thread.Sleep(TimeSpan.FromMilliseconds(-2)));

		exception.Should().BeOfType<ArgumentOutOfRangeException>()
		   .Which.HResult.Should().Be(-2146233086);
	}

	[Fact]
	public void Sleep_Timespan_ShouldSleepForSpecifiedMilliseconds()
	{
		TimeSpan timeout = TimeSpan.FromMilliseconds(10);

		DateTime before = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(timeout);
		DateTime after = TimeSystem.DateTime.UtcNow;

		after.Should().BeOnOrAfter(before.Add(timeout));
	}
}