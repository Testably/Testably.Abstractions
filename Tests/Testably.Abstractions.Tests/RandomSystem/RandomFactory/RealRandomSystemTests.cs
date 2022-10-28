namespace Testably.Abstractions.Tests.RandomSystem.RandomFactory;

public static class RealRandomSystemTests
{
	public sealed class
		RandomFactoryTests : RandomSystemRandomFactoryTests<RealRandomSystem>
	{
		public RandomFactoryTests() : base(new RealRandomSystem())
		{
		}
	}
}