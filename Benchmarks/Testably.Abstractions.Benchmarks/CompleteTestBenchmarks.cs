using BenchmarkDotNet.Attributes;
using Testably.Abstractions.Testing;
using SioaTesting = System.IO.Abstractions.TestingHelpers;

namespace Testably.Abstractions.Benchmarks;

/// <summary>
///     A complete unit test: seed a small project, let the code under test read its configuration, enumerate and
///     concatenate the sources into an output file, back it up and clean up, then check the result.
/// </summary>
/// <remarks>
///     Both libraries run the same calls, so the numbers compare the implementations and not the way the file system
///     is seeded.
/// </remarks>
public class CompleteTestBenchmarks : BenchmarksBase
{
	private const int SourceFiles = 5;

	[Benchmark(Baseline = true)]
	public bool Complete_Testably()
	{
		MockFileSystem fileSystem = new();
		fileSystem.Directory.CreateDirectory("project/src");
		fileSystem.File.WriteAllText("project/config.json", "{\"output\":\"out/bundle.txt\"}");
		for (int i = 0; i < SourceFiles; i++)
		{
			fileSystem.File.WriteAllText($"project/src/file{i}.cs", $"class C{i} {{ }}");
		}

		string config = fileSystem.File.ReadAllText("project/config.json");
		fileSystem.Directory.CreateDirectory("project/out");
		using (StreamWriter writer = fileSystem.File.CreateText("project/out/bundle.txt"))
		{
			foreach (string file in fileSystem.Directory.EnumerateFiles("project/src", "*.cs"))
			{
				writer.WriteLine(fileSystem.File.ReadAllText(file));
			}
		}

		fileSystem.File.Copy("project/out/bundle.txt", "project/out/bundle.bak");
		fileSystem.File.Delete("project/src/file0.cs");

		return config.Length > 0 &&
		       fileSystem.File.Exists("project/out/bundle.bak") &&
		       fileSystem.Directory.GetFiles("project/src").Length == SourceFiles - 1;
	}

	[Benchmark]
	public bool Complete_SystemIOAbstractions()
	{
		SioaTesting.MockFileSystem fileSystem = new();
		fileSystem.Directory.CreateDirectory("project/src");
		fileSystem.File.WriteAllText("project/config.json", "{\"output\":\"out/bundle.txt\"}");
		for (int i = 0; i < SourceFiles; i++)
		{
			fileSystem.File.WriteAllText($"project/src/file{i}.cs", $"class C{i} {{ }}");
		}

		string config = fileSystem.File.ReadAllText("project/config.json");
		fileSystem.Directory.CreateDirectory("project/out");
		using (StreamWriter writer = fileSystem.File.CreateText("project/out/bundle.txt"))
		{
			foreach (string file in fileSystem.Directory.EnumerateFiles("project/src", "*.cs"))
			{
				writer.WriteLine(fileSystem.File.ReadAllText(file));
			}
		}

		fileSystem.File.Copy("project/out/bundle.txt", "project/out/bundle.bak");
		fileSystem.File.Delete("project/src/file0.cs");

		return config.Length > 0 &&
		       fileSystem.File.Exists("project/out/bundle.bak") &&
		       fileSystem.Directory.GetFiles("project/src").Length == SourceFiles - 1;
	}
}
