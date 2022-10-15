using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.TimeSystem.Thread;

public static class RealTimeSystem
{
	// ReSharper disable once UnusedMember.Global
	[SystemTest(nameof(RealTimeSystemTests))]
	public sealed class ThreadTests : TimeSystemThreadTests<Abstractions.TimeSystem>
	{
		public ThreadTests() : base(new Abstractions.TimeSystem())
		{
		}
	}
}