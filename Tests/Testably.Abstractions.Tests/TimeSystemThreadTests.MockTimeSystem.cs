namespace Testably.Abstractions.Tests;

public abstract partial class TimeSystemThreadTests
{
    // ReSharper disable once UnusedMember.Global
    public sealed class MockTimeSystem : TimeSystemThreadTests<TimeSystemMock>
    {
        public MockTimeSystem() : base(new TimeSystemMock())
        {
        }
    }
}