namespace Testably.Abstractions.Testing.Tests.TimeSystem;

public class TimeZoneInfoMockTests
{
	[Test]
	public async Task FindSystemTimeZoneById_UnknownId_ShouldThrowTimeZoneNotFoundException()
	{
		MockTimeSystem timeSystem = new();

		void Act()
			=> timeSystem.TimeZoneInfo.FindSystemTimeZoneById("Unknown/Does-Not-Exist");

		await That(Act).Throws<TimeZoneNotFoundException>();
	}

	[Test]
	public async Task LocalTimeZone_WhenSetOnProvider_ShouldBeReturnedByLocal()
	{
		TimeZoneInfo timeZone = TimeZoneInfo.CreateCustomTimeZone(
			"Custom/Plus02", TimeSpan.FromHours(2), "Custom +02", "Custom +02");
		MockTimeSystem timeSystem = new();

		timeSystem.TimeProvider.LocalTimeZone = timeZone;

		await That(timeSystem.TimeZoneInfo.Local).IsEqualTo(timeZone);
	}

	[Test]
	public async Task Now_ShouldUseConfiguredLocalTimeZoneOffset()
	{
		DateTime utcNow = new(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);
		TimeZoneInfo timeZone = TimeZoneInfo.CreateCustomTimeZone(
			"Custom/Plus05", TimeSpan.FromHours(5), "Custom +05", "Custom +05");
		MockTimeSystem timeSystem = new(TimeProviderFactory.Use(utcNow, timeZone));

		DateTime now = timeSystem.DateTime.Now;

		await That(now).IsEqualTo(new DateTime(2024, 1, 15, 17, 0, 0, DateTimeKind.Local));
		await That(now.Kind).IsEqualTo(DateTimeKind.Local);
		await That(timeSystem.DateTime.UtcNow).IsEqualTo(utcNow);
	}

	[Test]
	public async Task RegisterTimeZone_ShouldBeResolvableAndEnumerated()
	{
		TimeZoneInfo timeZone = TimeZoneInfo.CreateCustomTimeZone(
			"Custom/Plus03", TimeSpan.FromHours(3), "Custom +03", "Custom +03");
		MockTimeSystem timeSystem = new();

		timeSystem.TimeProvider.RegisterTimeZone(timeZone);

		await That(timeSystem.TimeZoneInfo.FindSystemTimeZoneById("Custom/Plus03"))
			.IsEqualTo(timeZone);
		await That(timeSystem.TimeZoneInfo.GetSystemTimeZones()).Contains(timeZone);
	}

	[Test]
	public async Task SettingLocalTimeZone_ShouldRegisterItForLookup()
	{
		TimeZoneInfo timeZone = TimeZoneInfo.CreateCustomTimeZone(
			"Custom/Plus04", TimeSpan.FromHours(4), "Custom +04", "Custom +04");
		MockTimeSystem timeSystem = new();

		timeSystem.TimeProvider.LocalTimeZone = timeZone;

		await That(timeSystem.TimeZoneInfo.FindSystemTimeZoneById("Custom/Plus04"))
			.IsEqualTo(timeZone);
	}
}
