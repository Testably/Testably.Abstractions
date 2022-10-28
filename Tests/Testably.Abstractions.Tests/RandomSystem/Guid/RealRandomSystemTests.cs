namespace Testably.Abstractions.Tests.RandomSystem.Guid;

public static class RealRandomSystemTests
{
	public sealed class GuidTests : RandomSystemGuidTests<RealRandomSystem>
	{
		public GuidTests() : base(new RealRandomSystem())
		{
		}
	}
}