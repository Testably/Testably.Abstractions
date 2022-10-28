namespace Testably.Abstractions.Tests.RandomSystem.Random;

public static class MockRandomSystemTests
{
	public sealed class RandomTests : RandomSystemRandomTests<MockRandomSystem>
	{
		public RandomTests() : base(new MockRandomSystem())
		{
		}
	}
}