#if NET9_0
namespace Testably.Abstractions.Parity.Tests;

[InheritsTests]
// ReSharper disable once UnusedMember.Global
public class Net9ParityTests : ParityTests
{
	public Net9ParityTests()
		: base(new TestHelpers.Parity())
	{
	}
}

#endif
