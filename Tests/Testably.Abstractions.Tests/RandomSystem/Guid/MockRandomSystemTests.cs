namespace Testably.Abstractions.Tests.RandomSystem.Guid;

public static class MockRandomSystemTests
{
	public sealed class GuidTests : RandomSystemGuidTests<MockRandomSystem>
	{
		public GuidTests() : base(new MockRandomSystem())
		{
		}
	}
}