using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.RandomSystem.Random;

public static class MockRandomSystemTests
{
	// ReSharper disable once UnusedMember.Global
	[SystemTest(nameof(Tests.RandomSystem.MockRandomSystemTests))]
	public sealed class RandomTests : RandomSystemRandomTests<RandomSystemMock>
	{
		public RandomTests() : base(new RandomSystemMock())
		{
		}
	}
}