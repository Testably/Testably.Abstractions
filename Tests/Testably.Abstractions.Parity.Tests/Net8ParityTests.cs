#if NET8_0
namespace Testably.Abstractions.Parity.Tests;

[InheritsTests]
// ReSharper disable once UnusedMember.Global
public class Net8ParityTests : ParityTests
{
	public Net8ParityTests()
		: base(new TestHelpers.Parity())
	{
	}
}

#endif
