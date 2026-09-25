#if FEATURE_FILESYSTEM_RANDOMACCESS
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public class RandomAccessStatisticsTests
{
#if FEATURE_RANDOMACCESS_FLUSHTODISK
	[Test]
	public async Task Method_FlushToDisk_SafeFileHandle_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using SafeFileHandle handle = OpenHandle(sut);

		sut.RandomAccess.FlushToDisk(handle);

		await That(sut.Statistics.RandomAccess).OnlyContainsMethodCall(
			nameof(IRandomAccess.FlushToDisk), handle);
	}
#endif

	[Test]
	public async Task Method_GetLength_SafeFileHandle_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using SafeFileHandle handle = OpenHandle(sut);

		_ = sut.RandomAccess.GetLength(handle);

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.RandomAccess).OnlyContainsMethodCall(
			nameof(IRandomAccess.GetLength), handle);
	}

	[Test]
	public async Task Method_Read_SafeFileHandle_IReadOnlyListMemoryByte_Int64_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using SafeFileHandle handle = OpenHandle(sut);
		IReadOnlyList<Memory<byte>> buffers = [new byte[1],];
		long fileOffset = 0;

		_ = sut.RandomAccess.Read(handle, buffers, fileOffset);

		await That(sut.Statistics.RandomAccess).OnlyContainsMethodCall(
			nameof(IRandomAccess.Read), handle, buffers, fileOffset);
	}

	[Test]
	public async Task Method_Read_SafeFileHandle_SpanByte_Int64_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using SafeFileHandle handle = OpenHandle(sut);
		Span<byte> buffer = new byte[1];
		long fileOffset = 0;

		_ = sut.RandomAccess.Read(handle, buffer, fileOffset);

		await That(sut.Statistics.RandomAccess).OnlyContainsMethodCall(
			nameof(IRandomAccess.Read), handle, (ReadOnlySpan<byte>)buffer, fileOffset);
	}

	[Test]
	public async Task
		Method_ReadAsync_SafeFileHandle_IReadOnlyListMemoryByte_Int64_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using SafeFileHandle handle = OpenHandle(sut);
		IReadOnlyList<Memory<byte>> buffers = [new byte[1],];
		long fileOffset = 0;
		CancellationToken cancellationToken = CancellationToken.None;

		_ = await sut.RandomAccess.ReadAsync(handle, buffers, fileOffset, cancellationToken);

		await That(sut.Statistics.RandomAccess).OnlyContainsMethodCall(
			nameof(IRandomAccess.ReadAsync), handle, buffers, fileOffset, cancellationToken);
	}

	[Test]
	public async Task
		Method_ReadAsync_SafeFileHandle_IReadOnlyListMemoryByte_Int64_CancellationToken_WhenCancelled_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using SafeFileHandle handle = OpenHandle(sut);
		IReadOnlyList<Memory<byte>> buffers = [new byte[1],];
		long fileOffset = 0;
		using CancellationTokenSource cts = new();
		cts.Cancel();
		CancellationToken cancellationToken = cts.Token;

		async Task Act() => _ = await sut.RandomAccess.ReadAsync(handle, buffers, fileOffset,
			cancellationToken);

		await That(Act).Throws<TaskCanceledException>();
		await That(sut.Statistics.RandomAccess).OnlyContainsMethodCall(
			nameof(IRandomAccess.ReadAsync), handle, buffers, fileOffset, cancellationToken);
	}

	[Test]
	public async Task
		Method_ReadAsync_SafeFileHandle_MemoryByte_Int64_CancellationToken_WhenCancelled_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using SafeFileHandle handle = OpenHandle(sut);
		Memory<byte> buffer = new byte[1];
		long fileOffset = 0;
		using CancellationTokenSource cts = new();
		cts.Cancel();
		CancellationToken cancellationToken = cts.Token;

		async Task Act() => _ = await sut.RandomAccess.ReadAsync(handle, buffer, fileOffset,
			cancellationToken);

		await That(Act).Throws<TaskCanceledException>();
		await That(sut.Statistics.RandomAccess).OnlyContainsMethodCall(
			nameof(IRandomAccess.ReadAsync), handle, buffer, fileOffset, cancellationToken);
	}

	[Test]
	public async Task
		Method_ReadAsync_SafeFileHandle_MemoryByte_Int64_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using SafeFileHandle handle = OpenHandle(sut);
		Memory<byte> buffer = new byte[1];
		long fileOffset = 0;
		CancellationToken cancellationToken = CancellationToken.None;

		_ = await sut.RandomAccess.ReadAsync(handle, buffer, fileOffset, cancellationToken);

		await That(sut.Statistics.RandomAccess).OnlyContainsMethodCall(
			nameof(IRandomAccess.ReadAsync), handle, buffer, fileOffset, cancellationToken);
	}

#if FEATURE_RANDOMACCESS_FLUSHTODISK
	[Test]
	public async Task Method_SetLength_SafeFileHandle_Int64_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using SafeFileHandle handle = OpenHandle(sut);
		long length = 2;

		sut.RandomAccess.SetLength(handle, length);

		await That(sut.Statistics.RandomAccess).OnlyContainsMethodCall(
			nameof(IRandomAccess.SetLength), handle, length);
	}
#endif

	[Test]
	public async Task
		Method_Write_SafeFileHandle_IReadOnlyListReadOnlyMemoryByte_Int64_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using SafeFileHandle handle = OpenHandle(sut);
		IReadOnlyList<ReadOnlyMemory<byte>> buffers = [new byte[] { 1, },];
		long fileOffset = 0;

		sut.RandomAccess.Write(handle, buffers, fileOffset);

		await That(sut.Statistics.RandomAccess).OnlyContainsMethodCall(
			nameof(IRandomAccess.Write), handle, buffers, fileOffset);
	}

	[Test]
	public async Task Method_Write_SafeFileHandle_ReadOnlySpanByte_Int64_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using SafeFileHandle handle = OpenHandle(sut);
		ReadOnlySpan<byte> buffer = new byte[] { 1, };
		long fileOffset = 0;

		sut.RandomAccess.Write(handle, buffer, fileOffset);

		await That(sut.Statistics.RandomAccess).OnlyContainsMethodCall(
			nameof(IRandomAccess.Write), handle, buffer, fileOffset);
	}

	[Test]
	public async Task
		Method_WriteAsync_SafeFileHandle_IReadOnlyListReadOnlyMemoryByte_Int64_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using SafeFileHandle handle = OpenHandle(sut);
		IReadOnlyList<ReadOnlyMemory<byte>> buffers = [new byte[] { 1, },];
		long fileOffset = 0;
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.RandomAccess.WriteAsync(handle, buffers, fileOffset, cancellationToken);

		await That(sut.Statistics.RandomAccess).OnlyContainsMethodCall(
			nameof(IRandomAccess.WriteAsync), handle, buffers, fileOffset, cancellationToken);
	}

	[Test]
	public async Task
		Method_WriteAsync_SafeFileHandle_IReadOnlyListReadOnlyMemoryByte_Int64_CancellationToken_WhenCancelled_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using SafeFileHandle handle = OpenHandle(sut);
		IReadOnlyList<ReadOnlyMemory<byte>> buffers = [new byte[] { 1, },];
		long fileOffset = 0;
		using CancellationTokenSource cts = new();
		cts.Cancel();
		CancellationToken cancellationToken = cts.Token;

		async Task Act() => await sut.RandomAccess.WriteAsync(handle, buffers, fileOffset,
			cancellationToken);

		await That(Act).Throws<TaskCanceledException>();
		await That(sut.Statistics.RandomAccess).OnlyContainsMethodCall(
			nameof(IRandomAccess.WriteAsync), handle, buffers, fileOffset, cancellationToken);
	}

	[Test]
	public async Task
		Method_WriteAsync_SafeFileHandle_ReadOnlyMemoryByte_Int64_CancellationToken_WhenCancelled_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using SafeFileHandle handle = OpenHandle(sut);
		ReadOnlyMemory<byte> buffer = new byte[] { 1, };
		long fileOffset = 0;
		using CancellationTokenSource cts = new();
		cts.Cancel();
		CancellationToken cancellationToken = cts.Token;

		async Task Act() => await sut.RandomAccess.WriteAsync(handle, buffer, fileOffset,
			cancellationToken);

		await That(Act).Throws<TaskCanceledException>();
		await That(sut.Statistics.RandomAccess).OnlyContainsMethodCall(
			nameof(IRandomAccess.WriteAsync), handle, buffer, fileOffset, cancellationToken);
	}

	[Test]
	public async Task
		Method_WriteAsync_SafeFileHandle_ReadOnlyMemoryByte_Int64_CancellationToken_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		using SafeFileHandle handle = OpenHandle(sut);
		ReadOnlyMemory<byte> buffer = new byte[] { 1, };
		long fileOffset = 0;
		CancellationToken cancellationToken = CancellationToken.None;

		await sut.RandomAccess.WriteAsync(handle, buffer, fileOffset, cancellationToken);

		await That(sut.Statistics.RandomAccess).OnlyContainsMethodCall(
			nameof(IRandomAccess.WriteAsync), handle, buffer, fileOffset, cancellationToken);
	}

	private static SafeFileHandle OpenHandle(MockFileSystem sut)
		=> sut.File.OpenHandle("foo", FileMode.OpenOrCreate, FileAccess.ReadWrite);
}
#endif
