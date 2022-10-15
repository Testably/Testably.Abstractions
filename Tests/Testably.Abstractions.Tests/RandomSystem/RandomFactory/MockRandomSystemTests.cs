using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.RandomSystem.RandomFactory;

public static class MockRandomSystemTests
{
	// ReSharper disable once UnusedMember.Global
	[SystemTest(nameof(Tests.RandomSystem.MockRandomSystemTests))]
	public sealed class
		RandomFactoryTests : RandomSystemRandomFactoryTests<RandomSystemMock>
	{
		public RandomFactoryTests() : base(new RandomSystemMock())
		{
		}
	}
}