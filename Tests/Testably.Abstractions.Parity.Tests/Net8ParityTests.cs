#if NET8_0
using System.IO;

namespace Testably.Abstractions.Parity.Tests;

[InheritsTests]
// ReSharper disable once UnusedMember.Global
public class Net8ParityTests : ParityTests
{
	public Net8ParityTests()
		: base(new TestHelpers.Parity())
	{
		Parity.File.MissingMethods.Add(
			typeof(File).GetMethod(nameof(File.OpenHandle)));
	}
}

#endif
