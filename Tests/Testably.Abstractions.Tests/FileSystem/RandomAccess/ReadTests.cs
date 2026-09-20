#if FEATURE_FILESYSTEM_RANDOMACCESS
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.RandomAccess;

[FileSystemTests]
public class ReadTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task Read_ShouldReadFromTheGivenOffset(string path)
	{
		byte[] contents = [1, 2, 3, 4, 5, 6, 7, 8,];
		FileSystem.File.WriteAllBytes(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		byte[] buffer = new byte[4];
		int read = FileSystem.RandomAccess.Read(handle, buffer, 2);

		await That(read).IsEqualTo(4);
		await That(buffer).IsEqualTo(new byte[] { 3, 4, 5, 6, });
	}

	[Test]
	[AutoArguments]
	public async Task Read_ShouldNotAdvanceAnyPosition(string path)
	{
		byte[] contents = [1, 2, 3, 4,];
		FileSystem.File.WriteAllBytes(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		byte[] first = new byte[2];
		byte[] second = new byte[2];
		FileSystem.RandomAccess.Read(handle, first, 0);
		FileSystem.RandomAccess.Read(handle, second, 0);

		await That(first).IsEqualTo(second);
	}

	[Test]
	[AutoArguments]
	public async Task Read_WhenBufferIsLargerThanTheRemainder_ShouldReturnShortRead(string path)
	{
		byte[] contents = [1, 2, 3,];
		FileSystem.File.WriteAllBytes(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		byte[] buffer = new byte[10];
		int read = FileSystem.RandomAccess.Read(handle, buffer, 1);

		await That(read).IsEqualTo(2);
		await That(buffer[0]).IsEqualTo((byte)2);
		await That(buffer[1]).IsEqualTo((byte)3);
	}

	[Test]
	[AutoArguments]
	public async Task Read_WhenOffsetIsBeyondTheEnd_ShouldReturnZero(string path)
	{
		byte[] contents = [1, 2, 3,];
		FileSystem.File.WriteAllBytes(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		byte[] buffer = new byte[4];
		int read = FileSystem.RandomAccess.Read(handle, buffer, 10);

		await That(read).IsEqualTo(0);
	}

	[Test]
	[AutoArguments]
	public async Task Read_WhenOffsetIsAtTheEnd_ShouldReturnZero(string path)
	{
		byte[] contents = [1, 2, 3,];
		FileSystem.File.WriteAllBytes(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		byte[] buffer = new byte[4];
		int read = FileSystem.RandomAccess.Read(handle, buffer, 3);

		await That(read).IsEqualTo(0);
	}

	[Test]
	[AutoArguments]
	public async Task Read_WithEmptyBuffer_ShouldReturnZero(string path)
	{
		byte[] contents = [1, 2, 3,];
		FileSystem.File.WriteAllBytes(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		int read = FileSystem.RandomAccess.Read(handle, Span<byte>.Empty, 0);

		await That(read).IsEqualTo(0);
	}

	[Test]
	[AutoArguments]
	public async Task Read_WithNegativeOffset_ShouldThrowArgumentOutOfRangeException(string path)
	{
		FileSystem.File.WriteAllBytes(path, [1, 2, 3,]);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		void Act() => FileSystem.RandomAccess.Read(handle, new byte[2], -1);

		await That(Act).Throws<ArgumentOutOfRangeException>();
	}

	[Test]
	[AutoArguments]
	public async Task Read_WithWriteOnlyHandle_ShouldThrowUnauthorizedAccessException(string path)
	{
		FileSystem.File.WriteAllBytes(path, [1, 2, 3,]);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write);

		void Act() => FileSystem.RandomAccess.Read(handle, new byte[2], 0);

		await That(Act).Throws<UnauthorizedAccessException>();
	}

	[Test]
	[AutoArguments]
	public async Task Read_WithScatterBuffers_ShouldFillThemInOrder(string path)
	{
		byte[] contents = [1, 2, 3, 4, 5, 6,];
		FileSystem.File.WriteAllBytes(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		Memory<byte> first = new byte[2];
		Memory<byte> second = new byte[3];
		IReadOnlyList<Memory<byte>> buffers = [first, second,];

		long read = FileSystem.RandomAccess.Read(handle, buffers, 1);

		await That(read).IsEqualTo(5L);
		await That(first.ToArray()).IsEqualTo(new byte[] { 2, 3, });
		await That(second.ToArray()).IsEqualTo(new byte[] { 4, 5, 6, });
	}

	[Test]
	[AutoArguments]
	public async Task Read_WithScatterBuffers_WhenFileIsShorter_ShouldReturnShortRead(string path)
	{
		byte[] contents = [1, 2, 3,];
		FileSystem.File.WriteAllBytes(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		Memory<byte> first = new byte[2];
		Memory<byte> second = new byte[4];
		IReadOnlyList<Memory<byte>> buffers = [first, second,];

		long read = FileSystem.RandomAccess.Read(handle, buffers, 0);

		await That(read).IsEqualTo(3L);
		await That(first.ToArray()).IsEqualTo(new byte[] { 1, 2, });
		await That(second.ToArray()[0]).IsEqualTo((byte)3);
	}

	[Test]
	[AutoArguments]
	public async Task ReadAsync_ShouldReadFromTheGivenOffset(string path)
	{
		byte[] contents = [1, 2, 3, 4, 5,];
		FileSystem.File.WriteAllBytes(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		Memory<byte> buffer = new byte[3];
		int read = await FileSystem.RandomAccess.ReadAsync(handle, buffer, 2, CancellationToken);

		await That(read).IsEqualTo(3);
		await That(buffer.ToArray()).IsEqualTo(new byte[] { 3, 4, 5, });
	}

	[Test]
	[AutoArguments]
	public async Task ReadAsync_WithScatterBuffers_ShouldFillThemInOrder(string path)
	{
		byte[] contents = [1, 2, 3, 4,];
		FileSystem.File.WriteAllBytes(path, contents);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		Memory<byte> first = new byte[2];
		Memory<byte> second = new byte[2];
		IReadOnlyList<Memory<byte>> buffers = [first, second,];

		long read = await FileSystem.RandomAccess.ReadAsync(handle, buffers, 0, CancellationToken);

		await That(read).IsEqualTo(4L);
		await That(first.ToArray()).IsEqualTo(new byte[] { 1, 2, });
		await That(second.ToArray()).IsEqualTo(new byte[] { 3, 4, });
	}
}
#endif
