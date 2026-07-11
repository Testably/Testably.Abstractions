namespace Testably.Abstractions.Testing.Tests.TimeSystem;

public class DateTimeOffsetMockTests
{
	[Test]
	public async Task Now_ShouldUseConfiguredLocalTimeZoneOffset()
	{
		DateTime utcNow = new(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);
		TimeZoneInfo timeZone = TimeZoneInfo.CreateCustomTimeZone(
			"Custom/Plus05", TimeSpan.FromHours(5), "Custom +05", "Custom +05");
		MockTimeSystem timeSystem = new(TimeProviderFactory.Use(utcNow, timeZone));

		DateTimeOffset now = timeSystem.DateTimeOffset.Now;

		await That(now.Offset).IsEqualTo(TimeSpan.FromHours(5));
		await That(now)
			.IsEqualTo(new DateTimeOffset(2024, 1, 15, 17, 0, 0, TimeSpan.FromHours(5)));
	}

	[Test]
	public async Task UtcNow_ShouldUseZeroOffset()
	{
		DateTime utcNow = new(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);
		TimeZoneInfo timeZone = TimeZoneInfo.CreateCustomTimeZone(
			"Custom/Plus05", TimeSpan.FromHours(5), "Custom +05", "Custom +05");
		MockTimeSystem timeSystem = new(TimeProviderFactory.Use(utcNow, timeZone));

		DateTimeOffset result = timeSystem.DateTimeOffset.UtcNow;

		await That(result.Offset).IsEqualTo(TimeSpan.Zero);
		await That(result).IsEqualTo(new DateTimeOffset(utcNow));
	}
}
