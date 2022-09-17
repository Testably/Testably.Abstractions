using FluentAssertions;
using Xunit;

namespace Testably.Abstractions.Testing.Tests.Time;

public partial class TimeSystemMockTests
{
    [Fact]
    public void DateTime_ShouldSetExtensionPoint()
    {
        TimeSystemMock timeSystem = new();

        ITimeSystem result = timeSystem.DateTime.TimeSystem;

        result.Should().Be(timeSystem);
    }

    [Fact]
    public void Task_ShouldSetExtensionPoint()
    {
        TimeSystemMock timeSystem = new();

        ITimeSystem result = timeSystem.Task.TimeSystem;

        result.Should().Be(timeSystem);
    }

    [Fact]
    public void Thread_ShouldSetExtensionPoint()
    {
        TimeSystemMock timeSystem = new();

        ITimeSystem result = timeSystem.Thread.TimeSystem;

        result.Should().Be(timeSystem);
    }
}