namespace Testably.Abstractions.Tests;

public abstract partial class TimeSystemTests
{
    // ReSharper disable once UnusedMember.Global
    public sealed class MockTimeSystem : TimeSystemTests<TimeSystemMock>
    {
        public MockTimeSystem() : base(new TimeSystemMock())
        {
        }
    }
}