namespace Testably.Abstractions.Tests.Mock;

public static partial class MockTimeSystem
{
    // ReSharper disable once UnusedMember.Global
    public sealed class Tests : TimeSystemTests<TimeSystemMock>
    {
        public Tests() : base(new TimeSystemMock())
        {
        }
    }
}