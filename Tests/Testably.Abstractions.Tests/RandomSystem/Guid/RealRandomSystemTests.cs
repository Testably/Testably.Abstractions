namespace Testably.Abstractions.Tests.RandomSystem.Guid;

public static class RealRandomSystemTests
{
	public sealed class GuidTests : RandomSystemGuidTests<Abstractions.RealRandomSystem>
	{
		public GuidTests() : base(new Abstractions.RealRandomSystem())
		{
		}
	}
}