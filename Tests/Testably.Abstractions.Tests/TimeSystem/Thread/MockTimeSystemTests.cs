namespace Testably.Abstractions.Tests.TimeSystem.Thread;

public static class MockTimeSystemTests
{
	public sealed class ThreadTests : TimeSystemThreadTests<TimeSystemMock>
	{
		public ThreadTests() : base(new TimeSystemMock())
		{
		}
	}
}