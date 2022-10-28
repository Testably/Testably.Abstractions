namespace Testably.Abstractions.Tests.RandomSystem.Guid;

// ReSharper disable once UnusedMember.Global
public static class RealRandomSystemTests
{
	// ReSharper disable once UnusedMember.Global
	public sealed class GuidTests : RandomSystemGuidTests<RealRandomSystem>
	{
		public GuidTests() : base(new RealRandomSystem())
		{
		}
	}
}