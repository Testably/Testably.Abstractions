namespace Testably.Abstractions.Tests.RandomSystem.Random;

public static class RealRandomSystemTests
{
	public sealed class RandomTests : RandomSystemRandomTests<RealRandomSystem>
	{
		public RandomTests() : base(new RealRandomSystem())
		{
		}
	}
}