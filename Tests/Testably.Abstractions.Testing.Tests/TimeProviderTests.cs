using System.Collections.Concurrent;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Tests.TestHelpers;
using Testably.Abstractions.Testing.TimeSystem;

namespace Testably.Abstractions.Testing.Tests;

public class TimeProviderTests
{
	[Fact]
	public async Task Now_ShouldReturnCurrentDateTime()
	{
		DateTime begin = DateTime.UtcNow;
		ITimeProvider timeProvider = TimeProvider.Now();
		DateTime end = DateTime.UtcNow;

		DateTime result1 = timeProvider.Read();
		DateTime result2 = timeProvider.Read();

		await That(result1).IsOnOrAfter(begin).And.IsOnOrBefore(end);
		await That(result2).IsOnOrAfter(begin).And.IsOnOrBefore(end);
	}

	[Fact]
	public async Task Random_ShouldReturnRandomDateTime()
	{
		ConcurrentBag<DateTime> results = [];

		Parallel.For(0, 100, _ =>
		{
			results.Add(TimeProvider.Random().Read());
		});

		await That(results).AreAllUnique();
	}

	[Theory]
	[AutoData]
	public async Task SetTo_ShouldChangeTimeForRead(DateTime time1, DateTime time2)
	{
		ITimeProvider timeProvider = TimeProvider.Use(time1);

		DateTime result1 = timeProvider.Read();
		timeProvider.SetTo(time2);
		DateTime result2 = timeProvider.Read();

		await That(result1).IsEqualTo(time1);
		await That(result2).IsEqualTo(time2);
	}

	[Fact]
	public async Task Use_ShouldReturnFixedDateTime()
	{
		DateTime now = TimeTestHelper.GetRandomTime();
		ITimeProvider timeProvider = TimeProvider.Use(now);

		DateTime result1 = timeProvider.Read();
		DateTime result2 = timeProvider.Read();

		await That(result1).IsEqualTo(now);
		await That(result2).IsEqualTo(now);
	}
}
