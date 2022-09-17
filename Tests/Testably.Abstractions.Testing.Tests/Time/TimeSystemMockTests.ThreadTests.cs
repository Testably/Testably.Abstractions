using FluentAssertions;
using System;
using Xunit;

namespace Testably.Abstractions.Testing.Tests.Time;

public partial class TimeSystemMockTests
{
    public class ThreadTests
    {
        [Fact]
        public void Sleep_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
        {
            TimeSystemMock timeSystem = new();

            Exception? exception = Record.Exception(() => timeSystem.Thread.Sleep(-2));

            exception.Should().BeAssignableTo<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Sleep_Milliseconds_ShouldSleepForSpecifiedMilliseconds()
        {
            DateTime now = Helpers.GetRandomTime(DateTimeKind.Utc);
            TimeSystemMock timeSystem = new(now);
            int millisecondsTimeout = 10000;

            timeSystem.Thread.Sleep(millisecondsTimeout);
            DateTime after = timeSystem.DateTime.UtcNow;

            after.Should().Be(now.AddMilliseconds(millisecondsTimeout));
        }

        [Fact]
        public void
            Sleep_Timespan_LessThanNegativeOne_ShouldThrowArgumentOutOfRangeException()
        {
            TimeSystemMock timeSystem = new();

            Exception? exception = Record.Exception(() =>
                timeSystem.Thread.Sleep(TimeSpan.FromMilliseconds(-2)));

            exception.Should().BeAssignableTo<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Sleep_Timespan_ShouldSleepForSpecifiedMilliseconds()
        {
            DateTime now = Helpers.GetRandomTime(DateTimeKind.Utc);
            TimeSystemMock timeSystem = new(now);
            TimeSpan timeout = TimeSpan.FromMinutes(1);

            timeSystem.Thread.Sleep(timeout);
            DateTime after = timeSystem.DateTime.UtcNow;

            after.Should().Be(now.Add(timeout));
        }
    }
}