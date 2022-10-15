using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.RandomSystem.Random;

public static class MockRandomSystem
{
	// ReSharper disable once UnusedMember.Global
	[SystemTest(nameof(MockRandomSystemTests))]
	public sealed class RandomTests : RandomSystemRandomTests<RandomSystemMock>
	{
		public RandomTests() : base(new RandomSystemMock())
		{
		}
	}
}