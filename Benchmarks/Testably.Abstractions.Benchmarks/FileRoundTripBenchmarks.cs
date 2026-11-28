using BenchmarkDotNet.Attributes;
using Testably.Abstractions.Testing;
using SioaTesting = System.IO.Abstractions.TestingHelpers;

namespace Testably.Abstractions.Benchmarks;

/// <summary>
///     Writes a text file of <see cref="Size" /> characters and reads it back, like code that stores and loads a
///     configuration or a document.
/// </summary>
public class FileRoundTripBenchmarks : BenchmarksBase
{
	private string _content = null!;
	private SioaTesting.MockFileSystem _systemIOAbstractions = null!;
	private MockFileSystem _testably = null!;

	[Params(1024, 65536)] public int Size { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		_content = new string('x', Size);
		_testably = new MockFileSystem();
		_systemIOAbstractions = new SioaTesting.MockFileSystem();
	}

	[Benchmark(Baseline = true)]
	public string RoundTrip_Testably()
	{
		_testably.File.WriteAllText("document.txt", _content);
		return _testably.File.ReadAllText("document.txt");
	}

	[Benchmark]
	public string RoundTrip_SystemIOAbstractions()
	{
		_systemIOAbstractions.File.WriteAllText("document.txt", _content);
		return _systemIOAbstractions.File.ReadAllText("document.txt");
	}
}
