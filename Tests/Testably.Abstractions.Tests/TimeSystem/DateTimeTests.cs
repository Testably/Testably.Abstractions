namespace Testably.Abstractions.Tests.TimeSystem;

[TimeSystemTests]
public partial class DateTimeTests
{
	[Fact]
	public async Task MaxValue_ShouldReturnDefaultValue()
	{
		DateTime expectedResult = DateTime.MaxValue;

		DateTime result = TimeSystem.DateTime.MaxValue;

		await That(result).IsEqualTo(expectedResult);
	}

	[Fact]
	public async Task MinValue_ShouldReturnDefaultValue()
	{
		DateTime expectedResult = DateTime.MinValue;

		DateTime result = TimeSystem.DateTime.MinValue;

		await That(result).IsEqualTo(expectedResult);
	}

	[Fact]
	public async Task Now_ShouldBeSetToNow()
	{
		// Tests are brittle on the build system
		TimeSpan tolerance = TimeSpan.FromMilliseconds(250);

		DateTime before = DateTime.Now;
		DateTime result = TimeSystem.DateTime.Now;
		DateTime after = DateTime.Now;

		await That(result.Kind).IsEqualTo(DateTimeKind.Local);
		await That(result).IsBetween(before).And(after).Within(tolerance);
	}

	[Fact]
	public async Task Today_ShouldBeSetToToday()
	{
		DateTime before = DateTime.Today;
		DateTime result = TimeSystem.DateTime.Today;
		DateTime after = DateTime.Today;

		await That(result.Hour).IsEqualTo(0);
		await That(result.Minute).IsEqualTo(0);
		await That(result.Second).IsEqualTo(0);
		await That(result).IsBetween(before).And(after).Within(TimeComparison.Tolerance);
	}

	[Fact]
	public async Task UnixEpoch_ShouldReturnDefaultValue()
	{
#pragma warning disable MA0113 // Use DateTime.UnixEpoch
		DateTime expectedResult = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
#pragma warning restore MA0113

		DateTime result = TimeSystem.DateTime.UnixEpoch;

		await That(result).IsEqualTo(expectedResult);
	}

	[Fact]
	public async Task UtcNow_ShouldBeSetToUtcNow()
	{
		// Tests are brittle on the build system
		TimeSpan tolerance = TimeSpan.FromMilliseconds(250);

		DateTime before = DateTime.UtcNow;
		DateTime result = TimeSystem.DateTime.UtcNow;
		DateTime after = DateTime.UtcNow;

		await That(result.Kind).IsEqualTo(DateTimeKind.Utc);
		await That(result).IsBetween(before).And(after).Within(tolerance);
	}
}
