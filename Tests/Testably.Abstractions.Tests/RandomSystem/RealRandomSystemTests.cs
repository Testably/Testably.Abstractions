namespace Testably.Abstractions.Tests.RandomSystem;

// ReSharper disable once UnusedMember.Global
public sealed class RealRandomSystemTests : RandomSystemTests<RealRandomSystem>
{
	public RealRandomSystemTests() : base(new RealRandomSystem())
	{
	}
}