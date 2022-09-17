using FluentAssertions;
using System;
using Xunit;

namespace Testably.Abstractions.Tests.Time;

public partial class TimeSystemTests
{
    public class DateTimeTests
    {
        [Fact]
        public void MaxValue_ShouldReturnDefaultValue()
        {
            DateTime expectedResult = DateTime.MaxValue;
            TimeSystem timeSystem = new();

            DateTime result = timeSystem.DateTime.MaxValue;

            result.Should().Be(expectedResult);
        }

        [Fact]
        public void MinValue_ShouldReturnDefaultValue()
        {
            DateTime expectedResult = DateTime.MinValue;
            TimeSystem timeSystem = new();

            DateTime result = timeSystem.DateTime.MinValue;

            result.Should().Be(expectedResult);
        }

        [Fact]
        public void Now_ShouldReturnDefaultValue()
        {
            DateTime begin = DateTime.Now;
            TimeSystem timeSystem = new();

            DateTime result = timeSystem.DateTime.Now;

            DateTime end = DateTime.Now;

            result.Kind.Should().Be(DateTimeKind.Local);
            result.Should().BeOnOrAfter(begin).And.BeOnOrBefore(end);
        }

        [Fact]
        public void Today_ShouldReturnDefaultValue()
        {
            DateTime begin = DateTime.Today;
            TimeSystem timeSystem = new();

            DateTime result = timeSystem.DateTime.Today;

            DateTime end = DateTime.Today;

            result.Kind.Should().Be(begin.Kind);
            result.TimeOfDay.Should().Be(TimeSpan.Zero);
            result.Should().BeOnOrAfter(begin).And.BeOnOrBefore(end);
        }

        [Fact]
        public void UnixEpoch_ShouldReturnDefaultValue()
        {
            DateTime expectedResult = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSystem timeSystem = new();

            DateTime result = timeSystem.DateTime.UnixEpoch;

            result.Should().Be(expectedResult);
        }

        [Fact]
        public void UtcNow_ShouldReturnDefaultValue()
        {
            DateTime begin = DateTime.UtcNow;
            TimeSystem timeSystem = new();

            DateTime result = timeSystem.DateTime.UtcNow;

            DateTime end = DateTime.UtcNow;

            result.Kind.Should().Be(DateTimeKind.Utc);
            result.Should().BeOnOrAfter(begin).And.BeOnOrBefore(end);
        }
    }
}