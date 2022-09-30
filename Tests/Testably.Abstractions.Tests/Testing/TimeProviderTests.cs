using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.Testing;

public class TimeProviderTests
{
    [Fact]
    [Trait(nameof(Testing), nameof(TimeProvider))]
    public void Now_ShouldReturnCurrentDateTime()
    {
        DateTime begin = DateTime.UtcNow;
        TimeSystemMock.ITimeProvider timeProvider = TimeProvider.Now();
        DateTime end = DateTime.UtcNow;

        DateTime result1 = timeProvider.Read();
        DateTime result2 = timeProvider.Read();

        result1.Should().BeOnOrAfter(begin).And.BeOnOrBefore(end);
        result2.Should().BeOnOrAfter(begin).And.BeOnOrBefore(end);
    }

    [Fact]
    [Trait(nameof(Testing), nameof(TimeProvider))]
    public void Random_ShouldReturnRandomDateTime()
    {
        ConcurrentBag<DateTime> results = new();

        Parallel.For(0, 100, _ =>
        {
            results.Add(TimeProvider.Random().Read());
        });

        results.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    [Trait(nameof(Testing), nameof(TimeProvider))]
    public void Use_ShouldReturnFixedDateTime()
    {
        DateTime now = TimeTestHelper.GetRandomTime();
        TimeSystemMock.ITimeProvider timeProvider = TimeProvider.Use(now);

        DateTime result1 = timeProvider.Read();
        DateTime result2 = timeProvider.Read();

        result1.Should().Be(now);
        result2.Should().Be(now);
    }
}