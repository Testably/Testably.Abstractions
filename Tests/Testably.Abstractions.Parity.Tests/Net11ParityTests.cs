#if NET11_0
namespace Testably.Abstractions.Parity.Tests;

[InheritsTests]
// ReSharper disable once UnusedMember.Global
public class Net11ParityTests : ParityTests
{
	public Net11ParityTests()
		: base(new TestHelpers.Parity())
	{
	}
}

#endif
