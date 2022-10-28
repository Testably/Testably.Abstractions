namespace Testably.Abstractions.Tests.RandomSystem;

public sealed class MockRandomSystemTests : RandomSystemTests<MockRandomSystem>
{
	public MockRandomSystemTests() : base(new MockRandomSystem())
	{
	}
}