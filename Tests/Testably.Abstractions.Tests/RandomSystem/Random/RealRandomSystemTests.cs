namespace Testably.Abstractions.Tests.RandomSystem.Random;

// ReSharper disable once UnusedMember.Global
public static class RealRandomSystemTests
{
	// ReSharper disable once UnusedMember.Global
	public sealed class RandomTests : RandomSystemRandomTests<RealRandomSystem>
	{
		public RandomTests() : base(new RealRandomSystem())
		{
		}
	}
}