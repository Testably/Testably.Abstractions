using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.Real;

public static partial class RealTimeSystem
{
    // ReSharper disable once UnusedMember.Global
    [SystemTest(nameof(RealTimeSystem))]
    public sealed class ThreadTests : TimeSystemThreadTests<TimeSystem>
    {
        public ThreadTests() : base(new TimeSystem())
        {
        }
    }
}