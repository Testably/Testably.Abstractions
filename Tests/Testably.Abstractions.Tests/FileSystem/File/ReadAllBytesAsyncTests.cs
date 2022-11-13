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

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[SkippableTheory]
	[AutoData]
	public async Task ReadAllBytesAsync_MissingFile_ShouldThrowFileNotFoundException(
		string path)
	{
		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.ReadAllBytesAsync(path));
		
		exception.Should().BeException<FileNotFoundException>(
			$"'{FileSystem.Path.GetFullPath(path)}'",
			hResult: -2147024894);
	}

	[SkippableTheory]
	[AutoData]
	public async Task ReadAllBytesAsync_ShouldReturnWrittenBytes(
		byte[] bytes, string path)
	{
		await FileSystem.File.WriteAllBytesAsync(path, bytes);

		byte[] result = await FileSystem.File.ReadAllBytesAsync(path);

		result.Should().BeEquivalentTo(bytes);
	}
}
#endif