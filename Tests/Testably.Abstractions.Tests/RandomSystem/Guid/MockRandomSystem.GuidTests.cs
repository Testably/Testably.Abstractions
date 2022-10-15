using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.RandomSystem.Guid;

public static class MockRandomSystem
{
	// ReSharper disable once UnusedMember.Global
	[SystemTest(nameof(MockRandomSystemTests))]
	public sealed class GuidTests : RandomSystemGuidTests<RandomSystemMock>
	{
		public GuidTests() : base(new RandomSystemMock())
		{
		}
	}
}