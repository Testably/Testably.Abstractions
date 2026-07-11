using System.IO;
using System.IO.MemoryMappedFiles;

namespace Testably.Abstractions.MemoryMappedFiles.Tests.MemoryMappedFile;

[FileSystemTests]
public class CreateFromFileTests(FileSystemTestData testData)
	: FileSystemTestBase(testData)
{
	[Test]
	public async Task CreateFromFile_OnEmptyFile_WithoutCapacity_ShouldThrowArgumentException()
	{
		FileSystem.File.WriteAllBytes("empty.bin", []);

		void Act()
		{
			using IMemoryMappedFile _ =
				FileSystem.MemoryMappedFile.CreateFromFile("empty.bin");
		}

		await That(Act).Throws<ArgumentException>();
	}

	[Test]
	public async Task CreateFromFile_WithNegativeCapacity_ShouldThrowArgumentOutOfRangeException()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);

		void Act()
		{
			using IMemoryMappedFile _ = FileSystem.MemoryMappedFile.CreateFromFile(
				"data.bin", FileMode.Open, null, -1, MemoryMappedFileAccess.ReadWrite);
		}

		await That(Act).Throws<ArgumentOutOfRangeException>()
			.WithParamName("capacity");
	}

	[Test]
	public async Task CreateFromFile_WithCapacitySmallerThanFile_ShouldThrowArgumentOutOfRangeException()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);

		void Act()
		{
			using IMemoryMappedFile _ = FileSystem.MemoryMappedFile.CreateFromFile(
				"data.bin", FileMode.Open, null, 50, MemoryMappedFileAccess.ReadWrite);
		}

		await That(Act).Throws<ArgumentOutOfRangeException>()
			.WithParamName("capacity");
	}

	[Test]
	public async Task CreateFromFile_WithReadAccess_AndCapacityLargerThanFile_ShouldThrowArgumentException()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);

		void Act()
		{
			using IMemoryMappedFile _ = FileSystem.MemoryMappedFile.CreateFromFile(
				"data.bin", FileMode.Open, null, 200, MemoryMappedFileAccess.Read);
		}

		await That(Act).Throws<ArgumentException>();
	}

	[Test]
	public async Task CreateFromFile_WithLargerCapacity_ShouldGrowFile()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[10]);

		using (IMemoryMappedFile mappedFile = FileSystem.MemoryMappedFile.CreateFromFile(
			"data.bin", FileMode.Open, null, 100, MemoryMappedFileAccess.ReadWrite))
		{
			using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();
			accessor.Write(50, 4242);
		}

		await That(FileSystem.File.ReadAllBytes("data.bin").Length).IsEqualTo(100);
	}

	[Test]
	public async Task CreateFromFile_WrittenData_ShouldPersistToFile()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);

		using (IMemoryMappedFile mappedFile =
			FileSystem.MemoryMappedFile.CreateFromFile("data.bin"))
		{
			using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();
			accessor.Write(0, (byte)42);
			accessor.Write(1, (byte)43);
		}

		byte[] bytes = FileSystem.File.ReadAllBytes("data.bin");
		await That(bytes[0]).IsEqualTo((byte)42);
		await That(bytes[1]).IsEqualTo((byte)43);
	}

	[Test]
	public async Task CreateViewAccessor_BeyondFileCapacity_ShouldThrowUnauthorizedAccessException()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using IMemoryMappedFile mappedFile =
			FileSystem.MemoryMappedFile.CreateFromFile("data.bin");

		void Act() => mappedFile.CreateViewAccessor(50, 100);

		await That(Act).Throws<UnauthorizedAccessException>();
	}

	[Test]
	public async Task CreateViewAccessor_WithOffsetBeyondCapacity_ShouldThrowUnauthorizedAccessException()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using IMemoryMappedFile mappedFile =
			FileSystem.MemoryMappedFile.CreateFromFile("data.bin");

		void Act() => mappedFile.CreateViewAccessor(long.MaxValue, 0);

		await That(Act).Throws<UnauthorizedAccessException>();
	}

	[Test]
	public async Task CreateViewAccessor_WithSizeThatWouldOverflow_ShouldThrowUnauthorizedAccessException()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using IMemoryMappedFile mappedFile =
			FileSystem.MemoryMappedFile.CreateFromFile("data.bin");

		// `offset + size` would overflow `long`, so the view must still be rejected instead of
		// wrapping around to a valid-looking (negative) value.
		void Act() => mappedFile.CreateViewAccessor(1, long.MaxValue);

		await That(Act).Throws<UnauthorizedAccessException>();
	}

	[Test]
	public async Task CreateViewAccessor_WithNegativeOffset_ShouldThrowArgumentOutOfRangeException()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using IMemoryMappedFile mappedFile =
			FileSystem.MemoryMappedFile.CreateFromFile("data.bin");

		void Act() => mappedFile.CreateViewAccessor(-1, 10);

		await That(Act).Throws<ArgumentOutOfRangeException>()
			.WithParamName("offset");
	}

	[Test]
	public async Task CreateViewAccessor_WithNegativeSize_ShouldThrowArgumentOutOfRangeException()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using IMemoryMappedFile mappedFile =
			FileSystem.MemoryMappedFile.CreateFromFile("data.bin");

		void Act() => mappedFile.CreateViewAccessor(0, -1);

		await That(Act).Throws<ArgumentOutOfRangeException>()
			.WithParamName("size");
	}

	[Test]
	public async Task CreateFromFile_WithFileSystemStream_AndLeaveOpen_ShouldKeepStreamOpen()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using FileSystemStream stream =
			FileSystem.FileStream.New("data.bin", FileMode.Open, FileAccess.ReadWrite);

		using (IMemoryMappedFile mappedFile = FileSystem.MemoryMappedFile.CreateFromFile(
			stream, null, 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None,
			leaveOpen: true))
		{
			using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();
			accessor.Write(0, 12345);
		}

		// The stream is still usable after disposing the memory-mapped file.
		await That(stream.CanRead).IsTrue();
		stream.Position = 0;
	}

	[Test]
	public async Task CreateFromFile_WithFileSystemStream_AndNotLeaveOpen_ShouldDisposeStream()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		FileSystemStream stream =
			FileSystem.FileStream.New("data.bin", FileMode.Open, FileAccess.ReadWrite);

		using (IMemoryMappedFile mappedFile = FileSystem.MemoryMappedFile.CreateFromFile(
			stream, null, 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None,
			leaveOpen: false))
		{
			using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();
			accessor.Write(0, 12345);
		}

		void Act() => stream.Position = 0;

		await That(Act).Throws<ObjectDisposedException>();
	}

	[Test]
	public async Task View_ShouldRemainUsable_AfterMappedFileIsDisposed()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		IMemoryMappedViewAccessor accessor;
		using (IMemoryMappedFile mappedFile =
			FileSystem.MemoryMappedFile.CreateFromFile("data.bin"))
		{
			accessor = mappedFile.CreateViewAccessor();
			accessor.Write(0, 12345);
		}

		// The memory-mapped file is disposed, but the still-open view keeps working (the file and
		// its views have independent lifetimes).
		await That(accessor.ReadInt32(0)).IsEqualTo(12345);
		accessor.Write(4, 67890);
		await That(accessor.ReadInt32(4)).IsEqualTo(67890);
		accessor.Dispose();
	}
}
