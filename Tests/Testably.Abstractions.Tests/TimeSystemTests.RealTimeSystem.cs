namespace Testably.Abstractions.Tests;

public abstract partial class TimeSystemTests
{
    // ReSharper disable once UnusedMember.Global
    public sealed class RealTimeSystem : TimeSystemTests<TimeSystem>
    {
        public RealTimeSystem() : base(new TimeSystem())
        {
        }
    }
}