namespace Testably.Abstractions.Tests.TimeSystem;

// ReSharper disable once UnusedMember.Global
public sealed class MockTimeSystemTests : TimeSystemTests<MockTimeSystem>
{
	public MockTimeSystemTests() : base(new MockTimeSystem())
	{
	}
}