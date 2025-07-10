#if FEATURE_FILESYSTEM_ASYNC
using NSubstitute.ExceptionExtensions;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class ReadAllBytesAsyncTests
{
	[Theory]
	[AutoData]
	public async Task ReadAllBytesAsync_Cancelled_ShouldThrowTaskCanceledException(
		string path)
	{
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		async Task Act() =>
			await FileSystem.File.ReadAllBytesAsync(path, cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Theory]
	[AutoData]
	public async Task ReadAllBytesAsync_MissingFile_ShouldThrowFileNotFoundException(
		string path)
	{
		async Task Act() =>
			await FileSystem.File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken);

		await That(Act).Throws<FileNotFoundException>()
			.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
			.WithHResult(-2147024894);
	}

	[Theory]
	[AutoData]
	public async Task ReadAllBytesAsync_ShouldReturnWrittenBytes(
		byte[] bytes, string path)
	{
		await FileSystem.File.WriteAllBytesAsync(path, bytes, TestContext.Current.CancellationToken);

		byte[] result =
 await FileSystem.File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken);

		await That(result).IsEqualTo(bytes).InAnyOrder();
	}
}
#endif
