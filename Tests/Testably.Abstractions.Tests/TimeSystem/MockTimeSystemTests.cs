namespace Testably.Abstractions.Tests.TimeSystem;

public sealed class MockTimeSystemTests : TimeSystemTests<MockTimeSystem>
{
	public MockTimeSystemTests() : base(new MockTimeSystem())
	{
	}
}