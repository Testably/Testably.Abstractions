using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.RandomSystem;

[SystemTest(nameof(MockRandomSystemTests))]
public sealed class MockRandomSystemTests : RandomSystemTests<RandomSystemMock>
{
	public MockRandomSystemTests() : base(new RandomSystemMock())
	{
	}
}