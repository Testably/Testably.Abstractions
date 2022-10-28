namespace Testably.Abstractions.Tests.RandomSystem;

public sealed class RealRandomSystemTests : RandomSystemTests<Abstractions.RealRandomSystem>
{
	public RealRandomSystemTests() : base(new Abstractions.RealRandomSystem())
	{
	}
}