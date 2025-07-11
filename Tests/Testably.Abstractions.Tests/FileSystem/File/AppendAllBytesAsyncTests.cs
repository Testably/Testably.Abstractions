#if FEATURE_FILE_SPAN
using System.IO;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable MethodHasAsyncOverload
[FileSystemTests]
public partial class AppendAllBytesAsyncTests
{
	[Theory]
	[AutoData]
	public async Task AppendAllBytesAsync_Cancelled_ShouldThrowTaskCanceledException(
		string path, byte[] bytes)
	{
		using CancellationTokenSource cts = new();
		cts.Cancel();

		async Task Act() =>
			await FileSystem.File.AppendAllBytesAsync(path, bytes, cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllBytesAsync_ExistingFile_ShouldAppendLinesToFile(
		string path, byte[] previousBytes, byte[] bytes)
	{
		await FileSystem.File.AppendAllBytesAsync(path, previousBytes,
			TestContext.Current.CancellationToken);

		await FileSystem.File.AppendAllBytesAsync(path, bytes,
			TestContext.Current.CancellationToken);

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo([..previousBytes, ..bytes]);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllBytesAsync_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string missingPath, string fileName, byte[] bytes)
	{
		string filePath = FileSystem.Path.Combine(missingPath, fileName);

		async Task Act()
		{
			await FileSystem.File.AppendAllBytesAsync(filePath, bytes,
				TestContext.Current.CancellationToken);
		}

		await That(Act).Throws<DirectoryNotFoundException>().WithHResult(-2147024893);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllBytesAsync_MissingFile_ShouldCreateFile(
		string path, byte[] bytes)
	{
		await FileSystem.File.AppendAllBytesAsync(path, bytes,
			TestContext.Current.CancellationToken);

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(bytes);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllBytesAsync_ReadOnlyMemory_Cancelled_ShouldThrowTaskCanceledException(
		string path, byte[] bytes)
	{
		using CancellationTokenSource cts = new();
		cts.Cancel();

		async Task Act() =>
			await FileSystem.File.AppendAllBytesAsync(path, bytes.AsMemory(), cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllBytesAsync_ReadOnlyMemory_ExistingFile_ShouldAppendLinesToFile(
		string path, byte[] previousBytes, byte[] bytes)
	{
		await FileSystem.File.AppendAllBytesAsync(path, previousBytes,
			TestContext.Current.CancellationToken);

		await FileSystem.File.AppendAllBytesAsync(path, bytes.AsMemory(),
			TestContext.Current.CancellationToken);

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo([..previousBytes, ..bytes]);
	}

	[Theory]
	[AutoData]
	public async Task
		AppendAllBytesAsync_ReadOnlyMemory_MissingDirectory_ShouldThrowDirectoryNotFoundException(
			string missingPath, string fileName, byte[] bytes)
	{
		string filePath = FileSystem.Path.Combine(missingPath, fileName);

		async Task Act()
		{
			await FileSystem.File.AppendAllBytesAsync(filePath, bytes.AsMemory(),
				TestContext.Current.CancellationToken);
		}

		await That(Act).Throws<DirectoryNotFoundException>().WithHResult(-2147024893);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllBytesAsync_ReadOnlyMemory_MissingFile_ShouldCreateFile(
		string path, byte[] bytes)
	{
		await FileSystem.File.AppendAllBytesAsync(path, bytes.AsMemory(),
			TestContext.Current.CancellationToken);

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(bytes);
	}

	[Theory]
	[AutoData]
	public async Task
		AppendAllBytesAsync_ReadOnlyMemory_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path, byte[] bytes)
	{
		FileSystem.Directory.CreateDirectory(path);

		async Task Act()
		{
			await FileSystem.File.AppendAllBytesAsync(path, bytes.AsMemory(),
				TestContext.Current.CancellationToken);
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
		await That(FileSystem.Directory.Exists(path)).IsTrue();
		await That(FileSystem.File.Exists(path)).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task
		AppendAllBytesAsync_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path, byte[] bytes)
	{
		FileSystem.Directory.CreateDirectory(path);

		async Task Act()
		{
			await FileSystem.File.AppendAllBytesAsync(path, bytes,
				TestContext.Current.CancellationToken);
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
		await That(FileSystem.Directory.Exists(path)).IsTrue();
		await That(FileSystem.File.Exists(path)).IsFalse();
	}
}
#endif
