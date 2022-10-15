using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.TimeSystem.Thread;

public static class MockTimeSystemTests
{
	// ReSharper disable once UnusedMember.Global
	[SystemTest(nameof(Tests.TimeSystem.MockTimeSystemTests))]
	public sealed class ThreadTests : TimeSystemThreadTests<TimeSystemMock>
	{
		public ThreadTests() : base(new TimeSystemMock())
		{
		}
	}
}