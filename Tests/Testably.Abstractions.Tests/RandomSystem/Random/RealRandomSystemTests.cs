using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.RandomSystem.Random;

public static class RealRandomSystemTests
{
	// ReSharper disable once UnusedMember.Global
	[SystemTest(nameof(RealRandomSystemTests))]
	public sealed class RandomTests : RandomSystemRandomTests<Abstractions.RandomSystem>
	{
		public RandomTests() : base(new Abstractions.RandomSystem())
		{
		}
	}
}