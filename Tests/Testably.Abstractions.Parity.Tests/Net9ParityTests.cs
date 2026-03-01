#if NET9_0
using System.IO;

namespace Testably.Abstractions.Parity.Tests;

[InheritsTests]
// ReSharper disable once UnusedMember.Global
public class Net9ParityTests : ParityTests
{
	public Net9ParityTests()
		: base(new TestHelpers.Parity())
	{
		Parity.File.MissingMethods.Add(
			typeof(File).GetMethod(nameof(File.OpenHandle)));
	}
}

#endif
