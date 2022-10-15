using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.RandomSystem.RandomFactory;

public static partial class MockRandomSystem
{
	// ReSharper disable once UnusedMember.Global
	[SystemTest(nameof(MockRandomSystemTests))]
	public sealed class
		RandomFactoryTests : RandomSystemRandomFactoryTests<RandomSystemMock>
	{
		public RandomFactoryTests() : base(new RandomSystemMock())
		{
		}
	}
}