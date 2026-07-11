namespace Testably.Abstractions.Testing.Tests.TimeSystem;

public class DateTimeOffsetMockTests
{
	[Test]
	public async Task Now_WithLocalKindInput_ShouldInterpretAgainstConfiguredZone_NotHostZone()
	{
		DateTime localInput = new(2024, 1, 15, 12, 0, 0, DateTimeKind.Local);
		TimeZoneInfo plus05 = TimeZoneInfo.CreateCustomTimeZone(
			"Custom/Plus05", TimeSpan.FromHours(5), "Custom +05", "Custom +05");
		TimeZoneInfo plus08 = TimeZoneInfo.CreateCustomTimeZone(
			"Custom/Plus08", TimeSpan.FromHours(8), "Custom +08", "Custom +08");
		MockTimeSystem inPlus05 = new(TimeProviderFactory.Use(localInput, plus05));
		MockTimeSystem inPlus08 = new(TimeProviderFactory.Use(localInput, plus08));

		// The local wall-clock time round-trips to the input regardless of the configured (or host) zone.
		await That(inPlus05.DateTimeOffset.Now)
			.IsEqualTo(new DateTimeOffset(2024, 1, 15, 12, 0, 0, TimeSpan.FromHours(5)));
		await That(inPlus05.DateTimeOffset.Now.Offset).IsEqualTo(TimeSpan.FromHours(5));
		await That(inPlus08.DateTimeOffset.Now)
			.IsEqualTo(new DateTimeOffset(2024, 1, 15, 12, 0, 0, TimeSpan.FromHours(8)));
		await That(inPlus08.DateTimeOffset.Now.Offset).IsEqualTo(TimeSpan.FromHours(8));

		// The underlying UTC instant is derived from the configured zone (12:00 - offset), not the host.
		await That(inPlus05.DateTimeOffset.UtcNow)
			.IsEqualTo(new DateTimeOffset(2024, 1, 15, 7, 0, 0, TimeSpan.Zero));
		await That(inPlus08.DateTimeOffset.UtcNow)
			.IsEqualTo(new DateTimeOffset(2024, 1, 15, 4, 0, 0, TimeSpan.Zero));
	}

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
