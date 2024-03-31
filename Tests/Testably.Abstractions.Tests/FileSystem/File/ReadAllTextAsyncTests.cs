#if FEATURE_FILESYSTEM_ASYNC
using AutoFixture;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ReadAllTextAsyncTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
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

	[SkippableTheory]
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

	[SkippableTheory]
	[AutoData]
	public async Task ReadAllTextAsync_MissingFile_ShouldThrowFileNotFoundException(
		string path)
	{
		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.ReadAllTextAsync(path));

		exception.Should().BeException<FileNotFoundException>(
			$"'{FileSystem.Path.GetFullPath(path)}'",
			hResult: -2147024894);
	}

	[SkippableTheory]
	[ClassData(typeof(TestDataGetEncodingDifference))]
	public async Task ReadAllTextAsync_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string contents, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		await FileSystem.File.WriteAllTextAsync(path, contents, writeEncoding);

		string result = await FileSystem.File.ReadAllTextAsync(path, readEncoding);

		result.Should().NotBe(contents,
			$"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
	}
}
#endif
