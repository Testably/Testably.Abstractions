namespace Testably.Abstractions.Tests.TimeSystem.Thread;

// ReSharper disable once UnusedMember.Global
public static class MockTimeSystemTests
{
	// ReSharper disable once UnusedMember.Global
	public sealed class ThreadTests : TimeSystemThreadTests<MockTimeSystem>
	{
		public ThreadTests() : base(new MockTimeSystem())
		{
		}
	}
}