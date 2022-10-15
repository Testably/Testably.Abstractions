using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.RandomSystem.Guid;

public static class MockRandomSystemTests
{
	// ReSharper disable once UnusedMember.Global
	[SystemTest(nameof(Tests.RandomSystem.MockRandomSystemTests))]
	public sealed class GuidTests : RandomSystemGuidTests<RandomSystemMock>
	{
		public GuidTests() : base(new RandomSystemMock())
		{
		}
	}
}