#if FEATURE_FILESYSTEM_ASYNC
using AutoFixture;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class ReadAllLinesAsyncTests
{
	[Theory]
	[AutoData]
	public async Task ReadAllLinesAsync_Cancelled_ShouldThrowTaskCanceledException(
		string path)
	{
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.ReadAllLinesAsync(path, cts.Token));

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[Theory]
	[AutoData]
	public async Task
		ReadAllLinesAsync_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
			string path)
	{
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.ReadAllLinesAsync(path, Encoding.UTF8, cts.Token));

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[Theory]
	[AutoData]
	public async Task ReadAllLinesAsync_MissingFile_ShouldThrowFileNotFoundException(
		string path)
	{
		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.ReadAllLinesAsync(path, TestContext.Current.CancellationToken));

		exception.Should().BeException<FileNotFoundException>(
			$"'{FileSystem.Path.GetFullPath(path)}'",
			hResult: -2147024894);
	}

	[Theory]
	[AutoData]
	public async Task ReadAllLinesAsync_ShouldEnumerateLines(string path, string[] lines)
	{
		string contents = string.Join(Environment.NewLine, lines);
		await FileSystem.File.WriteAllTextAsync(path, contents, TestContext.Current.CancellationToken);

		string[] results = await FileSystem.File.ReadAllLinesAsync(path, TestContext.Current.CancellationToken);

		results.Should().BeEquivalentTo(lines, o => o.WithStrictOrdering());
	}

	[Theory]
	[ClassData(typeof(TestDataGetEncodingDifference))]
	public async Task ReadAllLinesAsync_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string specialLine, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		string[] lines = new Fixture().Create<string[]>();
		lines[1] = specialLine;
		string contents = string.Join(Environment.NewLine, lines);
		await FileSystem.File.WriteAllTextAsync(path, contents, writeEncoding, TestContext.Current.CancellationToken);

		string[] result = await FileSystem.File.ReadAllLinesAsync(path, readEncoding, TestContext.Current.CancellationToken);

		result.Should().NotBeEquivalentTo(lines,
			$"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
		result[0].Should().Be(lines[0]);
	}
}
#endif
