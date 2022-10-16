#if NET6_0
using System.IO;
using Xunit.Abstractions;

namespace Testably.Abstractions.Parity.Tests;

public class Net6ParityTests : ParityTests
{
	public Net6ParityTests(ITestOutputHelper testOutputHelper)
		: base(new TestHelpers.Parity(), testOutputHelper)
	{
		Parity.File.MissingMethods.Add(
			typeof(File).GetMethod(nameof(File.OpenHandle)));
	}
}

#endif