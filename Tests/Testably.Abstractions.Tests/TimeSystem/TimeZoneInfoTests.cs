using System.Collections.ObjectModel;

namespace Testably.Abstractions.Tests.TimeSystem;

[TimeSystemTests]
public class TimeZoneInfoTests(TimeSystemTestData testData) : TimeSystemTestBase(testData)
{
	[Test]
	public async Task FindSystemTimeZoneById_Utc_ShouldReturnUtcTimeZone()
	{
		TimeZoneInfo result =
			TimeSystem.TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Utc.Id);

		await That(result.BaseUtcOffset).IsEqualTo(TimeSpan.Zero);
	}

	[Test]
	public async Task GetSystemTimeZones_ShouldNotBeEmpty()
	{
		ReadOnlyCollection<TimeZoneInfo> result = TimeSystem.TimeZoneInfo.GetSystemTimeZones();

		await That(result).IsNotEmpty();
	}

	[Test]
	public async Task Local_ShouldReturnLocalTimeZone()
	{
		TimeZoneInfo result = TimeSystem.TimeZoneInfo.Local;

		await That(result).IsEqualTo(TimeZoneInfo.Local);
	}

	[Test]
	public async Task Utc_ShouldReturnUtcTimeZone()
	{
		TimeZoneInfo result = TimeSystem.TimeZoneInfo.Utc;

		await That(result).IsEqualTo(TimeZoneInfo.Utc);
	}
}
