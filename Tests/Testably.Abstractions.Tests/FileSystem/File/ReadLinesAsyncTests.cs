#if FEATURE_FILESYSTEM_NET_7_OR_GREATER
using AutoFixture;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class ReadLinesAsyncTests
{
	[Theory]
	[AutoData]
	public async Task ReadLinesAsync_Cancelled_ShouldThrowTaskCanceledException(
		string path)
	{
		await FileSystem.File.WriteAllTextAsync(path, "some content", TestContext.Current.CancellationToken);
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		async Task Act()
		{
			await foreach (string _ in FileSystem.File.ReadLinesAsync(path, cts.Token))
			{
				// do nothing
			}
		}

		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[Theory]
	[AutoData]
	public async Task
		ReadLinesAsync_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
			string path)
	{
		await FileSystem.File.WriteAllTextAsync(path, "some content", TestContext.Current.CancellationToken);
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

		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[Theory]
	[AutoData]
	public async Task ReadLinesAsync_MissingFile_ShouldThrowFileNotFoundException(
		string path)
	{
		async Task Act()
		{
			await foreach (string _ in FileSystem.File.ReadLinesAsync(path, TestContext.Current.CancellationToken))
			{
				// do nothing
			}
		}

		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<FileNotFoundException>(
			$"'{FileSystem.Path.GetFullPath(path)}'",
			hResult: -2147024894);
	}

	[Theory]
	[AutoData]
	public async Task ReadLinesAsync_ShouldEnumerateLines(string path, string[] lines)
	{
		string contents = string.Join(Environment.NewLine, lines);
		await FileSystem.File.WriteAllTextAsync(path, contents, TestContext.Current.CancellationToken);
		List<string> results = [];

		await foreach (string line in FileSystem.File.ReadLinesAsync(path, TestContext.Current.CancellationToken))
		{
			results.Add(line);
		}

		await That(results).IsEqualTo(lines);
	}

	[Theory]
	[ClassData(typeof(TestDataGetEncodingDifference))]
	public async Task ReadLinesAsync_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string specialLine, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		string[] lines = new Fixture().Create<string[]>();
		lines[1] = specialLine;
		string contents = string.Join(Environment.NewLine, lines);
		await FileSystem.File.WriteAllTextAsync(path, contents, writeEncoding, TestContext.Current.CancellationToken);
		List<string> results = [];

		await foreach (string line in FileSystem.File.ReadLinesAsync(path, readEncoding, TestContext.Current.CancellationToken))
		{
			results.Add(line);
		}

		await That(results).IsNotEqualTo(lines).InAnyOrder();
		await That(results[0]).IsEqualTo(lines[0]);
	}
}
#endif
