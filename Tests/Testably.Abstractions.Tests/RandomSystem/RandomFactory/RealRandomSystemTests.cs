namespace Testably.Abstractions.Tests.RandomSystem.RandomFactory;

public static class RealRandomSystemTests
{
	public sealed class
		RandomFactoryTests : RandomSystemRandomFactoryTests<Abstractions.RealRandomSystem>
	{
		public RandomFactoryTests() : base(new Abstractions.RealRandomSystem())
		{
		}
	}
}