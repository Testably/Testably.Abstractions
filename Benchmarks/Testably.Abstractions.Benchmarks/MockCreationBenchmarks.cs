using BenchmarkDotNet.Attributes;
using Testably.Abstractions.Testing;
using SioaTesting = System.IO.Abstractions.TestingHelpers;

namespace Testably.Abstractions.Benchmarks;

/// <summary>
///     The fixed cost every test pays: creating an empty mock file system.
/// </summary>
public class MockCreationBenchmarks : BenchmarksBase
{
	[Benchmark(Baseline = true)]
	public object CreateMock_Testably()
		=> new MockFileSystem();

	[Benchmark]
	public object CreateMock_SystemIOAbstractions()
		=> new SioaTesting.MockFileSystem();
}
