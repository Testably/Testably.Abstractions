namespace Testably.Abstractions.Tests.TimeSystem;

public sealed class RealTimeSystemTests : TimeSystemTests<Abstractions.TimeSystem>
{
	public RealTimeSystemTests() : base(new Abstractions.TimeSystem())
	{
	}
}