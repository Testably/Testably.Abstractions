using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions;
using System.Linq;
using Testably.Abstractions.Testing;
using Testably.Abstractions.Testing.Initializer;
using MockFileSystem = Testably.Abstractions.Testing.MockFileSystem;

namespace Testably.Abstractions.Benchmarks;

public partial class Benchmarks
{
	private readonly Dictionary<string, string> _testData = CreateTestData();

	[Benchmark]
	public IFileSystem ConstructorEmpty_Testably() => new MockFileSystem();

	[Benchmark]
	public IFileSystem ConstructorWithInitialization_1000_Testably() => new MockFileSystem()
		.Initialize()
		.With(_testData.Select(x => new FileDescription(x.Key, x.Value)).ToArray()).FileSystem;

	private static Dictionary<string, string> CreateTestData()
	{
		int filesCount = 1000;
		int maxDirectoryDepth = 8;
		return Enumerable.Range(0, filesCount)
			.ToDictionary(
				i => @$"C:\{string.Join(@"\", Enumerable.Range(0, (i % maxDirectoryDepth) + 1).Select(j => j.ToString(CultureInfo.InvariantCulture)))}\{i}.bin",
				i => i.ToString(CultureInfo.InvariantCulture));
	}
}
