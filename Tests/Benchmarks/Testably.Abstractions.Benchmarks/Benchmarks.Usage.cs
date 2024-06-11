using BenchmarkDotNet.Attributes;
using System.IO.Abstractions;
using MockFileSystem = Testably.Abstractions.Testing.MockFileSystem;

namespace Testably.Abstractions.Benchmarks;

public partial class Benchmarks
{
	private readonly string _directoryPath = @"C:\l1\l2\l3\l4\l5\l6\l7\l8\l9\l10";
	private readonly string _fileContent = "some file content";
	private readonly string _fileName = "filename.txt";

	private readonly IFileSystem _fileSystem = new MockFileSystem();

	[Benchmark]
	public void CreateDirectory_Testably()
		=> _fileSystem.Directory.CreateDirectory(_directoryPath);

	[Benchmark]
	public void WriteFile_Testably()
		=> _fileSystem.File.WriteAllText(_fileName, _fileContent);
}
