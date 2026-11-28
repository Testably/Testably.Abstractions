using BenchmarkDotNet.Attributes;
using Testably.Abstractions.Testing;
using SioaTesting = System.IO.Abstractions.TestingHelpers;

namespace Testably.Abstractions.Benchmarks;

/// <summary>
///     Seeds a test fixture with <see cref="N" /> files in one directory, each library the way its documentation
///     recommends.
/// </summary>
public class InitializationBenchmarks : BenchmarksBase
{
	private string[] _contents = null!;
	private string[] _fileNames = null!;
	private string[] _paths = null!;

	[Params(10, 100)] public int N { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		_fileNames = Enumerable.Range(0, N).Select(i => $"file{i}.txt").ToArray();
		_paths = _fileNames.Select(name => Path.Combine("project", name)).ToArray();
		_contents = Enumerable.Range(0, N).Select(i => $"content of file {i}").ToArray();
	}

	[Benchmark(Baseline = true)]
	public object Initialize_Testably()
	{
		MockFileSystem fileSystem = new();
		fileSystem.Initialize().WithSubdirectory("project").Initialized(project =>
		{
			for (int i = 0; i < _fileNames.Length; i++)
			{
				string content = _contents[i];
				project.WithFile(_fileNames[i]).Which(f => f.HasStringContent(content));
			}
		});
		return fileSystem;
	}

	[Benchmark]
	public object Initialize_SystemIOAbstractions()
	{
		Dictionary<string, SioaTesting.MockFileData> files = new(_paths.Length);
		for (int i = 0; i < _paths.Length; i++)
		{
			files[_paths[i]] = new SioaTesting.MockFileData(_contents[i]);
		}

		return new SioaTesting.MockFileSystem(files);
	}
}
