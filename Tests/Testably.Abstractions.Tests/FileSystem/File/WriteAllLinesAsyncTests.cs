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

		async Task Act() =>
			await FileSystem.File.WriteAllLinesAsync(path, contents, cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Theory]
	[AutoData]
	public async Task
		WriteAllLinesAsync_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
			string path, string[] contents)
	{
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		async Task Act() =>
			await FileSystem.File.WriteAllLinesAsync(path, contents, Encoding.UTF8, cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Theory]
	[AutoData]
	public async Task
		WriteAllLinesAsync_Enumerable_Cancelled_ShouldThrowTaskCanceledException(
			string path, string[] contents)
	{
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		async Task Act() =>
			await FileSystem.File.WriteAllLinesAsync(path, contents.AsEnumerable(), cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Theory]
	[AutoData]
	public async Task
		WriteAllLinesAsync_Enumerable_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
			string path, string[] contents)
	{
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		async Task Act() =>
			await FileSystem.File.WriteAllLinesAsync(path, contents.AsEnumerable(),
				Encoding.UTF8, cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Theory]
	[AutoData]
	public async Task
		WriteAllLinesAsync_Enumerable_PreviousFile_ShouldOverwriteFileWithText(
			string path, string[] contents)
	{
		await FileSystem.File.WriteAllTextAsync(path, "foo", TestContext.Current.CancellationToken);

		await FileSystem.File.WriteAllLinesAsync(path, contents.AsEnumerable(), TestContext.Current.CancellationToken);

		string[] result =
 await FileSystem.File.ReadAllLinesAsync(path, TestContext.Current.CancellationToken);
		await That(result).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllLinesAsync_Enumerable_ShouldCreateFileWithText(
		string path, string[] contents)
	{
		await FileSystem.File.WriteAllLinesAsync(path, contents.AsEnumerable(), TestContext.Current.CancellationToken);

		string[] result =
 await FileSystem.File.ReadAllLinesAsync(path, TestContext.Current.CancellationToken);
		await That(result).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllLinesAsync_PreviousFile_ShouldOverwriteFileWithText(
		string path, string[] contents)
	{
		await FileSystem.File.WriteAllTextAsync(path, "foo", TestContext.Current.CancellationToken);

		await FileSystem.File.WriteAllLinesAsync(path, contents, TestContext.Current.CancellationToken);

		string[] result =
 await FileSystem.File.ReadAllLinesAsync(path, TestContext.Current.CancellationToken);
		await That(result).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllLinesAsync_ShouldCreateFileWithText(
		string path, string[] contents)
	{
		await FileSystem.File.WriteAllLinesAsync(path, contents, TestContext.Current.CancellationToken);

		string[] result =
 await FileSystem.File.ReadAllLinesAsync(path, TestContext.Current.CancellationToken);
		await That(result).IsEqualTo(contents);
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

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
		await That(FileSystem.Directory.Exists(path)).IsTrue();
		await That(FileSystem.File.Exists(path)).IsFalse();
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

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
	}
}
#endif
