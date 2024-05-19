using BenchmarkDotNet.Attributes;
using System.IO.Abstractions;
using MockFileSystem = Testably.Abstractions.Testing.MockFileSystem;

namespace Testably.Abstractions.Benchmarks;

[RPlotExporter]
[MemoryDiagnoser]
public class CreateDirectoryBenchmarks
{
	private readonly string DirectoryPath = @"C:\l1\l2\l3\l4\l5\l6\l7\l8\l9\l10";

	private readonly IFileSystem SystemIOFileSystem =
		new System.IO.Abstractions.TestingHelpers.MockFileSystem();

	private readonly IFileSystem TestablyFileSystem = new MockFileSystem();

	[Benchmark]
	public void TestableIO_CreateDirectory()
		=> SystemIOFileSystem.Directory.CreateDirectory(DirectoryPath);

	[Benchmark]
	public void Testably_CreateDirectory()
		=> TestablyFileSystem.Directory.CreateDirectory(DirectoryPath);
}
