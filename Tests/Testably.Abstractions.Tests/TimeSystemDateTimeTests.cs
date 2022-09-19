using FluentAssertions;
using System;
using Xunit;

namespace Testably.Abstractions.Tests;

public abstract class TimeSystemDateTimeTests<TTimeSystem>
    where TTimeSystem : ITimeSystem
{
    #region Test Setup

    public TTimeSystem TimeSystem { get; }

    protected TimeSystemDateTimeTests(TTimeSystem timeSystem)
    {
        TimeSystem = timeSystem;
    }

    #endregion

    [Fact]
    public void MaxValue_ShouldReturnDefaultValue()
    {
        DateTime expectedResult = DateTime.MaxValue;

        DateTime result = TimeSystem.DateTime.MaxValue;

        result.Should().Be(expectedResult);
    }

    [Fact]
    public void MinValue_ShouldReturnDefaultValue()
    {
        DateTime expectedResult = DateTime.MinValue;

        DateTime result = TimeSystem.DateTime.MinValue;

        result.Should().Be(expectedResult);
    }

    [Fact]
    public void UnixEpoch_ShouldReturnDefaultValue()
    {
        DateTime expectedResult = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        DateTime result = TimeSystem.DateTime.UnixEpoch;

        result.Should().Be(expectedResult);
    }
}