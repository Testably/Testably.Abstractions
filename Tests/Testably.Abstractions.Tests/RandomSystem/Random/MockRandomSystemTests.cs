namespace Testably.Abstractions.Tests.RandomSystem.Random;

public static class MockRandomSystemTests
{
	public sealed class RandomTests : RandomSystemRandomTests<RandomSystemMock>
	{
		public RandomTests() : base(new RandomSystemMock())
		{
		}
	}
}