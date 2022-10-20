namespace Testably.Abstractions.Tests.RandomSystem.Guid;

public static class MockRandomSystemTests
{
	public sealed class GuidTests : RandomSystemGuidTests<RandomSystemMock>
	{
		public GuidTests() : base(new RandomSystemMock())
		{
		}
	}
}