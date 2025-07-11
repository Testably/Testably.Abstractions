#if FEATURE_FILESYSTEM_ASYNC
using AutoFixture;
using NSubstitute.ExceptionExtensions;
using System.IO;
using System.Text;
using System.Threading;

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

		async Task Act() =>
			await FileSystem.File.ReadAllTextAsync(path, cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Theory]
	[AutoData]
	public async Task
		ReadAllTextAsync_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
			string path)
	{
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		async Task Act () =>
			await FileSystem.File.ReadAllTextAsync(path, Encoding.UTF8, cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Theory]
	[AutoData]
	public async Task ReadAllTextAsync_MissingFile_ShouldThrowFileNotFoundException(
		string path)
	{
		async Task Act() =>
			await FileSystem.File.ReadAllTextAsync(path, TestContext.Current.CancellationToken);

		await That(Act).Throws<FileNotFoundException>()
			.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
			.WithHResult(-2147024894);
	}

	[Theory]
	[ClassData(typeof(TestDataGetEncodingDifference))]
	public async Task ReadAllTextAsync_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string contents, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		await FileSystem.File.WriteAllTextAsync(path, contents, writeEncoding, TestContext.Current.CancellationToken);

		string result =
 await FileSystem.File.ReadAllTextAsync(path, readEncoding, TestContext.Current.CancellationToken);

		await That(result).IsNotEqualTo(contents).Because($"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
	}
}
#endif
