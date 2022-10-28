namespace Testably.Abstractions.Tests.TimeSystem.Thread;

public static class MockTimeSystemTests
{
	public sealed class ThreadTests : TimeSystemThreadTests<MockTimeSystem>
	{
		public ThreadTests() : base(new MockTimeSystem())
		{
		}
	}
}