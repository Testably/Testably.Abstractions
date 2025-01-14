#if FEATURE_FILESYSTEM_NET7
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
	[SkippableTheory]
	[AutoData]
	public async Task ReadLinesAsync_Cancelled_ShouldThrowTaskCanceledException(
		string path)
	{
		await FileSystem.File.WriteAllTextAsync(path, "some content");
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			await foreach (string _ in FileSystem.File.ReadLinesAsync(path, cts.Token))
			{
				// do nothing
			}
		});

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[SkippableTheory]
	[AutoData]
	public async Task
		ReadLinesAsync_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
			string path)
	{
		await FileSystem.File.WriteAllTextAsync(path, "some content");
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			await foreach (string _ in FileSystem.File.ReadLinesAsync(path, Encoding.UTF8,
				cts.Token))
			{
				// do nothing
			}
		});

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[SkippableTheory]
	[AutoData]
	public async Task ReadLinesAsync_MissingFile_ShouldThrowFileNotFoundException(
		string path)
	{
		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			await foreach (string _ in FileSystem.File.ReadLinesAsync(path))
			{
				// do nothing
			}
		});

		exception.Should().BeException<FileNotFoundException>(
			$"'{FileSystem.Path.GetFullPath(path)}'",
			hResult: -2147024894);
	}

	[SkippableTheory]
	[AutoData]
	public async Task ReadLinesAsync_ShouldEnumerateLines(string path, string[] lines)
	{
		string contents = string.Join(Environment.NewLine, lines);
		await FileSystem.File.WriteAllTextAsync(path, contents);
		List<string> results = [];

		await foreach (string line in FileSystem.File.ReadLinesAsync(path))
		{
			results.Add(line);
		}

		results.Should().BeEquivalentTo(lines, o => o.WithStrictOrdering());
	}

	[SkippableTheory]
	[ClassData(typeof(TestDataGetEncodingDifference))]
	public async Task ReadLinesAsync_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string specialLine, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		string[] lines = new Fixture().Create<string[]>();
		lines[1] = specialLine;
		string contents = string.Join(Environment.NewLine, lines);
		await FileSystem.File.WriteAllTextAsync(path, contents, writeEncoding);
		List<string> results = [];

		await foreach (string line in FileSystem.File.ReadLinesAsync(path, readEncoding))
		{
			results.Add(line);
		}

		results.Should().NotBeEquivalentTo(lines,
			$"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
		results[0].Should().Be(lines[0]);
	}
}
#endif
