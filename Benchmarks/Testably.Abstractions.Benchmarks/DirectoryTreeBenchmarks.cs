using System.IO.Abstractions;
using BenchmarkDotNet.Attributes;
using Testably.Abstractions.Testing;
using SioaTesting = System.IO.Abstractions.TestingHelpers;

namespace Testably.Abstractions.Benchmarks;

/// <summary>
///     Moves a directory with 10 subdirectories of 10 files each and moves it back, like code that archives a folder.
/// </summary>
public class DirectoryTreeBenchmarks : BenchmarksBase
{
	private SioaTesting.MockFileSystem _systemIOAbstractions = null!;
	private MockFileSystem _testably = null!;

	[GlobalSetup]
	public void Setup()
	{
		_testably = new MockFileSystem();
		_systemIOAbstractions = new SioaTesting.MockFileSystem();
		foreach (IFileSystem fileSystem in new IFileSystem[] { _testably, _systemIOAbstractions, })
		{
			for (int directory = 0; directory < 10; directory++)
			{
				string path = Path.Combine("source", $"d{directory}");
				fileSystem.Directory.CreateDirectory(path);
				for (int file = 0; file < 10; file++)
				{
					fileSystem.File.WriteAllText(Path.Combine(path, $"f{file}.txt"), "content");
				}
			}
		}
	}

	[Benchmark(Baseline = true)]
	public void MoveTree_Testably()
	{
		_testably.Directory.Move("source", "archive");
		_testably.Directory.Move("archive", "source");
	}

	[Benchmark]
	public void MoveTree_SystemIOAbstractions()
	{
		_systemIOAbstractions.Directory.Move("source", "archive");
		_systemIOAbstractions.Directory.Move("archive", "source");
	}
}
