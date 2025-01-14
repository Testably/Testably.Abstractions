#if FEATURE_FILESYSTEM_ASYNC
using AutoFixture;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable MethodHasAsyncOverload
[FileSystemTests]
public partial class AppendAllTextAsyncTests
{
	[SkippableTheory]
	[AutoData]
	public async Task AppendAllTextAsync_Cancelled_ShouldThrowTaskCanceledException(
		string path, string contents)
	{
		using CancellationTokenSource cts = new();
		cts.Cancel();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.AppendAllTextAsync(path, contents, cts.Token));

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[SkippableTheory]
	[AutoData]
	public async Task
		AppendAllTextAsync_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
			string path, string contents)
	{
		using CancellationTokenSource cts = new();
		cts.Cancel();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.AppendAllTextAsync(path, contents, Encoding.UTF8, cts.Token));

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[SkippableTheory]
	[AutoData]
	public async Task AppendAllTextAsync_ExistingFile_ShouldAppendLinesToFile(
		string path, string previousContents, string contents)
	{
		await FileSystem.File.AppendAllTextAsync(path, previousContents);

		await FileSystem.File.AppendAllTextAsync(path, contents);

		FileSystem.Should().HaveFile(path)
			.Which.HasContent(previousContents + contents);
	}

	[SkippableTheory]
	[AutoData]
	public async Task AppendAllTextAsync_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string missingPath, string fileName, string contents)
	{
		string filePath = FileSystem.Path.Combine(missingPath, fileName);
		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			await FileSystem.File.AppendAllTextAsync(filePath, contents);
		});

		exception.Should().BeException<DirectoryNotFoundException>(hResult: -2147024893);
	}

	[SkippableTheory]
	[AutoData]
	public async Task AppendAllTextAsync_MissingFile_ShouldCreateFile(
		string path, string contents)
	{
		await FileSystem.File.AppendAllTextAsync(path, contents);

		FileSystem.Should().HaveFile(path)
			.Which.HasContent(contents);
	}

	[SkippableTheory]
	[AutoData]
	public async Task AppendAllTextAsync_ShouldNotEndWithNewline(string path)
	{
		string contents = "foo";

		await FileSystem.File.AppendAllTextAsync(path, contents);

		FileSystem.Should().HaveFile(path)
			.Which.HasContent(contents);
	}

	[SkippableTheory]
	[AutoData]
	public async Task
		AppendAllTextAsync_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path, string contents)
	{
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			await FileSystem.File.AppendAllTextAsync(path, contents);
		});

		exception.Should().BeException<UnauthorizedAccessException>(
			hResult: -2147024891);
		FileSystem.Should().HaveDirectory(path);
		FileSystem.Should().NotHaveFile(path);
	}

	[SkippableTheory]
	[ClassData(typeof(TestDataGetEncodingDifference))]
	public async Task AppendAllTextAsync_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string contents, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		await FileSystem.File.AppendAllTextAsync(path, contents, writeEncoding);

		string[] result = FileSystem.File.ReadAllLines(path, readEncoding);

		result.Should().NotBeEquivalentTo(contents,
			$"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
	}
}
#endif
