namespace Testably.Abstractions.Tests.Real;

public static partial class RealTimeSystem
{
    // ReSharper disable once UnusedMember.Global
    public sealed class ThreadTests : TimeSystemThreadTests<TimeSystem>
    {
        public ThreadTests() : base(new TimeSystem())
        {
        }
    }
}