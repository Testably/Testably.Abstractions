using BenchmarkDotNet.Attributes;
using Testably.Abstractions.Testing;
using SioaTesting = System.IO.Abstractions.TestingHelpers;

namespace Testably.Abstractions.Benchmarks;

/// <summary>
///     Recursively enumerates a file system with <see cref="N" /> files in folders of ten, half of them <c>.txt</c>.
/// </summary>
public class EnumerationBenchmarks : BenchmarksBase
{
	private const int FilesPerFolder = 10;

	private SioaTesting.MockFileSystem _systemIOAbstractions = null!;
	private MockFileSystem _testably = null!;

	[Params(10, 1000)] public int N { get; set; }

	[Params("*", "*.txt")] public string Pattern { get; set; } = "*";

	[GlobalSetup]
	public void Setup()
	{
		_testably = new MockFileSystem();
		_systemIOAbstractions = new SioaTesting.MockFileSystem();
		for (int folder = 0; folder < N / FilesPerFolder; folder++)
		{
			_testably.Directory.CreateDirectory($"root/d{folder}");
			_systemIOAbstractions.Directory.CreateDirectory($"root/d{folder}");
			for (int file = 0; file < FilesPerFolder; file++)
			{
				string path = $"root/d{folder}/f{file}.{(file % 2 == 0 ? "txt" : "log")}";
				_testably.File.WriteAllText(path, "");
				_systemIOAbstractions.File.WriteAllText(path, "");
			}
		}
	}

	[Benchmark(Baseline = true)]
	public int Enumerate_Testably()
		=> _testably.Directory.EnumerateFiles("root", Pattern, SearchOption.AllDirectories).Count();

	[Benchmark]
	public int Enumerate_SystemIOAbstractions()
		=> _systemIOAbstractions.Directory.EnumerateFiles("root", Pattern, SearchOption.AllDirectories).Count();
}
