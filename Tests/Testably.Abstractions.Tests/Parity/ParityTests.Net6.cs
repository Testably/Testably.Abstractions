#if NET6_0
using Xunit.Abstractions;

namespace Testably.Abstractions.Tests.Parity;

public abstract partial class ParityTests
{
    public class Net6 : ParityTests
    {
        public Net6(ITestOutputHelper testOutputHelper)
            : base(new ParityExclusions(), testOutputHelper)
        {
        }
    }
}
#endif