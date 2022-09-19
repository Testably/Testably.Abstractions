using FluentAssertions;
using System;
using Xunit;

namespace Testably.Abstractions.Tests;

public abstract class TimeSystemThreadTests<TTimeSystem>
    where TTimeSystem : ITimeSystem
{
    #region Test Setup

    public TTimeSystem TimeSystem { get; }

    protected TimeSystemThreadTests(TTimeSystem timeSystem)
    {
        TimeSystem = timeSystem;
    }

    #endregion

    [Fact]
    public void Sleep_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
    {
        Exception? exception = Record.Exception(() => TimeSystem.Thread.Sleep(-2));

        exception.Should().BeAssignableTo<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Sleep_Milliseconds_ShouldSleepForSpecifiedMilliseconds()
    {
        int millisecondsTimeout = 10;

        DateTime before = TimeSystem.DateTime.UtcNow;
        TimeSystem.Thread.Sleep(millisecondsTimeout);
        DateTime after = TimeSystem.DateTime.UtcNow;

        after.Should().BeOnOrAfter(before.AddMilliseconds(millisecondsTimeout));
    }

    [Fact]
    public void
        Sleep_Timespan_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
    {
        Exception? exception = Record.Exception(() =>
            TimeSystem.Thread.Sleep(TimeSpan.FromMilliseconds(-2)));

        exception.Should().BeAssignableTo<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Sleep_Timespan_ShouldSleepForSpecifiedMilliseconds()
    {
        TimeSpan timeout = TimeSpan.FromMilliseconds(10);

        DateTime before = TimeSystem.DateTime.UtcNow;
        TimeSystem.Thread.Sleep(timeout);
        DateTime after = TimeSystem.DateTime.UtcNow;

        after.Should().BeOnOrAfter(before.Add(timeout));
    }
}