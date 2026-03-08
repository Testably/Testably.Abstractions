#if FEATURE_FILESYSTEM_ASYNC
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable MethodHasAsyncOverload
[FileSystemTests]
public class WriteAllBytesAsyncTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task WriteAllBytesAsync_Cancelled_ShouldThrowTaskCanceledException(
		string path, byte[] bytes)
	{
		using CancellationTokenSource cts = new();
		cts.Cancel();

		async Task Act() =>
			await FileSystem.File.WriteAllBytesAsync(path, bytes, cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Test]
	[AutoArguments]
	public async Task WriteAllBytesAsync_PreviousFile_ShouldOverwriteFileWithBytes(
		string path, byte[] bytes)
	{
		await FileSystem.File.WriteAllBytesAsync(path, Encoding.UTF8.GetBytes("foo"), CancellationToken);

		await FileSystem.File.WriteAllBytesAsync(path, bytes, CancellationToken);

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(bytes);
	}

	[Test]
	[AutoArguments]
	public async Task WriteAllBytesAsync_ShouldCreateFileWithBytes(
		string path, byte[] bytes)
	{
		await FileSystem.File.WriteAllBytesAsync(path, bytes, CancellationToken);

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(bytes);
	}

	[Test]
	[AutoArguments]
	public async Task WriteAllBytesAsync_WhenBytesAreNull_ShouldThrowArgumentNullException(
		string path)
	{
		async Task Act()
		{
			await FileSystem.File.WriteAllBytesAsync(path, null!, CancellationToken);
		}

		await That(Act).Throws<ArgumentNullException>().WithParamName("bytes");
	}

	[Test]
	[AutoArguments]
	public async Task
		WriteAllBytesAsync_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path, byte[] bytes)
	{
		FileSystem.Directory.CreateDirectory(path);

		async Task Act()
		{
			await FileSystem.File.WriteAllBytesAsync(path, bytes, CancellationToken);
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
		await That(FileSystem.Directory.Exists(path)).IsTrue();
		await That(FileSystem.File.Exists(path)).IsFalse();
	}

	[Test]
	[AutoArguments]
	public async Task
		WriteAllTextAsync_WhenFileIsHidden_ShouldThrowUnauthorizedAccessException_OnWindows(
			string path, byte[] bytes)
	{
		Skip.IfNot(Test.RunsOnWindows);

		await FileSystem.File.WriteAllTextAsync(path, "", CancellationToken);
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);

		async Task Act()
		{
			await FileSystem.File.WriteAllBytesAsync(path, bytes, CancellationToken);
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
	}
	
#if FEATURE_FILE_SPAN
	[Test]
	[AutoArguments]
	public async Task WriteAllBytesAsync_ReadOnlyMemory_Cancelled_ShouldThrowTaskCanceledException(
		string path, byte[] bytes)
	{
		using CancellationTokenSource cts = new();
		cts.Cancel();

		async Task Act() =>
			await FileSystem.File.WriteAllBytesAsync(path, bytes.AsMemory(), cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Test]
	[AutoArguments]
	public async Task WriteAllBytesAsync_ReadOnlyMemory_PreviousFile_ShouldOverwriteFileWithBytes(
		string path, byte[] bytes)
	{
		await FileSystem.File.WriteAllBytesAsync(path, Encoding.UTF8.GetBytes("foo"), CancellationToken);

		await FileSystem.File.WriteAllBytesAsync(path, bytes.AsMemory(), CancellationToken);

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(bytes);
	}

	[Test]
	[AutoArguments]
	public async Task WriteAllBytesAsync_ReadOnlyMemory_ShouldCreateFileWithBytes(
		string path, byte[] bytes)
	{
		await FileSystem.File.WriteAllBytesAsync(path, bytes.AsMemory(), CancellationToken);

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(bytes);
	}

	[Test]
	[AutoArguments]
	public async Task
		WriteAllBytesAsync_ReadOnlyMemory_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path, byte[] bytes)
	{
		FileSystem.Directory.CreateDirectory(path);

		async Task Act()
		{
			await FileSystem.File.WriteAllBytesAsync(path, bytes.AsMemory(), CancellationToken);
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
		await That(FileSystem.Directory.Exists(path)).IsTrue();
		await That(FileSystem.File.Exists(path)).IsFalse();
	}

	[Test]
	[AutoArguments]
	public async Task
		WriteAllTextAsync_ReadOnlyMemory_WhenFileIsHidden_ShouldThrowUnauthorizedAccessException_OnWindows(
			string path, byte[] bytes)
	{
		Skip.IfNot(Test.RunsOnWindows);

		await FileSystem.File.WriteAllTextAsync(path, "", CancellationToken);
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);

		async Task Act()
		{
			await FileSystem.File.WriteAllBytesAsync(path, bytes.AsMemory(), CancellationToken);
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
	}
#endif
}
#endif
