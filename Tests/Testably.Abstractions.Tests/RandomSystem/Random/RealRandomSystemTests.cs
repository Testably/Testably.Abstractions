namespace Testably.Abstractions.Tests.RandomSystem.Random;

public static class RealRandomSystemTests
{
	public sealed class RandomTests : RandomSystemRandomTests<Abstractions.RealRandomSystem>
	{
		public RandomTests() : base(new Abstractions.RealRandomSystem())
		{
		}
	}
}