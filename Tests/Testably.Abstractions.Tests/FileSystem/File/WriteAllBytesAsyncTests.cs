#if FEATURE_FILESYSTEM_ASYNC
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable MethodHasAsyncOverload
[FileSystemTests]
public partial class WriteAllBytesAsyncTests
{
	[Theory]
	[AutoData]
	public async Task WriteAllBytesAsync_Cancelled_ShouldThrowTaskCanceledException(
		string path, byte[] bytes)
	{
		using CancellationTokenSource cts = new();
		cts.Cancel();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.WriteAllBytesAsync(path, bytes, cts.Token));

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllBytesAsync_PreviousFile_ShouldOverwriteFileWithBytes(
		string path, byte[] bytes)
	{
		await FileSystem.File.WriteAllBytesAsync(path, Encoding.UTF8.GetBytes("foo"), TestContext.Current.CancellationToken);

		await FileSystem.File.WriteAllBytesAsync(path, bytes, TestContext.Current.CancellationToken);

		FileSystem.Should().HaveFile(path)
			.Which.HasContent(bytes);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllBytesAsync_ShouldCreateFileWithBytes(
		string path, byte[] bytes)
	{
		await FileSystem.File.WriteAllBytesAsync(path, bytes, TestContext.Current.CancellationToken);

		FileSystem.Should().HaveFile(path)
			.Which.HasContent(bytes);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllBytesAsync_WhenBytesAreNull_ShouldThrowArgumentNullException(
		string path)
	{
		async Task Act()
		{
			await FileSystem.File.WriteAllBytesAsync(path, null!, TestContext.Current.CancellationToken);
		}

		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<ArgumentNullException>(paramName: "bytes");
	}

	[Theory]
	[AutoData]
	public async Task
		WriteAllBytesAsync_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path, byte[] bytes)
	{
		FileSystem.Directory.CreateDirectory(path);

		async Task Act()
		{
			await FileSystem.File.WriteAllBytesAsync(path, bytes, TestContext.Current.CancellationToken);
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
		WriteAllTextAsync_WhenFileIsHidden_ShouldThrowUnauthorizedAccessException_OnWindows(
			string path, byte[] bytes)
	{
		Skip.IfNot(Test.RunsOnWindows);

		await FileSystem.File.WriteAllTextAsync(path, null, TestContext.Current.CancellationToken);
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);

		async Task Act()
		{
			await FileSystem.File.WriteAllBytesAsync(path, bytes, TestContext.Current.CancellationToken);
		}

		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<UnauthorizedAccessException>(hResult: -2147024891);
	}
}
#endif
