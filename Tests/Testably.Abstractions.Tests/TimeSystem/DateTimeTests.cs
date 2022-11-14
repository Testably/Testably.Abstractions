namespace Testably.Abstractions.Tests.TimeSystem;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class DateTimeTests<TTimeSystem>
	: TimeSystemTestBase<TTimeSystem>
	where TTimeSystem : ITimeSystem
{
	[Fact]
	public void MaxValue_ShouldReturnDefaultValue()
	{
		DateTime expectedResult = DateTime.MaxValue;

		DateTime result = TimeSystem.DateTime.MaxValue;

		result.Should().Be(expectedResult);
	}

	[Fact]
	public void MinValue_ShouldReturnDefaultValue()
	{
		DateTime expectedResult = DateTime.MinValue;

		DateTime result = TimeSystem.DateTime.MinValue;

		result.Should().Be(expectedResult);
	}

	[SkippableFact]
	public void Now_ShouldBeSetToNow()
	{
		// Tests are brittle on the build system
		const int tolerance = 250;

		DateTime before = DateTime.Now;
		DateTime result = TimeSystem.DateTime.Now;
		DateTime after = DateTime.Now;

		result.Kind.Should().Be(DateTimeKind.Local);
		result.Should().BeOnOrAfter(before.ApplySystemClockTolerance(tolerance));
		result.ApplySystemClockTolerance(tolerance).Should().BeOnOrBefore(after);
	}

	[Fact]
	public void Today_ShouldBeSetToToday()
	{
		DateTime before = DateTime.Today;
		DateTime result = TimeSystem.DateTime.Today;
		DateTime after = DateTime.Today;

		result.Hour.Should().Be(0);
		result.Minute.Should().Be(0);
		result.Second.Should().Be(0);
		result.Should().BeOnOrAfter(before.ApplySystemClockTolerance());
		result.ApplySystemClockTolerance().Should().BeOnOrBefore(after);
	}

	[Fact]
	public void UnixEpoch_ShouldReturnDefaultValue()
	{
		DateTime expectedResult = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		DateTime result = TimeSystem.DateTime.UnixEpoch;

		result.Should().Be(expectedResult);
	}

	[SkippableFact]
	public void UtcNow_ShouldBeSetToUtcNow()
	{
		// Tests are brittle on the build system
		const int tolerance = 250;

		DateTime before = DateTime.UtcNow;
		DateTime result = TimeSystem.DateTime.UtcNow;
		DateTime after = DateTime.UtcNow;

		result.Kind.Should().Be(DateTimeKind.Utc);
		result.Should().BeOnOrAfter(before.ApplySystemClockTolerance(tolerance));
		result.ApplySystemClockTolerance(tolerance).Should().BeOnOrBefore(after);
	}
}