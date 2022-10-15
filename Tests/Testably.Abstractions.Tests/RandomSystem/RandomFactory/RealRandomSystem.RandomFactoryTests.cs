using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.RandomSystem.RandomFactory;

public static partial class RealRandomSystem
{
	// ReSharper disable once UnusedMember.Global
	[SystemTest(nameof(RealRandomSystem))]
	public sealed class RandomFactoryTests : RandomSystemRandomFactoryTests<Abstractions.RandomSystem>
	{
		public RandomFactoryTests() : base(new Abstractions.RandomSystem())
		{
		}
	}
}