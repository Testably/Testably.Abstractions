namespace Testably.Abstractions.Tests.TimeSystem;

public sealed class RealTimeSystemTests : TimeSystemTests<Abstractions.RealTimeSystem>
{
	public RealTimeSystemTests() : base(new Abstractions.RealTimeSystem())
	{
	}
}