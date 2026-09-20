#if FEATURE_FILESYSTEM_RANDOMACCESS
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.RandomAccess;

[FileSystemTests]
public class WriteTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task Write_ShouldWriteAtTheGivenOffset(string path)
	{
		FileSystem.File.WriteAllBytes(path, [1, 2, 3, 4,]);

		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write))
		{
			FileSystem.RandomAccess.Write(handle, new byte[] { 9, 9, }, 1);
		}

		await That(FileSystem.File.ReadAllBytes(path))
			.IsEqualTo(new byte[] { 1, 9, 9, 4, });
	}

	[Test]
	[AutoArguments]
	public async Task Write_BeyondTheEnd_ShouldGrowTheFile(string path)
	{
		FileSystem.File.WriteAllBytes(path, [1, 2,]);

		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write))
		{
			FileSystem.RandomAccess.Write(handle, new byte[] { 7, }, 4);
		}

		await That(FileSystem.File.ReadAllBytes(path))
			.IsEqualTo(new byte[] { 1, 2, 0, 0, 7, });
	}

	[Test]
	[AutoArguments]
	public async Task Write_ShouldNotTruncateTheRemainder(string path)
	{
		FileSystem.File.WriteAllBytes(path, [1, 2, 3, 4, 5,]);

		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write))
		{
			FileSystem.RandomAccess.Write(handle, new byte[] { 9, }, 1);
		}

		await That(FileSystem.File.ReadAllBytes(path))
			.IsEqualTo(new byte[] { 1, 9, 3, 4, 5, });
	}

	[Test]
	[AutoArguments]
	public async Task Write_WithEmptyBuffer_ShouldNotChangeTheFile(string path)
	{
		FileSystem.File.WriteAllBytes(path, [1, 2, 3,]);

		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write))
		{
			FileSystem.RandomAccess.Write(handle, ReadOnlySpan<byte>.Empty, 0);
		}

		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(new byte[] { 1, 2, 3, });
	}

	[Test]
	[AutoArguments]
	public async Task Write_WithNegativeOffset_ShouldThrowArgumentOutOfRangeException(string path)
	{
		FileSystem.File.WriteAllBytes(path, [1,]);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write);

		void Act() => FileSystem.RandomAccess.Write(handle, new byte[] { 1, }, -1);

		await That(Act).Throws<ArgumentOutOfRangeException>();
	}

	[Test]
	[AutoArguments]
	public async Task Write_WithReadOnlyHandle_ShouldThrowUnauthorizedAccessException(string path)
	{
		FileSystem.File.WriteAllBytes(path, [1, 2, 3,]);

		using SafeFileHandle handle = FileSystem.File.OpenHandle(path);

		void Act() => FileSystem.RandomAccess.Write(handle, new byte[] { 9, }, 0);

		await That(Act).Throws<UnauthorizedAccessException>();
	}

	[Test]
	[AutoArguments]
	public async Task Write_WithGatherBuffers_ShouldWriteThemInOrder(string path)
	{
		FileSystem.File.WriteAllBytes(path, []);

		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write))
		{
			ReadOnlyMemory<byte> first = new byte[] { 1, 2, };
			ReadOnlyMemory<byte> second = new byte[] { 3, };
			IReadOnlyList<ReadOnlyMemory<byte>> buffers = [first, second,];

			FileSystem.RandomAccess.Write(handle, buffers, 0);
		}

		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(new byte[] { 1, 2, 3, });
	}

	[Test]
	[AutoArguments]
	public async Task WriteAsync_ShouldWriteAtTheGivenOffset(string path)
	{
		FileSystem.File.WriteAllBytes(path, [1, 2, 3,]);

		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write))
		{
			ReadOnlyMemory<byte> buffer = new byte[] { 8, 8, };
			await FileSystem.RandomAccess.WriteAsync(handle, buffer, 1, CancellationToken);
		}

		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(new byte[] { 1, 8, 8, });
	}

	[Test]
	[AutoArguments]
	public async Task WriteAsync_WithGatherBuffers_ShouldWriteThemInOrder(string path)
	{
		FileSystem.File.WriteAllBytes(path, []);

		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write))
		{
			ReadOnlyMemory<byte> first = new byte[] { 4, };
			ReadOnlyMemory<byte> second = new byte[] { 5, 6, };
			IReadOnlyList<ReadOnlyMemory<byte>> buffers = [first, second,];

			await FileSystem.RandomAccess.WriteAsync(handle, buffers, 0, CancellationToken);
		}

		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(new byte[] { 4, 5, 6, });
	}

	[Test]
	[AutoArguments]
	public async Task Write_ShouldBeVisibleThroughAnotherHandle(string path)
	{
		FileSystem.File.WriteAllBytes(path, []);

		using SafeFileHandle writer = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
		using SafeFileHandle reader = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

		FileSystem.RandomAccess.Write(writer, new byte[] { 1, 2, 3, }, 0);

		byte[] buffer = new byte[3];
		int read = FileSystem.RandomAccess.Read(reader, buffer, 0);

		await That(read).IsEqualTo(3);
		await That(buffer).IsEqualTo(new byte[] { 1, 2, 3, });
	}

	[Test]
	[AutoArguments]
	public async Task Write_ShouldBeVisibleThroughAStreamOnTheSamePath(string path)
	{
		FileSystem.File.WriteAllBytes(path, []);

		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
		{
			FileSystem.RandomAccess.Write(handle, new byte[] { 1, 2, 3, }, 0);
		}

		using FileSystemStream stream = FileSystem.File.OpenRead(path);
		byte[] buffer = new byte[3];
		int read = stream.Read(buffer, 0, buffer.Length);

		await That(read).IsEqualTo(3);
		await That(buffer).IsEqualTo(new byte[] { 1, 2, 3, });
	}

	/// <summary>
	///     <see cref="RandomAccess" /> permits concurrent writes through the same handle at distinct offsets, so a
	///     read-modify-write of the whole file must not let one of them overwrite the other.
	/// </summary>
	[Test]
	[AutoArguments]
	public async Task Write_ConcurrentlyAtDistinctOffsets_ShouldKeepBothWrites(string path)
	{
		const int count = 250;
		FileSystem.File.WriteAllBytes(path, new byte[2 * count]);

		using (SafeFileHandle handle = FileSystem.File.OpenHandle(path,
			FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
		{
			await Task.WhenAll(
				Task.Run(() =>
				{
					for (int i = 0; i < count; i++)
					{
						FileSystem.RandomAccess.Write(handle, new byte[] { 1, }, i);
					}
				}, CancellationToken),
				Task.Run(() =>
				{
					for (int i = 0; i < count; i++)
					{
						FileSystem.RandomAccess.Write(handle, new byte[] { 2, }, count + i);
					}
				}, CancellationToken));
		}

		byte[] result = FileSystem.File.ReadAllBytes(path);

		await That(result[..count]).All().AreEqualTo((byte)1);
		await That(result[count..]).All().AreEqualTo((byte)2);
	}
}
#endif
