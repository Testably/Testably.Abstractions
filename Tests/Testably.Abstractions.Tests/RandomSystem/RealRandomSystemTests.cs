namespace Testably.Abstractions.Tests.RandomSystem;

public sealed class RealRandomSystemTests : RandomSystemTests<RealRandomSystem>
{
	public RealRandomSystemTests() : base(new RealRandomSystem())
	{
	}
}