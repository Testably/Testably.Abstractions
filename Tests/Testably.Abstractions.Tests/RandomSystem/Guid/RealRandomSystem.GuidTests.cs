using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.RandomSystem.Guid;

public static class RealRandomSystem
{
	// ReSharper disable once UnusedMember.Global
	[SystemTest(nameof(RealRandomSystem))]
	public sealed class GuidTests : RandomSystemGuidTests<Abstractions.RandomSystem>
	{
		public GuidTests() : base(new Abstractions.RandomSystem())
		{
		}
	}
}