namespace Testably.Abstractions.Tests.Real;

public static partial class RealTimeSystem
{
    // ReSharper disable once UnusedMember.Global
    public sealed class DateTimeTests : TimeSystemDateTimeTests<TimeSystem>
    {
        #region Test Setup

        public DateTimeTests() : base(new TimeSystem())
        {
        }

        #endregion

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