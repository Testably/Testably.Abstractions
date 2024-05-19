using BenchmarkDotNet.Attributes;
using System.IO.Abstractions;
using MockFileSystem = Testably.Abstractions.Testing.MockFileSystem;

namespace Testably.Abstractions.Benchmarks;

[RPlotExporter]
[MemoryDiagnoser]
public class CreateFileBenchmarks
{
	private readonly string FileContent = "some file content";
	private readonly string FileName = "filename.txt";

	private readonly IFileSystem SystemIOFileSystem =
		new System.IO.Abstractions.TestingHelpers.MockFileSystem();

	private readonly IFileSystem TestablyFileSystem = new MockFileSystem();

	[Benchmark]
	public void TestableIO_CreateDirectory()
		=> SystemIOFileSystem.File.WriteAllText(FileName, FileContent);

	[Benchmark]
	public void Testably_CreateDirectory()
		=> TestablyFileSystem.File.WriteAllText(FileName, FileContent);
}
