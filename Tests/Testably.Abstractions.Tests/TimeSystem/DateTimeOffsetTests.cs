using aweXpect.Chronology;

namespace Testably.Abstractions.Tests.TimeSystem;

[TimeSystemTests]
public class DateTimeOffsetTests(TimeSystemTestData testData) : TimeSystemTestBase(testData)
{
	[Test]
	public async Task MaxValue_ShouldReturnDefaultValue()
	{
		DateTimeOffset expectedResult = DateTimeOffset.MaxValue;

		DateTimeOffset result = TimeSystem.DateTimeOffset.MaxValue;

		await That(result).IsEqualTo(expectedResult);
	}

	[Test]
	public async Task MinValue_ShouldReturnDefaultValue()
	{
		DateTimeOffset expectedResult = DateTimeOffset.MinValue;

		DateTimeOffset result = TimeSystem.DateTimeOffset.MinValue;

		await That(result).IsEqualTo(expectedResult);
	}

	[Test]
	public async Task Now_ShouldBeSetToNow()
	{
		// Tests are brittle on the build system
		TimeSpan tolerance = TimeSpan.FromMilliseconds(250);

		DateTimeOffset result = TimeSystem.DateTimeOffset.Now;
		DateTimeOffset after = DateTimeOffset.Now;

		await That(result.Offset).IsEqualTo(TimeZoneInfo.Local.GetUtcOffset(result));
		await That(result).IsBetween(new DateTimeOffset(BeforeTime.ToLocalTime())).And(after)
			.Within(tolerance);
	}

	[Test]
	public async Task NowAndUtcNow_ShouldRepresentTheSameInstant()
	{
		DateTimeOffset utcNowBefore = TimeSystem.DateTimeOffset.UtcNow;
		DateTimeOffset now = TimeSystem.DateTimeOffset.Now;
		DateTimeOffset utcNowAfter = TimeSystem.DateTimeOffset.UtcNow;

		await That(now.Offset).IsEqualTo(TimeZoneInfo.Local.GetUtcOffset(now));
		await That(now.UtcDateTime)
			.IsBetween(utcNowBefore.UtcDateTime).And(utcNowAfter.UtcDateTime)
			.Within(50.Milliseconds());
	}

	[Test]
	public async Task UnixEpoch_ShouldReturnDefaultValue()
	{
		DateTimeOffset expectedResult = new(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

		DateTimeOffset result = TimeSystem.DateTimeOffset.UnixEpoch;

		await That(result).IsEqualTo(expectedResult);
	}

	[Test]
	public async Task UtcNow_ShouldBeSetToUtcNow()
	{
		// Tests are brittle on the build system
		TimeSpan tolerance = TimeSpan.FromMilliseconds(250);

		DateTimeOffset result = TimeSystem.DateTimeOffset.UtcNow;
		DateTimeOffset after = DateTimeOffset.UtcNow;

		await That(result.Offset).IsEqualTo(TimeSpan.Zero);
		await That(result).IsBetween(new DateTimeOffset(BeforeTime, TimeSpan.Zero)).And(after)
			.Within(tolerance);
	}
}
