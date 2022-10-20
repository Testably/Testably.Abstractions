namespace Testably.Abstractions.Tests.RandomSystem.RandomFactory;

public static class MockRandomSystemTests
{
	public sealed class
		RandomFactoryTests : RandomSystemRandomFactoryTests<RandomSystemMock>
	{
		public RandomFactoryTests() : base(new RandomSystemMock())
		{
		}
	}
}