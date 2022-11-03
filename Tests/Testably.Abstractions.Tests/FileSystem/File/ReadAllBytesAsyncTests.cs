#if FEATURE_FILESYSTEM_ASYNC
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ReadAllBytesAsyncTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public async Task ReadAllBytesAsync_Cancelled_ShouldThrowTaskCanceledException(
		string path)
	{
		CancellationTokenSource cts = new();
		cts.Cancel();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.ReadAllBytesAsync(path, cts.Token));

		exception.Should().BeOfType<TaskCanceledException>()
		   .Which.HResult.Should().Be(-2146233029);
	}

	[SkippableTheory]
	[AutoData]
	public async Task ReadAllBytesAsync_MissingFile_ShouldThrowFileNotFoundException(
		string path)
	{
		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.ReadAllBytesAsync(path));

		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.HResult.Should().Be(-2147024894);
		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should()
		   .Contain($"'{FileSystem.Path.GetFullPath(path)}'");
	}

	[SkippableTheory]
	[AutoData]
	public async Task ReadAllBytesAsync_ShouldReturnWrittenBytes(
		byte[] contents, string path)
	{
		await FileSystem.File.WriteAllBytesAsync(path, contents);

		byte[] result = await FileSystem.File.ReadAllBytesAsync(path);

		result.Should().BeEquivalentTo(contents);
	}
}
#endif