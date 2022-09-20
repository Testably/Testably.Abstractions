namespace Testably.Abstractions.Tests;

public abstract class TimeSystemTests<TTimeSystem>
    where TTimeSystem : ITimeSystem
{
    #region Test Setup

    public TTimeSystem TimeSystem { get; }

    protected TimeSystemTests(TTimeSystem timeSystem)
    {
        TimeSystem = timeSystem;
    }

    #endregion

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