namespace Testably.Abstractions.Tests.Mock;

public static partial class MockTimeSystem
{
    // ReSharper disable once UnusedMember.Global
    public sealed class ThreadTests : TimeSystemThreadTests<TimeSystemMock>
    {
        public ThreadTests() : base(new TimeSystemMock())
        {
        }
    }
}