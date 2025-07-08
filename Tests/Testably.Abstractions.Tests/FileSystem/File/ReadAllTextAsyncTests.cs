#if FEATURE_FILESYSTEM_ASYNC
using AutoFixture;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class ReadAllTextAsyncTests
{
	[Theory]
	[AutoData]
	public async Task ReadAllTextAsync_Cancelled_ShouldThrowTaskCanceledException(
		string path)
	{
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.ReadAllTextAsync(path, cts.Token));

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[Theory]
	[AutoData]
	public async Task
		ReadAllTextAsync_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
			string path)
	{
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.ReadAllTextAsync(path, Encoding.UTF8, cts.Token));

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[Theory]
	[AutoData]
	public async Task ReadAllTextAsync_MissingFile_ShouldThrowFileNotFoundException(
		string path)
	{
		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.ReadAllTextAsync(path, TestContext.Current.CancellationToken));

		exception.Should().BeException<FileNotFoundException>(
			$"'{FileSystem.Path.GetFullPath(path)}'",
			hResult: -2147024894);
	}

	[Theory]
	[ClassData(typeof(TestDataGetEncodingDifference))]
	public async Task ReadAllTextAsync_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string contents, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		await FileSystem.File.WriteAllTextAsync(path, contents, writeEncoding, TestContext.Current.CancellationToken);

		string result = await FileSystem.File.ReadAllTextAsync(path, readEncoding, TestContext.Current.CancellationToken);

		await That(result).IsNotEqualTo(contents).Because($"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
	}
}
#endif
