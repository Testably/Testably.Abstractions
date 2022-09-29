using System.Linq.Expressions;
using Testably.Abstractions.Tests.TestHelpers.Traits;

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

/// <summary>
///     Attributes for <see cref="TimeSystemTests{TTimeSystem}" />
/// </summary>
public static class TimeSystemTests
{
    /// <summary>
    ///     Tests for methods in <see cref="ITimeSystem.IDateTime" /> in <see cref="ITimeSystem" />.
    /// </summary>
    public class DateTime : TestabilityTraitAttribute
    {
        public DateTime(string method) : base(nameof(ITimeSystem),
            nameof(ITimeSystem.IDateTime), method)
        {
        }
    }

    /// <summary>
    ///     Tests for methods in <see cref="ITimeSystem.ITask" /> in <see cref="ITimeSystem" />.
    /// </summary>
    public class Task : TestabilityTraitAttribute
    {
        public Task(string method) : base(nameof(ITimeSystem),
            nameof(ITimeSystem.ITask), method)
        {
        }
    }

    /// <summary>
    ///     Tests for methods in <see cref="ITimeSystem.IThread" /> in <see cref="ITimeSystem" />.
    /// </summary>
    public class Thread : TestabilityTraitAttribute
    {
        public Thread(string method) : base(nameof(ITimeSystem),
            nameof(ITimeSystem.IThread),
            method)
        {
        }
    }
}