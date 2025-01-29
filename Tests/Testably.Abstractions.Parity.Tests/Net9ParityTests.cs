#if NET9_0
using System.IO;

namespace Testably.Abstractions.Parity.Tests;

// ReSharper disable once UnusedMember.Global
public class Net9ParityTests : ParityTests
{
	public Net9ParityTests(ITestOutputHelper testOutputHelper)
		: base(new TestHelpers.Parity(), testOutputHelper)
	{
		Parity.File.MissingMethods.Add(
			typeof(File).GetMethod(nameof(File.OpenHandle)));
	}
}

#endif
