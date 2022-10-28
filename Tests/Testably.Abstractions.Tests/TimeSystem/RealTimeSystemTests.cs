namespace Testably.Abstractions.Tests.TimeSystem;

public sealed class RealTimeSystemTests : TimeSystemTests<RealTimeSystem>
{
	public RealTimeSystemTests() : base(new RealTimeSystem())
	{
	}
}