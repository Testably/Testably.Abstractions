using FluentAssertions;
using System;
using Xunit;

namespace Testably.Abstractions.Testing.Tests.Time;

public partial class TimeSystemMockTests
{
    public class DateTimeTests
    {
        [Fact]
        public void MaxValue_FakedValue_ShouldReturnFakedValue()
        {
            DateTime now = Helpers.GetRandomTime();
            TimeSystemMock timeSystem = new() { TimeProvider = { MaxValue = now } };

            DateTime result = timeSystem.DateTime.MaxValue;

            result.Should().Be(now);
        }

        [Fact]
        public void MaxValue_ShouldReturnDefaultMaxValue()
        {
            TimeSystemMock timeSystem = new();

            DateTime result = timeSystem.DateTime.MaxValue;

            result.Should().Be(DateTime.MaxValue);
        }

        [Fact]
        public void MinValue_FakedValue_ShouldReturnFakedValue()
        {
            DateTime now = Helpers.GetRandomTime();
            TimeSystemMock timeSystem = new() { TimeProvider = { MinValue = now } };

            DateTime result = timeSystem.DateTime.MinValue;

            result.Should().Be(now);
        }

        [Fact]
        public void MinValue_ShouldReturnDefaultMinValue()
        {
            TimeSystemMock timeSystem = new();

            DateTime result = timeSystem.DateTime.MinValue;

            result.Should().Be(DateTime.MinValue);
        }

        [Fact]
        public void Now_Repeatedly_ShouldReturnSameTime()
        {
            DateTime now = Helpers.GetRandomTime();
            TimeSystemMock timeSystem = new(now);

            DateTime result1 = timeSystem.DateTime.Now;
            DateTime result2 = timeSystem.DateTime.Now;

            result2.Should().Be(result1);
        }

        [Fact]
        public void Now_SetTime_ShouldReturnSetTime()
        {
            DateTime now1 = Helpers.GetRandomTime(DateTimeKind.Local);
            DateTime now2 = Helpers.GetRandomTime(DateTimeKind.Local);
            TimeSystemMock timeSystem = new(now1);

            DateTime result1 = timeSystem.DateTime.Now;
            timeSystem.TimeProvider.SetTo(now2);
            DateTime result2 = timeSystem.DateTime.Now;

            result1.Should().Be(now1);
            result2.Should().Be(now2);
        }

        [Fact]
        public void Now_ShouldReturnLocalTime()
        {
            DateTime now = Helpers.GetRandomTime();
            TimeSystemMock timeSystem = new(now);

            DateTime result = timeSystem.DateTime.Now;

            result.Kind.Should().Be(DateTimeKind.Local);
            result.Should().Be(now.ToLocalTime());
        }

        [Fact]
        public void Today_Repeatedly_ShouldReturnSameTime()
        {
            DateTime now = Helpers.GetRandomTime();
            TimeSystemMock timeSystem = new(now);

            DateTime result1 = timeSystem.DateTime.Today;
            DateTime result2 = timeSystem.DateTime.Today;

            result2.Should().Be(result1);
        }

        [Fact]
        public void Today_SetTime_ShouldReturnSetTime()
        {
            DateTime now1 = Helpers.GetRandomTime().Date;
            DateTime now2 = Helpers.GetRandomTime().Date;
            TimeSystemMock timeSystem = new(now1);

            DateTime result1 = timeSystem.DateTime.Today;
            timeSystem.TimeProvider.SetTo(now2);
            DateTime result2 = timeSystem.DateTime.Today;

            result1.Should().Be(now1);
            result2.Should().Be(now2);
        }

        [Fact]
        public void Today_ShouldReturnDateWithoutTime()
        {
            DateTime now = Helpers.GetRandomTime();
            TimeSystemMock timeSystem = new(now);

            DateTime result = timeSystem.DateTime.Today;

            result.TimeOfDay.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void UnixEpoch_FakedValue_ShouldReturnFakedValue()
        {
            DateTime now = Helpers.GetRandomTime();
            TimeSystemMock timeSystem = new() { TimeProvider = { UnixEpoch = now } };

            DateTime result = timeSystem.DateTime.UnixEpoch;

            result.Should().Be(now);
        }

        [Fact]
        public void UnixEpoch_ShouldReturnDefaultValue()
        {
            TimeSystemMock timeSystem = new();

            DateTime result = timeSystem.DateTime.UnixEpoch;

            result.Should().Be(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        }

        [Fact]
        public void UtcNow_Repeatedly_ShouldReturnSameTime()
        {
            DateTime now = Helpers.GetRandomTime();
            TimeSystemMock timeSystem = new(now);

            DateTime result1 = timeSystem.DateTime.UtcNow;
            DateTime result2 = timeSystem.DateTime.UtcNow;

            result2.Should().Be(result1);
        }

        [Fact]
        public void UtcNow_SetTime_ShouldReturnSetTime()
        {
            DateTime now1 = Helpers.GetRandomTime(DateTimeKind.Utc);
            DateTime now2 = Helpers.GetRandomTime(DateTimeKind.Utc);
            TimeSystemMock timeSystem = new(now1);

            DateTime result1 = timeSystem.DateTime.UtcNow;
            timeSystem.TimeProvider.SetTo(now2);
            DateTime result2 = timeSystem.DateTime.UtcNow;

            result1.Should().Be(now1);
            result2.Should().Be(now2);
        }

        [Fact]
        public void UtcNow_ShouldReturnUniversalTime()
        {
            DateTime now = Helpers.GetRandomTime();
            TimeSystemMock timeSystem = new(now);

            DateTime result = timeSystem.DateTime.UtcNow;

            result.Kind.Should().Be(DateTimeKind.Utc);
            result.Should().Be(now.ToUniversalTime());
        }
    }
}