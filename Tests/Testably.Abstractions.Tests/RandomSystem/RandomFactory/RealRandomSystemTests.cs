using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.RandomSystem.RandomFactory;

public static class RealRandomSystemTests
{
	// ReSharper disable once UnusedMember.Global
	[SystemTest(nameof(RealRandomSystemTests))]
	public sealed class
		RandomFactoryTests : RandomSystemRandomFactoryTests<Abstractions.RandomSystem>
	{
		public RandomFactoryTests() : base(new Abstractions.RandomSystem())
		{
		}
	}
}