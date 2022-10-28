namespace Testably.Abstractions.Tests.RandomSystem.Guid;

// ReSharper disable once UnusedMember.Global
public static class MockRandomSystemTests
{
	// ReSharper disable once UnusedMember.Global
	public sealed class GuidTests : RandomSystemGuidTests<MockRandomSystem>
	{
		public GuidTests() : base(new MockRandomSystem())
		{
		}
	}
}