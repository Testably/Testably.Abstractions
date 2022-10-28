namespace Testably.Abstractions.Tests.RandomSystem.RandomFactory;

// ReSharper disable once UnusedMember.Global
public static class MockRandomSystemTests
{
	// ReSharper disable once UnusedMember.Global
	public sealed class RandomFactoryTests
		: RandomSystemRandomFactoryTests<MockRandomSystem>
	{
		public RandomFactoryTests() : base(new MockRandomSystem())
		{
		}
	}
}