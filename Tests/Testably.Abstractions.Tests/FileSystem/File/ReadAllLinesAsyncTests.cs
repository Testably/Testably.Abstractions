#if FEATURE_FILESYSTEM_ASYNC
using AutoFixture;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public class ReadAllLinesAsyncTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task ReadAllLinesAsync_Cancelled_ShouldThrowTaskCanceledException(
		string path)
	{
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		async Task Act() =>
			await FileSystem.File.ReadAllLinesAsync(path, cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Test]
	[AutoArguments]
	public async Task
		ReadAllLinesAsync_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
			string path)
	{
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		async Task Act() =>
			await FileSystem.File.ReadAllLinesAsync(path, Encoding.UTF8, cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Test]
	[AutoArguments]
	public async Task ReadAllLinesAsync_MissingFile_ShouldThrowFileNotFoundException(
		string path)
	{
		async Task Act() =>
			await FileSystem.File.ReadAllLinesAsync(path, CancellationToken);

		await That(Act).Throws<FileNotFoundException>()
			.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
			.WithHResult(-2147024894);
	}

	[Test]
	[AutoArguments]
	public async Task ReadAllLinesAsync_ShouldEnumerateLines(string path, string[] lines)
	{
		string contents = string.Join(Environment.NewLine, lines);
		await FileSystem.File.WriteAllTextAsync(path, contents, CancellationToken);

		string[] results =
 await FileSystem.File.ReadAllLinesAsync(path, CancellationToken);

		await That(results).IsEqualTo(lines);
	}

	[Test]
	[MethodDataSource(typeof(TestData), nameof(TestData.GetEncodingDifference))]
	public async Task ReadAllLinesAsync_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string specialLine, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		string[] lines = new Fixture().Create<string[]>();
		lines[1] = specialLine;
		string contents = string.Join(Environment.NewLine, lines);
		await FileSystem.File.WriteAllTextAsync(path, contents, writeEncoding, CancellationToken);

		string[] result =
 await FileSystem.File.ReadAllLinesAsync(path, readEncoding, CancellationToken);

		await That(result).IsNotEqualTo(lines).InAnyOrder();
		await That(result[0]).IsEqualTo(lines[0]);
	}
}
#endif
