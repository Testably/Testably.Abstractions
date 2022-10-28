namespace Testably.Abstractions.Tests.RandomSystem;

// ReSharper disable once UnusedMember.Global
public sealed class MockRandomSystemTests : RandomSystemTests<MockRandomSystem>
{
	public MockRandomSystemTests() : base(new MockRandomSystem())
	{
	}
}