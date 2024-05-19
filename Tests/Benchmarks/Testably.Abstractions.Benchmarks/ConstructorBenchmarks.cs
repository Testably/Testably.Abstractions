using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Testably.Abstractions.Testing;
using Testably.Abstractions.Testing.Initializer;
using MockFileSystem = Testably.Abstractions.Testing.MockFileSystem;

namespace Testably.Abstractions.Benchmarks;

[RPlotExporter]
[MemoryDiagnoser]
public class ConstructorBenchmarks
{
	private readonly Dictionary<string, string> _testData = CreateTestData();

	public static Dictionary<string, string> CreateTestData()
	{
		int filesCount = 1000;
		int maxDirectoryDepth = 8;
		return Enumerable.Range(0, filesCount)
			.ToDictionary(
				i => @$"C:\{string.Join(@"\", Enumerable.Range(0, (i % maxDirectoryDepth) + 1).Select(j => j.ToString(CultureInfo.InvariantCulture)))}\{i}.bin",
				i => i.ToString(CultureInfo.InvariantCulture));
	}

	[Benchmark]
	public IFileSystem TestableIO_1000()
		=> new System.IO.Abstractions.TestingHelpers.MockFileSystem(
			_testData.ToDictionary(x => x.Key, x => new MockFileData(x.Value)));

	[Benchmark]
	public IFileSystem TestableIO_Empty()
		=> new System.IO.Abstractions.TestingHelpers.MockFileSystem();

	[Benchmark]
	public IFileSystem Testably_1000() => new MockFileSystem().Initialize()
		.With(_testData.Select(x => new FileDescription(x.Key, x.Value)).ToArray()).FileSystem;

	[Benchmark]
	public IFileSystem Testably_Empty() => new MockFileSystem();
}
