using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.TimeSystem.Thread;

public static class MockTimeSystem
{
	// ReSharper disable once UnusedMember.Global
	[SystemTest(nameof(MockTimeSystemTests))]
	public sealed class ThreadTests : TimeSystemThreadTests<TimeSystemMock>
	{
		public ThreadTests() : base(new TimeSystemMock())
		{
		}
	}
}