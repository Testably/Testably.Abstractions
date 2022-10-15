using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.TimeSystem;

[SystemTest(nameof(RealTimeSystemTests))]
public sealed class RealTimeSystemTests : TimeSystemTests<Abstractions.TimeSystem>
{
	public RealTimeSystemTests() : base(new Abstractions.TimeSystem())
	{
	}
}