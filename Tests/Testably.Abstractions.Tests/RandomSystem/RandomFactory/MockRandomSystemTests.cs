namespace Testably.Abstractions.Tests.RandomSystem.RandomFactory;

public static class MockRandomSystemTests
{
	public sealed class
		RandomFactoryTests : RandomSystemRandomFactoryTests<MockRandomSystem>
	{
		public RandomFactoryTests() : base(new MockRandomSystem())
		{
		}
	}
}