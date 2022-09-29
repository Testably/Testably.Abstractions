using Testably.Abstractions.Tests.Real;
using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.Mock;

public static partial class MockTimeSystem
{
    // ReSharper disable once UnusedMember.Global
    [SystemTest(nameof(MockTimeSystem))]
    public sealed class ThreadTests : TimeSystemThreadTests<TimeSystemMock>
    {
        public ThreadTests() : base(new TimeSystemMock())
        {
        }
    }
}