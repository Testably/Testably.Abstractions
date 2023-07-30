using System.Collections.Concurrent;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Tests.TestHelpers;
using Testably.Abstractions.Testing.TimeSystem;

namespace Testably.Abstractions.Testing.Tests;

public class TimeProviderTests
{
	[Fact]
	public void Now_ShouldReturnCurrentDateTime()
	{
		DateTime begin = DateTime.UtcNow;
		ITimeProvider timeProvider = TimeProvider.Now();
		DateTime end = DateTime.UtcNow;

		DateTime result1 = timeProvider.Read();
		DateTime result2 = timeProvider.Read();

		result1.Should().BeOnOrAfter(begin).And.BeOnOrBefore(end);
		result2.Should().BeOnOrAfter(begin).And.BeOnOrBefore(end);
	}

	[Fact]
	public void Random_ShouldReturnRandomDateTime()
	{
		ConcurrentBag<DateTime> results = new();

		Parallel.For(0, 100, _ =>
		{
			results.Add(TimeProvider.Random().Read());
		});

		results.Should().OnlyHaveUniqueItems();
	}

	[Theory]
	[AutoData]
	public void SetTo_ShouldChangeTimeForRead(DateTime time1, DateTime time2)
	{
		ITimeProvider timeProvider = TimeProvider.Use(time1);

		DateTime result1 = timeProvider.Read();
		timeProvider.SetTo(time2);
		DateTime result2 = timeProvider.Read();

		result1.Should().Be(time1);
		result2.Should().Be(time2);
	}

	[Fact]
	public void Use_ShouldReturnFixedDateTime()
	{
		DateTime now = TimeTestHelper.GetRandomTime();
		ITimeProvider timeProvider = TimeProvider.Use(now);

		DateTime result1 = timeProvider.Read();
		DateTime result2 = timeProvider.Read();

		result1.Should().Be(now);
		result2.Should().Be(now);
	}
}
