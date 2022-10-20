namespace Testably.Abstractions.Tests.RandomSystem;

public sealed class RealRandomSystemTests : RandomSystemTests<Abstractions.RandomSystem>
{
	public RealRandomSystemTests() : base(new Abstractions.RandomSystem())
	{
	}
}