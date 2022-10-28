namespace Testably.Abstractions.Tests.RandomSystem.RandomFactory;

// ReSharper disable once UnusedMember.Global
public static class RealRandomSystemTests
{
	// ReSharper disable once UnusedMember.Global
	public sealed class RandomFactoryTests
		: RandomSystemRandomFactoryTests<RealRandomSystem>
	{
		public RandomFactoryTests() : base(new RealRandomSystem())
		{
		}
	}
}