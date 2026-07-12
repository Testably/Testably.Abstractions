using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using Skip = Testably.Abstractions.TestHelpers.Skip;

namespace Testably.Abstractions.MemoryMappedFiles.Tests.MemoryMappedViewStream;

[FileSystemTests]
public class Tests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	public async Task Capacity_ShouldBeAtLeastTheViewSize()
	{
		using IMemoryMappedFile mappedFile = FileSystem.CreateMappedFile();

		using MemoryMappedFileSystemViewStream stream = mappedFile.CreateViewStream(0, 20);

		await That(stream.Capacity).IsGreaterThanOrEqualTo(20L)
			.Because("the real file system rounds the capacity up to the system page size, so it is only guaranteed to be at least the requested size.");
	}

	[Test]
	public async Task DefaultViewStream_OnReadOnlyMapping_ShouldThrowUnauthorizedAccessException()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using IMemoryMappedFile mappedFile = FileSystem.MemoryMappedFile.CreateFromFile(
			"data.bin", FileMode.Open, null, 0, MemoryMappedFileAccess.Read);

		void Act() => mappedFile.CreateViewStream();

		await That(Act).Throws<UnauthorizedAccessException>()
			.Because("the default view access is `ReadWrite`, which the read-only mapping does not permit");
	}

	[Test]
	public async Task DefaultViewStream_ShouldSupportReadWriteAndSeek()
	{
		using IMemoryMappedFile mappedFile = FileSystem.CreateMappedFile();

		using Stream stream = mappedFile.CreateViewStream();

		await That(stream.CanRead).IsTrue();
		await That(stream.CanWrite).IsTrue();
		await That(stream.CanSeek).IsTrue();
	}

	[Test]
	public async Task Length_ShouldMatchViewSize()
	{
		using IMemoryMappedFile mappedFile = FileSystem.CreateMappedFile();

		using Stream stream = mappedFile.CreateViewStream(0, 50);

		await That(stream.Length).IsEqualTo(50L);
	}

	[Test]
	public async Task PointerOffset_ForViewAtStart_ShouldBeZero()
	{
		using IMemoryMappedFile mappedFile = FileSystem.CreateMappedFile();

		using MemoryMappedFileSystemViewStream stream = mappedFile.CreateViewStream(0, 20);

		await That(stream.PointerOffset).IsEqualTo(0L);
	}

	[Test]
	public async Task Position_SetToNegative_ShouldThrowArgumentOutOfRangeException()
	{
		using IMemoryMappedFile mappedFile = FileSystem.CreateMappedFile();
		using Stream stream = mappedFile.CreateViewStream(0, 50);

		void Act() => stream.Position = -1;

		await That(Act).Throws<ArgumentOutOfRangeException>();
	}

	[Test]
	public async Task Read_AfterDispose_ShouldThrowObjectDisposedException()
	{
		using IMemoryMappedFile mappedFile = FileSystem.CreateMappedFile();
		Stream stream = mappedFile.CreateViewStream(0, 50);
		stream.Dispose();

		void Act() => _ = stream.Read(new byte[1], 0, 1);

		await That(Act).Throws<ObjectDisposedException>();
	}

	[Test]
	public async Task Write_AfterDispose_ShouldThrowObjectDisposedException()
	{
		using IMemoryMappedFile mappedFile = FileSystem.CreateMappedFile();
		Stream stream = mappedFile.CreateViewStream(0, 50);
		stream.Dispose();

		void Act() => stream.Write(new byte[1], 0, 1);

		await That(Act).Throws<ObjectDisposedException>();
	}

	[Test]
	public async Task CanReadWriteSeek_AfterDispose_ShouldBeFalse()
	{
		using IMemoryMappedFile mappedFile = FileSystem.CreateMappedFile();
		Stream stream = mappedFile.CreateViewStream(0, 50);

		stream.Dispose();

		await That(stream.CanRead).IsFalse();
		await That(stream.CanWrite).IsFalse();
		await That(stream.CanSeek).IsFalse();
	}

	[Test]
	public async Task Read_AtEndOfStream_ShouldReturnZero()
	{
		using IMemoryMappedFile mappedFile = FileSystem.CreateMappedFile();
		using Stream stream = mappedFile.CreateViewStream(0, 10);
		stream.Seek(0, SeekOrigin.End);

		int read = stream.Read(new byte[10], 0, 10);

		await That(read).IsEqualTo(0);
	}

	[Test]
	public async Task Read_WithCountLargerThanBuffer_ShouldThrowArgumentException()
	{
		using IMemoryMappedFile mappedFile = FileSystem.CreateMappedFile();
		using Stream stream = mappedFile.CreateViewStream(0, 50);

		void Act() => _ = stream.Read(new byte[10], 5, 10);

		await That(Act).Throws<ArgumentException>();
	}

	[Test]
	public async Task Read_WithNegativeOffset_ShouldThrowArgumentOutOfRangeException()
	{
		using IMemoryMappedFile mappedFile = FileSystem.CreateMappedFile();
		using Stream stream = mappedFile.CreateViewStream(0, 50);

		void Act() => _ = stream.Read(new byte[10], -1, 1);

		await That(Act).Throws<ArgumentOutOfRangeException>();
	}

	[Test]
	public async Task Read_WithNullBuffer_ShouldThrowArgumentNullException()
	{
		using IMemoryMappedFile mappedFile = FileSystem.CreateMappedFile();
		using Stream stream = mappedFile.CreateViewStream(0, 50);

		void Act() => _ = stream.Read(null!, 0, 1);

		await That(Act).Throws<ArgumentNullException>();
	}

	[Test]
	public async Task ReadOnlyViewStream_ShouldNotSupportWriting()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using IMemoryMappedFile mappedFile = FileSystem.MemoryMappedFile.CreateFromFile(
			"data.bin", FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
		using Stream stream =
			mappedFile.CreateViewStream(0, 50, MemoryMappedFileAccess.Read);

		await That(stream.CanWrite).IsFalse();

		void Act() => stream.Write(new byte[5], 0, 5);

		await That(Act).Throws<NotSupportedException>();
	}

	[Test]
	public async Task ReadWrite_ShouldRoundtrip()
	{
		using IMemoryMappedFile mappedFile = FileSystem.CreateMappedFile();
		byte[] payload = [1, 2, 3, 4, 5,];

		using (Stream writeStream = mappedFile.CreateViewStream(0, 50))
		{
			writeStream.Write(payload, 0, payload.Length);
			writeStream.Flush();
		}

		byte[] result = new byte[payload.Length];
		using (Stream readStream = mappedFile.CreateViewStream(0, 50))
		{
			int read = 0;
			while (read < result.Length)
			{
				int r = readStream.Read(result, read, result.Length - read);
				if (r == 0)
				{
					break;
				}

				read += r;
			}
		}

		await That(result).IsEqualTo(payload);
	}

	[Test]
	public async Task Seek_BeforeBeginning_ShouldThrowIOException()
	{
		using IMemoryMappedFile mappedFile = FileSystem.CreateMappedFile();
		using Stream stream = mappedFile.CreateViewStream(0, 50);

		void Act() => stream.Seek(-1, SeekOrigin.Begin);

		await That(Act).Throws<IOException>();
	}

	[Test]
	public async Task Seek_FromBeginCurrentAndEnd_ShouldUpdatePosition()
	{
		using IMemoryMappedFile mappedFile = FileSystem.CreateMappedFile();
		using Stream stream = mappedFile.CreateViewStream(0, 50);

		await That(stream.Seek(10, SeekOrigin.Begin)).IsEqualTo(10L);
		await That(stream.Seek(5, SeekOrigin.Current)).IsEqualTo(15L);
		await That(stream.Seek(-10, SeekOrigin.End)).IsEqualTo(40L);
		await That(stream.Position).IsEqualTo(40L);
	}

	[Test]
	public async Task SetLength_ShouldThrowNotSupportedException()
	{
		using IMemoryMappedFile mappedFile = FileSystem.CreateMappedFile();
		using Stream stream = mappedFile.CreateViewStream(0, 50);

		void Act() => stream.SetLength(10);

		await That(Act).Throws<NotSupportedException>();
	}

	[Test]
	public async Task Read_AfterBackingStreamWasTruncated_ShouldReturnZeros()
	{
		Skip.IfNot(FileSystem is MockFileSystem,
			"The operating system rejects truncating a file with a user-mapped section.");

		FileSystem.File.WriteAllBytes("data.bin",
			Enumerable.Repeat((byte)7, 100).ToArray());
		using FileSystemStream stream = FileSystem.FileStream.New(
			"data.bin", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
		using IMemoryMappedFile mappedFile = FileSystem.MemoryMappedFile.CreateFromFile(
			stream, null, 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None,
			leaveOpen: true);
		using Stream viewStream = mappedFile.CreateViewStream();

		stream.SetLength(50);
		viewStream.Position = 60;
		byte[] buffer = [9, 9, 9, 9,];
		int read = viewStream.Read(buffer, 0, 4);

		await That(read).IsEqualTo(4)
			.Because("the view spans the full capacity, which does not shrink with the file");
		await That(buffer).IsEqualTo(new byte[4])
			.Because("reads of the truncated range return zeros");
	}

	[Test]
	public async Task Write_BeyondCapacity_ShouldThrowNotSupportedException()
	{
		using IMemoryMappedFile mappedFile = FileSystem.CreateMappedFile();
		using Stream stream = mappedFile.CreateViewStream(0, 10);

		void Act() => stream.Write(new byte[20], 0, 20);

		await That(Act).Throws<NotSupportedException>();
	}

	[Test]
	public async Task WriteOnlyViewStream_ShouldNotSupportReading()
	{
		using IMemoryMappedFile mappedFile = FileSystem.CreateMappedFile();
		using Stream stream =
			mappedFile.CreateViewStream(0, 50, MemoryMappedFileAccess.Write);

		await That(stream.CanRead).IsFalse();

		void Act() => _ = stream.Read(new byte[5], 0, 5);

		await That(Act).Throws<NotSupportedException>();
	}
}
