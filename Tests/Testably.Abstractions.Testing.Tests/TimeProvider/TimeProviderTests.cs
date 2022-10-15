﻿using System.Collections.Concurrent;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.TimeProvider;

public class TimeProviderTests
{
	[Fact]
	public void Now_ShouldReturnCurrentDateTime()
	{
		DateTime begin = DateTime.UtcNow;
		Testing.TimeSystemMock.ITimeProvider timeProvider = Testing.TimeProvider.Now();
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
			results.Add(Testing.TimeProvider.Random().Read());
		});

		results.Should().OnlyHaveUniqueItems();
	}

	[Fact]
	public void Use_ShouldReturnFixedDateTime()
	{
		DateTime now = TimeTestHelper.GetRandomTime();
		Testing.TimeSystemMock.ITimeProvider timeProvider = Testing.TimeProvider.Use(now);

		DateTime result1 = timeProvider.Read();
		DateTime result2 = timeProvider.Read();

		result1.Should().Be(now);
		result2.Should().Be(now);
	}
}