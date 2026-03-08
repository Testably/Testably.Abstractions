#if NET10_0
using System.IO;

namespace Testably.Abstractions.Parity.Tests;

[InheritsTests]
// ReSharper disable once UnusedMember.Global
public class Net10ParityTests : ParityTests
{
	public Net10ParityTests()
		: base(new TestHelpers.Parity())
	{
		Parity.File.MissingMethods.Add(
			typeof(File).GetMethod(nameof(File.OpenHandle)));
	}
}

#endif
