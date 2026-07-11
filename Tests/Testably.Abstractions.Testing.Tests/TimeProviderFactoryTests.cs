using System.Collections.Concurrent;
using Testably.Abstractions.Testing.Tests.TestHelpers;
using Testably.Abstractions.Testing.TimeSystem;

namespace Testably.Abstractions.Testing.Tests;

public class TimeProviderFactoryTests
{
	[Test]
	public async Task Now_ShouldUseLocalTimeZone()
	{
		ITimeProvider timeProvider = TimeProviderFactory.Now().Create(_ => { });

		await That(timeProvider.LocalTimeZone).IsEqualTo(TimeZoneInfo.Local);
	}

	[Test]
	public async Task Random_ShouldUseOneOfTheSampleTimeZones()
	{
		string[] sampleTimeZoneIds =
		[
			"UTC", "Sample/Plus0530", "Sample/Minus0800", "Sample/DaylightSaving",
		];

		ITimeProvider timeProvider = TimeProviderFactory.Random().Create(_ => { });

		await That(sampleTimeZoneIds).Contains(timeProvider.LocalTimeZone.Id);
	}

	[Test]
	public async Task Use_WithTimeZone_ShouldUseGivenTimeZone()
	{
		TimeZoneInfo timeZone = TimeZoneInfo.CreateCustomTimeZone(
			"Custom/Plus07", TimeSpan.FromHours(7), "Custom +07", "Custom +07");

		ITimeProvider timeProvider =
			TimeProviderFactory.Use(DateTime.UtcNow, timeZone).Create(_ => { });

		await That(timeProvider.LocalTimeZone).IsEqualTo(timeZone);
	}

	[Test]
	public async Task Now_ShouldReturnCurrentDateTime()
	{
		DateTime begin = DateTime.UtcNow;
		ITimeProvider timeProvider = TimeProviderFactory.Now().Create(_ => { });
		DateTime end = DateTime.UtcNow;

		DateTime result1 = timeProvider.Read();
		DateTime result2 = timeProvider.Read();

		await That(result1).IsOnOrAfter(begin).And.IsOnOrBefore(end);
		await That(result2).IsOnOrAfter(begin).And.IsOnOrBefore(end);
	}

	[Test]
	public async Task Random_ShouldReturnRandomDateTime()
	{
		ConcurrentBag<DateTime> results = [];

		Parallel.For(0, 100, _ =>
		{
			results.Add(TimeProviderFactory.Random().Create(_ => { }).Read());
		});

		await That(results).AreAllUnique();
	}

	[Test]
	[AutoArguments]
	public async Task SetTo_ShouldChangeTimeForRead(DateTime time1, DateTime time2)
	{
		ITimeProvider timeProvider = TimeProviderFactory.Use(time1).Create(_ => { });

		DateTime result1 = timeProvider.Read();
		timeProvider.SetTo(time2);
		DateTime result2 = timeProvider.Read();

		await That(result1).IsEqualTo(time1);
		await That(result2).IsEqualTo(time2);
	}

	[Test]
	public async Task Use_ShouldReturnFixedDateTime()
	{
		DateTime now = TimeTestHelper.GetRandomTime();
		ITimeProvider timeProvider = TimeProviderFactory.Use(now).Create(_ => { });

		DateTime result1 = timeProvider.Read();
		DateTime result2 = timeProvider.Read();

		await That(result1).IsEqualTo(now);
		await That(result2).IsEqualTo(now);
	}

	[Test]
	public async Task Use_UnspecifiedKind_ShouldConvertToUtcDateTime()
	{
		DateTime unspecifiedTime = TimeTestHelper.GetRandomTime();
		ITimeProvider timeProvider = TimeProviderFactory.Use(unspecifiedTime).Create(_ => { });
		DateTime result = timeProvider.Read();
		await That(result).IsEqualTo(DateTime.SpecifyKind(unspecifiedTime, DateTimeKind.Utc));
	}

#pragma warning disable CS0618 // Type or member is obsolete
	[Test]
	public async Task ObsoleteTimeProvider_ShouldForwardToTimeProviderFactory()
	{
		DateTime now = TimeTestHelper.GetRandomTime();

		DateTime fixedResult = TimeProvider.Use(now).Create(_ => { }).Read();

		await That(fixedResult).IsEqualTo(DateTime.SpecifyKind(now, DateTimeKind.Utc));
		await That(TimeProvider.Now()).IsNotNull();
		await That(TimeProvider.Random()).IsNotNull();
	}
#pragma warning restore CS0618
}
