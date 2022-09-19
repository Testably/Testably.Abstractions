using FluentAssertions;
using System;
using System.Threading.Tasks;
using Testably.Abstractions.Tests.TestHelpers;
using Xunit;

namespace Testably.Abstractions.Tests;

public abstract class TimeSystemTaskTests<TTimeSystem>
    where TTimeSystem : ITimeSystem
{
    #region Test Setup

    public TTimeSystem TimeSystem { get; }

    protected TimeSystemTaskTests(TTimeSystem timeSystem)
    {
        TimeSystem = timeSystem;
    }

    #endregion

    [Fact]
    public async Task
        Delay_Milliseconds_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
    {
        Exception? exception = await Record.ExceptionAsync(async () =>
        {
            await TimeSystem.Task.Delay(-2).ConfigureAwait(false);
        }).ConfigureAwait(false);

        exception.Should().BeAssignableTo<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task Delay_Milliseconds_ShouldDelayForSpecifiedMilliseconds()
    {
        int millisecondsTimeout = 100;

        DateTime before = TimeSystem.DateTime.UtcNow;
        await TimeSystem.Task.Delay(millisecondsTimeout).ConfigureAwait(false);
        DateTime after = TimeSystem.DateTime.UtcNow;

        after.Should().BeOnOrAfter(before.AddMilliseconds(millisecondsTimeout)
           .ApplySystemClockTolerance());
    }

    [Fact]
    public async Task
        Delay_Timespan_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
    {
        Exception? exception = await Record.ExceptionAsync(async () =>
        {
            await TimeSystem.Task
               .Delay(TimeSpan.FromMilliseconds(-2))
               .ConfigureAwait(false);
        }).ConfigureAwait(false);

        exception.Should().BeAssignableTo<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task Delay_Timespan_ShouldDelayForSpecifiedMilliseconds()
    {
        TimeSpan timeout = TimeSpan.FromMilliseconds(100);

        DateTime before = TimeSystem.DateTime.UtcNow;
        await TimeSystem.Task.Delay(timeout).ConfigureAwait(false);
        DateTime after = TimeSystem.DateTime.UtcNow;

        after.Should().BeOnOrAfter(before.Add(timeout).ApplySystemClockTolerance());
    }
}