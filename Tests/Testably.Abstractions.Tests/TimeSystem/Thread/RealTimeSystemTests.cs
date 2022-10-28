namespace Testably.Abstractions.Tests.TimeSystem.Thread;

public static class RealTimeSystemTests
{
	// ReSharper disable once UnusedMember.Global

	public sealed class ThreadTests : TimeSystemThreadTests<Abstractions.RealTimeSystem>
	{
		public ThreadTests() : base(new Abstractions.RealTimeSystem())
		{
		}
	}
}