namespace Testably.Abstractions.Tests.RandomSystem.Random;

// ReSharper disable once UnusedMember.Global
public static class MockRandomSystemTests
{
	// ReSharper disable once UnusedMember.Global
	public sealed class RandomTests : RandomSystemRandomTests<MockRandomSystem>
	{
		public RandomTests() : base(new MockRandomSystem())
		{
		}
	}
}