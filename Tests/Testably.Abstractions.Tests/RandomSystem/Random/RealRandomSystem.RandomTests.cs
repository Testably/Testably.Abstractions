using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.RandomSystem.Random;

public static partial class RealRandomSystem
{
	// ReSharper disable once UnusedMember.Global
	[SystemTest(nameof(RealRandomSystem))]
	public sealed class RandomTests : RandomSystemRandomTests<Abstractions.RandomSystem>
	{
		public RandomTests() : base(new Abstractions.RandomSystem())
		{
		}
	}
}