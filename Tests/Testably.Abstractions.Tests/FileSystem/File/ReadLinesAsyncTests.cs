#if FEATURE_FILESYSTEM_NET_7_OR_GREATER
using AutoFixture;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public class ReadLinesAsyncTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task ReadLinesAsync_Cancelled_ShouldThrowTaskCanceledException(
		string path)
	{
		await FileSystem.File.WriteAllTextAsync(path, "some content", CancellationToken);
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		async Task Act()
		{
			await foreach (string _ in FileSystem.File.ReadLinesAsync(path, cts.Token))
			{
				// do nothing
			}
		}

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Test]
	[AutoArguments]
	public async Task
		ReadLinesAsync_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
			string path)
	{
		await FileSystem.File.WriteAllTextAsync(path, "some content", CancellationToken);
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		async Task Act()
		{
			await foreach (string _ in FileSystem.File.ReadLinesAsync(path, Encoding.UTF8,
				cts.Token))
			{
				// do nothing
			}
		}

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Test]
	[AutoArguments]
	public async Task ReadLinesAsync_MissingFile_ShouldThrowFileNotFoundException(
		string path)
	{
		async Task Act()
		{
			await foreach (string _ in FileSystem.File.ReadLinesAsync(path, CancellationToken))
			{
				// do nothing
			}
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
			.WithHResult(-2147024894);
	}

	[Test]
	[AutoArguments]
	public async Task ReadLinesAsync_ShouldEnumerateLines(string path, string[] lines)
	{
		string contents = string.Join(Environment.NewLine, lines);
		await FileSystem.File.WriteAllTextAsync(path, contents, CancellationToken);
		List<string> results = [];

		await foreach (string line in FileSystem.File.ReadLinesAsync(path, CancellationToken))
		{
			results.Add(line);
		}

		await That(results).IsEqualTo(lines);
	}

	[Test]
	[MethodDataSource(typeof(TestData), nameof(TestData.GetEncodingDifference))]
	public async Task ReadLinesAsync_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string specialLine, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		string[] lines = new Fixture().Create<string[]>();
		lines[1] = specialLine;
		string contents = string.Join(Environment.NewLine, lines);
		await FileSystem.File.WriteAllTextAsync(path, contents, writeEncoding, CancellationToken);
		List<string> results = [];

		await foreach (string line in FileSystem.File.ReadLinesAsync(path, readEncoding, CancellationToken))
		{
			results.Add(line);
		}

		await That(results).IsNotEqualTo(lines).InAnyOrder();
		await That(results[0]).IsEqualTo(lines[0]);
	}
}
#endif
