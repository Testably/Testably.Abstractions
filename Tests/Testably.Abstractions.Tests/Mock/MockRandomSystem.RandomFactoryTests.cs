using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.Mock;

public static partial class MockRandomSystem
{
	// ReSharper disable once UnusedMember.Global
	[SystemTest(nameof(MockRandomSystem))]
	public sealed class
		RandomFactoryTests : RandomSystemRandomFactoryTests<RandomSystemMock>
	{
		public RandomFactoryTests() : base(new RandomSystemMock())
		{
		}
	}
}