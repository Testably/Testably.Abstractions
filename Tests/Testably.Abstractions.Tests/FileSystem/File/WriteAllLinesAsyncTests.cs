#if FEATURE_FILESYSTEM_ASYNC
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class WriteAllLinesAsyncTests
{
	[Theory]
	[AutoData]
	public async Task WriteAllLinesAsync_Cancelled_ShouldThrowTaskCanceledException(
		string path, string[] contents)
	{
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.WriteAllLinesAsync(path, contents, cts.Token));

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[Theory]
	[AutoData]
	public async Task
		WriteAllLinesAsync_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
			string path, string[] contents)
	{
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.WriteAllLinesAsync(path, contents, Encoding.UTF8, cts.Token));

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[Theory]
	[AutoData]
	public async Task
		WriteAllLinesAsync_Enumerable_Cancelled_ShouldThrowTaskCanceledException(
			string path, string[] contents)
	{
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.WriteAllLinesAsync(path, contents.AsEnumerable(), cts.Token));

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[Theory]
	[AutoData]
	public async Task
		WriteAllLinesAsync_Enumerable_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
			string path, string[] contents)
	{
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.WriteAllLinesAsync(path, contents.AsEnumerable(),
				Encoding.UTF8, cts.Token));

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[Theory]
	[AutoData]
	public async Task
		WriteAllLinesAsync_Enumerable_PreviousFile_ShouldOverwriteFileWithText(
			string path, string[] contents)
	{
		await FileSystem.File.WriteAllTextAsync(path, "foo", TestContext.Current.CancellationToken);

		await FileSystem.File.WriteAllLinesAsync(path, contents.AsEnumerable(), TestContext.Current.CancellationToken);

		string[] result = await FileSystem.File.ReadAllLinesAsync(path, TestContext.Current.CancellationToken);
		result.Should().BeEquivalentTo(contents, o => o.WithStrictOrdering());
	}

	[Theory]
	[AutoData]
	public async Task WriteAllLinesAsync_Enumerable_ShouldCreateFileWithText(
		string path, string[] contents)
	{
		await FileSystem.File.WriteAllLinesAsync(path, contents.AsEnumerable(), TestContext.Current.CancellationToken);

		string[] result = await FileSystem.File.ReadAllLinesAsync(path, TestContext.Current.CancellationToken);
		result.Should().BeEquivalentTo(contents, o => o.WithStrictOrdering());
	}

	[Theory]
	[AutoData]
	public async Task WriteAllLinesAsync_PreviousFile_ShouldOverwriteFileWithText(
		string path, string[] contents)
	{
		await FileSystem.File.WriteAllTextAsync(path, "foo", TestContext.Current.CancellationToken);

		await FileSystem.File.WriteAllLinesAsync(path, contents, TestContext.Current.CancellationToken);

		string[] result = await FileSystem.File.ReadAllLinesAsync(path, TestContext.Current.CancellationToken);
		result.Should().BeEquivalentTo(contents, o => o.WithStrictOrdering());
	}

	[Theory]
	[AutoData]
	public async Task WriteAllLinesAsync_ShouldCreateFileWithText(
		string path, string[] contents)
	{
		await FileSystem.File.WriteAllLinesAsync(path, contents, TestContext.Current.CancellationToken);

		string[] result = await FileSystem.File.ReadAllLinesAsync(path, TestContext.Current.CancellationToken);
		result.Should().BeEquivalentTo(contents, o => o.WithStrictOrdering());
	}

	[Theory]
	[AutoData]
	public async Task
		WriteAllLinesAsync_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path, string[] contents)
	{
		FileSystem.Directory.CreateDirectory(path);

		async Task Act()
		{
			await FileSystem.File.WriteAllLinesAsync(path, contents, TestContext.Current.CancellationToken);
		}

		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<UnauthorizedAccessException>(
			hResult: -2147024891);
		FileSystem.Should().HaveDirectory(path);
		FileSystem.Should().NotHaveFile(path);
	}

	[Theory]
	[AutoData]
	public async Task
		WriteAllLinesAsync_WhenFileIsHidden_ShouldThrowUnauthorizedAccessException_OnWindows(
			string path, string[] contents)
	{
		Skip.IfNot(Test.RunsOnWindows);

		await FileSystem.File.WriteAllTextAsync(path, "", TestContext.Current.CancellationToken);
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);

		async Task Act()
		{
			await FileSystem.File.WriteAllLinesAsync(path, contents, TestContext.Current.CancellationToken);
		}

		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<UnauthorizedAccessException>(hResult: -2147024891);
	}
}
#endif
