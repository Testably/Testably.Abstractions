namespace Testably.Abstractions.Tests;

public abstract partial class TimeSystemThreadTests
{
    // ReSharper disable once UnusedMember.Global
    public sealed class RealTimeSystem : TimeSystemThreadTests<TimeSystem>
    {
        public RealTimeSystem() : base(new TimeSystem())
        {
        }
    }
}