using FluentAssertions;
using System;
using Xunit;

namespace Testably.Abstractions.Tests.Time;

public partial class TimeSystemTests
{
    public class ThreadTests
    {
        [Fact]
        public void Sleep_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
        {
            TimeSystem timeSystem = new();

            Exception? exception = Record.Exception(() => timeSystem.Thread.Sleep(-2));

            exception.Should().BeAssignableTo<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Sleep_Milliseconds_ShouldSleepForSpecifiedMilliseconds()
        {
            TimeSystem timeSystem = new();
            int millisecondsTimeout = 10;

            DateTime before = DateTime.UtcNow;
            timeSystem.Thread.Sleep(millisecondsTimeout);
            DateTime after = DateTime.UtcNow;

            after.Should().BeOnOrAfter(before.AddMilliseconds(millisecondsTimeout));
        }

        [Fact]
        public void
            Sleep_Timespan_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
        {
            TimeSystem timeSystem = new();

            Exception? exception = Record.Exception(() =>
                timeSystem.Thread.Sleep(TimeSpan.FromMilliseconds(-2)));

            exception.Should().BeAssignableTo<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Sleep_Timespan_ShouldSleepForSpecifiedMilliseconds()
        {
            TimeSystem timeSystem = new();
            TimeSpan timeout = TimeSpan.FromMilliseconds(10);

            DateTime before = DateTime.UtcNow;
            timeSystem.Thread.Sleep(timeout);
            DateTime after = DateTime.UtcNow;

            after.Should().BeOnOrAfter(before.Add(timeout));
        }
    }
}