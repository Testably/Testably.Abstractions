namespace Testably.Abstractions.Tests.RandomSystem.Random;

public static class RealRandomSystemTests
{
	public sealed class RandomTests : RandomSystemRandomTests<Abstractions.RandomSystem>
	{
		public RandomTests() : base(new Abstractions.RandomSystem())
		{
		}
	}
}