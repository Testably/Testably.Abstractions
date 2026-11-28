using System.IO.Abstractions;
using BenchmarkDotNet.Attributes;
using Testably.Abstractions.Testing;
using SioaTesting = System.IO.Abstractions.TestingHelpers;

namespace Testably.Abstractions.Benchmarks;

/// <summary>
///     Inspects the metadata of a file, like code that decides whether a file is new or large enough to process.
/// </summary>
public class FileInfoBenchmarks : BenchmarksBase
{
	private SioaTesting.MockFileSystem _systemIOAbstractions = null!;
	private MockFileSystem _testably = null!;

	[GlobalSetup]
	public void Setup()
	{
		_testably = new MockFileSystem();
		_testably.Directory.CreateDirectory("data");
		_testably.File.WriteAllText(Path.Combine("data", "report.csv"), "a,b,c");
		_systemIOAbstractions = new SioaTesting.MockFileSystem();
		_systemIOAbstractions.Directory.CreateDirectory("data");
		_systemIOAbstractions.File.WriteAllText(Path.Combine("data", "report.csv"), "a,b,c");
	}

	[Benchmark(Baseline = true)]
	public object Inspect_Testably()
		=> Inspect(_testably);

	[Benchmark]
	public object Inspect_SystemIOAbstractions()
		=> Inspect(_systemIOAbstractions);

	private static object Inspect(IFileSystem fileSystem)
	{
		IFileInfo file = fileSystem.FileInfo.New(Path.Combine("data", "report.csv"));
		return (file.Exists, file.Length, file.LastWriteTimeUtc, file.Name, file.DirectoryName);
	}
}
