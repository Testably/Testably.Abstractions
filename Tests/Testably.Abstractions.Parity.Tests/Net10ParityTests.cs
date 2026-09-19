#if NET10_0
namespace Testably.Abstractions.Parity.Tests;

[InheritsTests]
// ReSharper disable once UnusedMember.Global
public class Net10ParityTests : ParityTests
{
	public Net10ParityTests()
		: base(new TestHelpers.Parity())
	{
	}
}

#endif
