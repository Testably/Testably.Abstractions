#if NET7_0
using System.IO;
using Xunit.Abstractions;

namespace Testably.Abstractions.Parity.Tests;

// ReSharper disable once UnusedMember.Global
public class Net7ParityTests : ParityTests
{
	public Net7ParityTests(ITestOutputHelper testOutputHelper)
		: base(new TestHelpers.Parity(), testOutputHelper)
	{
		Parity.File.MissingMethods.Add(
			typeof(File).GetMethod(nameof(File.OpenHandle)));
	}
}

#endif