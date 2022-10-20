namespace Testably.Abstractions.Tests.RandomSystem;

public sealed class MockRandomSystemTests : RandomSystemTests<RandomSystemMock>
{
	public MockRandomSystemTests() : base(new RandomSystemMock())
	{
	}
}