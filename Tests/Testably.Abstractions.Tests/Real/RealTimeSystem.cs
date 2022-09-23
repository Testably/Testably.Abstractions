namespace Testably.Abstractions.Tests.Real;

public static partial class RealTimeSystem
{
    // ReSharper disable once UnusedMember.Global
    public sealed class Tests : TimeSystemTests<TimeSystem>
    {
        public Tests() : base(new TimeSystem())
        {
        }
    }
}