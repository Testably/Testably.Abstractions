using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.Real;

public static partial class RealRandomSystem
{
    // ReSharper disable once UnusedMember.Global
    [SystemTest(nameof(RealRandomSystem))]
    public sealed class GuidTests : RandomSystemGuidTests<RandomSystem>
    {
        public GuidTests() : base(new RandomSystem())
        {
        }
    }
}