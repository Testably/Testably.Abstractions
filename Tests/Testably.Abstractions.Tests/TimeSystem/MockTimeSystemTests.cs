using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.TimeSystem;

[SystemTest(nameof(MockTimeSystemTests))]
public sealed class MockTimeSystemTests : TimeSystemTests<TimeSystemMock>
{
	public MockTimeSystemTests() : base(new TimeSystemMock())
	{
	}
}