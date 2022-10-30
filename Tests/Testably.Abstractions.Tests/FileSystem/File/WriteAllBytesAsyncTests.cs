#if FEATURE_FILESYSTEM_ASYNC
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
		string path, byte[] contents)
	{
		CancellationTokenSource cts = new();
		cts.Cancel();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.WriteAllBytesAsync(path, contents, cts.Token));

		exception.Should().BeOfType<TaskCanceledException>()
		   .Which.Message.Should().Be("A task was canceled.");
	}

	[SkippableTheory]
	[AutoData]
	public async Task WriteAllBytesAsync_PreviousFile_ShouldOverwriteFileWithBytes(
		string path, byte[] contents)
	{
		await FileSystem.File.WriteAllBytesAsync(path, Encoding.UTF8.GetBytes("foo"));

		await FileSystem.File.WriteAllBytesAsync(path, contents);

		byte[] result = FileSystem.File.ReadAllBytes(path);
		result.Should().BeEquivalentTo(contents);
	}

	[SkippableTheory]
	[AutoData]
	public async Task WriteAllBytesAsync_ShouldCreateFileWithBytes(
		string path, byte[] contents)
	{
		await FileSystem.File.WriteAllBytesAsync(path, contents);

		byte[] result = FileSystem.File.ReadAllBytes(path);
		result.Should().BeEquivalentTo(contents);
	}
}
#endif