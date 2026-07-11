using System.Collections.ObjectModel;
using System.Linq;

namespace Testably.Abstractions.Tests.TimeSystem;

[TimeSystemTests]
public class TimeZoneInfoTests(TimeSystemTestData testData) : TimeSystemTestBase(testData)
{
	[Test]
	public async Task FindSystemTimeZoneById_UnknownId_ShouldThrowTimeZoneNotFoundException()
	{
		string unknownId = "Testably/DoesNotExist";
		int expectedHResult = 0;
		try
		{
			_ = TimeZoneInfo.FindSystemTimeZoneById(unknownId);
		}
		catch (TimeZoneNotFoundException exception)
		{
			expectedHResult = exception.HResult;
		}

		void Act()
			=> _ = TimeSystem.TimeZoneInfo.FindSystemTimeZoneById(unknownId);

		await That(Act).Throws<TimeZoneNotFoundException>()
			.WithMessage($"The time zone ID '{unknownId}' was not found on the local computer.").And
			.WithHResult(expectedHResult);
	}

	[Test]
	public async Task FindSystemTimeZoneById_Utc_ShouldReturnUtcTimeZone()
	{
		TimeZoneInfo result =
			TimeSystem.TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Utc.Id);

		await That(result.BaseUtcOffset).IsEqualTo(TimeSpan.Zero);
	}

	[Test]
	public async Task GetSystemTimeZones_ShouldContainUtc()
	{
		ReadOnlyCollection<TimeZoneInfo> result = TimeSystem.TimeZoneInfo.GetSystemTimeZones();

		await That(result.Select(timeZone => timeZone.Id)).Contains(TimeZoneInfo.Utc.Id);
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
