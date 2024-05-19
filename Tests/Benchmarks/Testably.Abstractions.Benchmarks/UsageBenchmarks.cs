using BenchmarkDotNet.Attributes;
using System.IO.Abstractions;
using MockFileSystem = Testably.Abstractions.Testing.MockFileSystem;

namespace Testably.Abstractions.Benchmarks;

[RPlotExporter]
[MemoryDiagnoser]
public class UsageBenchmarks
{
	private readonly string _directoryPath = @"C:\l1\l2\l3\l4\l5\l6\l7\l8\l9\l10";
	private readonly string _fileContent = "some file content";
	private readonly string _fileName = "filename.txt";

	private readonly IFileSystem _systemIoFileSystem =
		new System.IO.Abstractions.TestingHelpers.MockFileSystem();

	private readonly IFileSystem _testablyFileSystem = new MockFileSystem();

	[Benchmark]
	public void CreateDirectory_TestableIO()
		=> _systemIoFileSystem.Directory.CreateDirectory(_directoryPath);

	[Benchmark]
	public void CreateDirectory_Testably()
		=> _testablyFileSystem.Directory.CreateDirectory(_directoryPath);

	[Benchmark]
	public void WriteFile_TestableIO()
		=> _systemIoFileSystem.File.WriteAllText(_fileName, _fileContent);

	[Benchmark]
	public void WriteFile_Testably()
		=> _testablyFileSystem.File.WriteAllText(_fileName, _fileContent);
}
