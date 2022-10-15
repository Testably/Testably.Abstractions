using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.TimeSystem.Thread;

public static partial class RealTimeSystem
{
	// ReSharper disable once UnusedMember.Global
	[SystemTest(nameof(Tests.TimeSystem.RealTimeSystemTests))]
	public sealed class ThreadTests : TimeSystemThreadTests<Abstractions.TimeSystem>
	{
		public ThreadTests() : base(new Abstractions.TimeSystem())
		{
		}
	}
}