using FluentAssertions;
using Xunit;

namespace Testably.Abstractions.Tests.Time;

public partial class TimeSystemTests
{
    [Fact]
    public void DateTime_ShouldSetExtensionPoint()
    {
        TimeSystem timeSystem = new();

        ITimeSystem result = timeSystem.DateTime.TimeSystem;

        result.Should().Be(timeSystem);
    }

    [Fact]
    public void Task_ShouldSetExtensionPoint()
    {
        TimeSystem timeSystem = new();

        ITimeSystem result = timeSystem.Task.TimeSystem;

        result.Should().Be(timeSystem);
    }

    [Fact]
    public void Thread_ShouldSetExtensionPoint()
    {
        TimeSystem timeSystem = new();

        ITimeSystem result = timeSystem.Thread.TimeSystem;

        result.Should().Be(timeSystem);
    }
}