#if FEATURE_FILESYSTEM_ASYNC
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable MethodHasAsyncOverload
// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class WriteAllBytesAsyncTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
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

	[SkippableTheory]
	[AutoData]
	public async Task WriteAllBytesAsync_PreviousFile_ShouldOverwriteFileWithBytes(
		string path, byte[] bytes)
	{
		await FileSystem.File.WriteAllBytesAsync(path, Encoding.UTF8.GetBytes("foo"));

		await FileSystem.File.WriteAllBytesAsync(path, bytes);

		byte[] result = FileSystem.File.ReadAllBytes(path);
		result.Should().BeEquivalentTo(bytes);
	}

	[SkippableTheory]
	[AutoData]
	public async Task WriteAllBytesAsync_ShouldCreateFileWithBytes(
		string path, byte[] bytes)
	{
		await FileSystem.File.WriteAllBytesAsync(path, bytes);

		byte[] result = FileSystem.File.ReadAllBytes(path);
		result.Should().BeEquivalentTo(bytes);
	}

	[SkippableTheory]
	[AutoData]
	public async Task WriteAllBytesAsync_WhenBytesAreNull_ShouldThrowArgumentNullException(
		string path)
	{
		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			await FileSystem.File.WriteAllBytesAsync(path, null!);
		});

		exception.Should().BeException<ArgumentNullException>(paramName: "bytes");
	}

	[SkippableTheory]
	[AutoData]
	public async Task
		WriteAllBytesAsync_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path, byte[] bytes)
	{
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			await FileSystem.File.WriteAllBytesAsync(path, bytes);
		});

		exception.Should().BeException<UnauthorizedAccessException>(
			hResult: Test.RunsOnWindows ? -2147024891 : 17);
		FileSystem.Directory.Exists(path).Should().BeTrue();
		FileSystem.File.Exists(path).Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public async Task
		WriteAllTextAsync_WhenFileIsHidden_ShouldThrowUnauthorizedAccessException_OnWindows(
			string path, byte[] bytes)
	{
		Skip.IfNot(Test.RunsOnWindows);

		await FileSystem.File.WriteAllTextAsync(path, null);
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);

		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			await FileSystem.File.WriteAllBytesAsync(path, bytes);
		});

		exception.Should().BeException<UnauthorizedAccessException>(hResult: -2147024891);
	}
}
#endif
