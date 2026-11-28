using System.IO.Abstractions;
using BenchmarkDotNet.Attributes;
using Testably.Abstractions.Testing;
using SioaTesting = System.IO.Abstractions.TestingHelpers;

namespace Testably.Abstractions.Benchmarks;

/// <summary>
///     Writes 64 KB through a file stream in chunks of 4 KB and reads it back the same way, like code that copies or
///     hashes a file.
/// </summary>
public class FileStreamBenchmarks : BenchmarksBase
{
	private const int ChunkSize = 4096;
	private const int ChunkCount = 16;

	private readonly byte[] _chunk = new byte[ChunkSize];
	private SioaTesting.MockFileSystem _systemIOAbstractions = null!;
	private MockFileSystem _testably = null!;

	[GlobalSetup]
	public void Setup()
	{
		_testably = new MockFileSystem();
		_systemIOAbstractions = new SioaTesting.MockFileSystem();
	}

	[Benchmark(Baseline = true)]
	public long Chunks_Testably()
		=> WriteAndRead(_testably);

	[Benchmark]
	public long Chunks_SystemIOAbstractions()
		=> WriteAndRead(_systemIOAbstractions);

	private long WriteAndRead(IFileSystem fileSystem)
	{
		using (Stream stream = fileSystem.FileStream.New("data.bin", FileMode.Create))
		{
			for (int i = 0; i < ChunkCount; i++)
			{
				stream.Write(_chunk, 0, ChunkSize);
			}
		}

		long total = 0;
		using (Stream stream = fileSystem.FileStream.New("data.bin", FileMode.Open))
		{
			int read;
			while ((read = stream.Read(_chunk, 0, ChunkSize)) > 0)
			{
				total += read;
			}
		}

		return total;
	}
}
