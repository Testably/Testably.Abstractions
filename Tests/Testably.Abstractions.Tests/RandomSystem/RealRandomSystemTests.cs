using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.RandomSystem;

// ReSharper disable once UnusedMember.Global
[SystemTest(nameof(RealRandomSystemTests))]
public sealed class RealRandomSystemTests : RandomSystemTests<Abstractions.RandomSystem>
{
	public RealRandomSystemTests() : base(new Abstractions.RandomSystem())
	{
	}
}