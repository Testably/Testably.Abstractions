#if FEATURE_FILESYSTEM_RANDOMACCESS
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.RandomAccess;

[FileSystemTests]
public class ValidationTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task Read_WithDisposedHandleAndNegativeOffset_ShouldThrowObjectDisposedException(
		string path)
	{
		SafeFileHandle handle = OpenDisposedHandle(path);

		void Act() => FileSystem.RandomAccess.Read(handle, new byte[1], -1);

		await That(Act).Throws<ObjectDisposedException>()
			.Because("the handle is validated before the offset");
	}

	[Test]
	public async Task Read_WithNullHandleAndNullBuffers_ShouldThrowArgumentNullExceptionForHandle()
	{
		void Act() => FileSystem.RandomAccess.Read(null!, (IReadOnlyList<Memory<byte>>)null!, 0);

		await That(Act).ThrowsExactly<ArgumentNullException>().WithParamName("handle")
			.Because("the handle is validated before the buffers");
	}

	[Test]
	[AutoArguments]
	public async Task Read_WithNullBuffersAndNegativeOffset_ShouldThrowArgumentOutOfRangeException(
		string path)
	{
		FileSystem.File.WriteAllBytes(path, [1, 2, 3,]);
		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		void Act() => FileSystem.RandomAccess.Read(handle, (IReadOnlyList<Memory<byte>>)null!, -1);

		await That(Act).Throws<ArgumentOutOfRangeException>().WithParamName("fileOffset")
			.Because("the offset is validated before the buffers");
	}

	[Test]
	[AutoArguments]
	public async Task Read_WithWriteOnlyHandleAndNullBuffers_ShouldThrowArgumentNullException(
		string path)
	{
		FileSystem.File.WriteAllBytes(path, [1, 2, 3,]);
		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write);

		void Act() => FileSystem.RandomAccess.Read(handle, (IReadOnlyList<Memory<byte>>)null!, 0);

		await That(Act).ThrowsExactly<ArgumentNullException>().WithParamName("buffers")
			.Because("the arguments are validated before the access to the file is checked");
	}

	[Test]
	[AutoArguments]
	public async Task Write_WithDisposedHandleAndNegativeOffset_ShouldThrowObjectDisposedException(
		string path)
	{
		SafeFileHandle handle = OpenDisposedHandle(path);

		void Act() => FileSystem.RandomAccess.Write(handle, new byte[] { 1, }, -1);

		await That(Act).Throws<ObjectDisposedException>()
			.Because("the handle is validated before the offset");
	}

	[Test]
	public async Task Write_WithNullHandleAndNullBuffers_ShouldThrowArgumentNullExceptionForHandle()
	{
		void Act() => FileSystem.RandomAccess.Write(null!,
			(IReadOnlyList<ReadOnlyMemory<byte>>)null!, 0);

		await That(Act).ThrowsExactly<ArgumentNullException>().WithParamName("handle")
			.Because("the handle is validated before the buffers");
	}

	[Test]
	[AutoArguments]
	public async Task Write_WithNullBuffersAndNegativeOffset_ShouldThrowArgumentOutOfRangeException(
		string path)
	{
		FileSystem.File.WriteAllBytes(path, [1, 2, 3,]);
		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write);

		void Act() => FileSystem.RandomAccess.Write(handle,
			(IReadOnlyList<ReadOnlyMemory<byte>>)null!, -1);

		await That(Act).Throws<ArgumentOutOfRangeException>().WithParamName("fileOffset")
			.Because("the offset is validated before the buffers");
	}

	[Test]
	public async Task ReadAsync_WithNullHandle_WhenCancelled_ShouldThrowArgumentNullException()
	{
		using CancellationTokenSource cts = new();
		cts.Cancel();

		void Act() => _ = FileSystem.RandomAccess.ReadAsync(null!, new byte[1], 0, cts.Token);

		await That(Act).ThrowsExactly<ArgumentNullException>().WithParamName("handle")
			.Because("the arguments are validated synchronously before the cancellation is observed");
	}

	[Test]
	[AutoArguments]
	public async Task ReadAsync_WithNullBuffers_WhenCancelled_ShouldThrowArgumentNullException(
		string path)
	{
		FileSystem.File.WriteAllBytes(path, [1, 2, 3,]);
		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);
		using CancellationTokenSource cts = new();
		cts.Cancel();

		void Act() => _ = FileSystem.RandomAccess.ReadAsync(handle,
			(IReadOnlyList<Memory<byte>>)null!, 0, cts.Token);

		await That(Act).ThrowsExactly<ArgumentNullException>().WithParamName("buffers")
			.Because("the arguments are validated synchronously before the cancellation is observed");
	}

	[Test]
	[AutoArguments]
	public async Task ReadAsync_WithWriteOnlyHandle_WhenCancelled_ShouldThrowTaskCanceledException(
		string path)
	{
		FileSystem.File.WriteAllBytes(path, [1, 2, 3,]);
		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write);
		using CancellationTokenSource cts = new();
		cts.Cancel();

		async Task Act() => await FileSystem.RandomAccess.ReadAsync(handle, new byte[1], 0, cts.Token);

		await That(Act).Throws<TaskCanceledException>()
			.Because("the cancellation is observed before the access to the file is checked");
	}

	[Test]
	[AutoArguments]
	public async Task WriteAsync_WithDisposedHandle_WhenCancelled_ShouldThrowObjectDisposedException(
		string path)
	{
		SafeFileHandle handle = OpenDisposedHandle(path);
		using CancellationTokenSource cts = new();
		cts.Cancel();

		void Act() => _ = FileSystem.RandomAccess.WriteAsync(handle, new byte[] { 1, }, 0, cts.Token);

		await That(Act).Throws<ObjectDisposedException>()
			.Because("the arguments are validated synchronously before the cancellation is observed");
	}

	[Test]
	[AutoArguments]
	public async Task WriteAsync_WithNegativeOffset_WhenCancelled_ShouldThrowArgumentOutOfRangeException(
		string path)
	{
		FileSystem.File.WriteAllBytes(path, [1, 2, 3,]);
		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write);
		using CancellationTokenSource cts = new();
		cts.Cancel();

		void Act() => _ = FileSystem.RandomAccess.WriteAsync(handle,
			(IReadOnlyList<ReadOnlyMemory<byte>>)[new byte[] { 1, },], -1, cts.Token);

		await That(Act).Throws<ArgumentOutOfRangeException>().WithParamName("fileOffset")
			.Because("the arguments are validated synchronously before the cancellation is observed");
	}

#if FEATURE_RANDOMACCESS_FLUSHTODISK
	[Test]
	public async Task SetLength_WithNullHandleAndNegativeLength_ShouldThrowArgumentNullException()
	{
		void Act() => FileSystem.RandomAccess.SetLength(null!, -1);

		await That(Act).ThrowsExactly<ArgumentNullException>().WithParamName("handle")
			.Because("the handle is validated before the length");
	}

	[Test]
	[AutoArguments]
	public async Task SetLength_WithDisposedHandleAndNegativeLength_ShouldThrowObjectDisposedException(
		string path)
	{
		SafeFileHandle handle = OpenDisposedHandle(path);

		void Act() => FileSystem.RandomAccess.SetLength(handle, -1);

		await That(Act).Throws<ObjectDisposedException>()
			.Because("the handle is validated before the length");
	}

	[Test]
	[AutoArguments]
	public async Task SetLength_WithReadOnlyHandleAndNegativeLength_ShouldThrowArgumentOutOfRangeException(
		string path)
	{
		FileSystem.File.WriteAllBytes(path, [1, 2, 3,]);
		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		void Act() => FileSystem.RandomAccess.SetLength(handle, -1);

		await That(Act).Throws<ArgumentOutOfRangeException>().WithParamName("length")
			.Because("the length is validated before the access to the file is checked");
	}
#endif

	private SafeFileHandle OpenDisposedHandle(string path)
	{
		FileSystem.File.WriteAllBytes(path, [1, 2, 3,]);
		SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.ReadWrite);
		handle.Dispose();
		return handle;
	}
}
#endif
