using FluentAssertions;
using System;
using Xunit;

namespace Testably.Abstractions.Tests;

public abstract partial class TimeSystemDateTimeTests
{
    // ReSharper disable once UnusedMember.Global
    public sealed class RealTimeSystem : TimeSystemDateTimeTests<TimeSystem>
    {
        public RealTimeSystem() : base(new TimeSystem())
        {
        }

        [Fact]
        public void Now_ShouldReturnDefaultValue()
        {
            DateTime begin = DateTime.Now;

            DateTime result = TimeSystem.DateTime.Now;

            DateTime end = DateTime.Now;

            result.Kind.Should().Be(DateTimeKind.Local);
            result.Should().BeOnOrAfter(begin).And.BeOnOrBefore(end);
        }

        [Fact]
        public void Today_ShouldReturnDefaultValue()
        {
            DateTime begin = DateTime.Today;

            DateTime result = TimeSystem.DateTime.Today;

            DateTime end = DateTime.Today;

            result.Kind.Should().Be(begin.Kind);
            result.TimeOfDay.Should().Be(TimeSpan.Zero);
            result.Should().BeOnOrAfter(begin).And.BeOnOrBefore(end);
        }

        [Fact]
        public void UtcNow_ShouldReturnDefaultValue()
        {
            DateTime begin = DateTime.UtcNow;

            DateTime result = TimeSystem.DateTime.UtcNow;

            DateTime end = DateTime.UtcNow;

            result.Kind.Should().Be(DateTimeKind.Utc);
            result.Should().BeOnOrAfter(begin).And.BeOnOrBefore(end);
        }
    }
}