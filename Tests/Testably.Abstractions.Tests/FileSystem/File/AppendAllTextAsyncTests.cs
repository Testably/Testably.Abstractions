#if FEATURE_FILESYSTEM_ASYNC
using AutoFixture;
using System.IO;
using System.Text;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable MethodHasAsyncOverload
[FileSystemTests]
public partial class AppendAllTextAsyncTests
{
	[Theory]
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

	[Theory]
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

	[Theory]
	[AutoData]
	public async Task AppendAllTextAsync_ExistingFile_ShouldAppendLinesToFile(
		string path, string previousContents, string contents)
	{
		await FileSystem.File.AppendAllTextAsync(path, previousContents, TestContext.Current.CancellationToken);

		await FileSystem.File.AppendAllTextAsync(path, contents, TestContext.Current.CancellationToken);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(previousContents + contents);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllTextAsync_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string missingPath, string fileName, string contents)
	{
		string filePath = FileSystem.Path.Combine(missingPath, fileName);

		async Task Act()
		{
			await FileSystem.File.AppendAllTextAsync(filePath, contents, TestContext.Current.CancellationToken);
		}

		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<DirectoryNotFoundException>(hResult: -2147024893);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllTextAsync_MissingFile_ShouldCreateFile(
		string path, string contents)
	{
		await FileSystem.File.AppendAllTextAsync(path, contents, TestContext.Current.CancellationToken);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllTextAsync_ShouldNotEndWithNewline(string path)
	{
		string contents = "foo";

		await FileSystem.File.AppendAllTextAsync(path, contents, TestContext.Current.CancellationToken);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task
		AppendAllTextAsync_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path, string contents)
	{
		FileSystem.Directory.CreateDirectory(path);

		async Task Act()
		{
			await FileSystem.File.AppendAllTextAsync(path, contents, TestContext.Current.CancellationToken);
		}

		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<UnauthorizedAccessException>(
			hResult: -2147024891);
		FileSystem.Directory.Exists(path).Should().BeTrue();
		FileSystem.File.Exists(path).Should().BeFalse();
	}

	[Theory]
	[ClassData(typeof(TestDataGetEncodingDifference))]
	public async Task AppendAllTextAsync_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string contents, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		await FileSystem.File.AppendAllTextAsync(path, contents, writeEncoding, TestContext.Current.CancellationToken);

		string[] result = FileSystem.File.ReadAllLines(path, readEncoding);

		result.Should().NotBeEquivalentTo(contents,
			$"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
	}
	
#if FEATURE_FILE_SPAN
	[Theory]
	[AutoData]
	public async Task AppendAllTextAsync_ReadOnlyMemory_Cancelled_ShouldThrowTaskCanceledException(
		string path, string contents)
	{
		using CancellationTokenSource cts = new();
		cts.Cancel();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.AppendAllTextAsync(path, contents.AsMemory(), cts.Token));

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[Theory]
	[AutoData]
	public async Task
		AppendAllTextAsync_ReadOnlyMemory_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
			string path, string contents)
	{
		using CancellationTokenSource cts = new();
		cts.Cancel();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.AppendAllTextAsync(path, contents.AsMemory(), Encoding.UTF8, cts.Token));

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllTextAsync_ReadOnlyMemory_ExistingFile_ShouldAppendLinesToFile(
		string path, string previousContents, string contents)
	{
		await FileSystem.File.AppendAllTextAsync(path, previousContents, TestContext.Current.CancellationToken);

		await FileSystem.File.AppendAllTextAsync(path, contents.AsMemory(), TestContext.Current.CancellationToken);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(previousContents + contents);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllTextAsync_ReadOnlyMemory_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string missingPath, string fileName, string contents)
	{
		string filePath = FileSystem.Path.Combine(missingPath, fileName);

		async Task Act()
		{
			await FileSystem.File.AppendAllTextAsync(filePath, contents.AsMemory(), TestContext.Current.CancellationToken);
		}

		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<DirectoryNotFoundException>(hResult: -2147024893);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllTextAsync_ReadOnlyMemory_MissingFile_ShouldCreateFile(
		string path, string contents)
	{
		await FileSystem.File.AppendAllTextAsync(path, contents.AsMemory(), TestContext.Current.CancellationToken);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllTextAsync_ReadOnlyMemory_ShouldNotEndWithNewline(string path)
	{
		string contents = "foo";

		await FileSystem.File.AppendAllTextAsync(path, contents.AsMemory(), TestContext.Current.CancellationToken);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task
		AppendAllTextAsync_ReadOnlyMemory_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path, string contents)
	{
		FileSystem.Directory.CreateDirectory(path);

		async Task Act()
		{
			await FileSystem.File.AppendAllTextAsync(path, contents.AsMemory(), TestContext.Current.CancellationToken);
		}

		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<UnauthorizedAccessException>(
			hResult: -2147024891);
		FileSystem.Directory.Exists(path).Should().BeTrue();
		FileSystem.File.Exists(path).Should().BeFalse();
	}

	[Theory]
	[ClassData(typeof(TestDataGetEncodingDifference))]
	public async Task AppendAllTextAsync_ReadOnlyMemory_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string contents, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		await FileSystem.File.AppendAllTextAsync(path, contents.AsMemory(), writeEncoding, TestContext.Current.CancellationToken);

		string[] result = FileSystem.File.ReadAllLines(path, readEncoding);

		result.Should().NotBeEquivalentTo(contents,
			$"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
	}
#endif
}
#endif
