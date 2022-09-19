using FluentAssertions;
using Xunit;

namespace Testably.Abstractions.Tests;

public abstract class TimeSystemTests<TTimeSystem>
    where TTimeSystem : ITimeSystem
{
    public TTimeSystem TimeSystem { get; }

    protected TimeSystemTests(TTimeSystem timeSystem)
    {
        TimeSystem = timeSystem;
    }

    [Fact]
    public void DateTime_ShouldSetExtensionPoint()
    {
        ITimeSystem result = TimeSystem.DateTime.TimeSystem;

        result.Should().Be(TimeSystem);
    }

    [Fact]
    public void Task_ShouldSetExtensionPoint()
    {
        ITimeSystem result = TimeSystem.Task.TimeSystem;

        result.Should().Be(TimeSystem);
    }

    [Fact]
    public void Thread_ShouldSetExtensionPoint()
    {
        ITimeSystem result = TimeSystem.Thread.TimeSystem;

        result.Should().Be(TimeSystem);
    }
}