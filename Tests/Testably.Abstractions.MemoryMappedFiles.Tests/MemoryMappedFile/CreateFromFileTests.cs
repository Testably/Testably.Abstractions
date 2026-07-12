using System.IO;
using System.IO.MemoryMappedFiles;
using Skip = Testably.Abstractions.TestHelpers.Skip;

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
	public async Task
		CreateFromFile_WithCapacitySmallerThanFile_ShouldThrowArgumentOutOfRangeException()
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
	public async Task
		CreateFromFile_WithNegativeCapacity_AndCreateMode_ShouldNotTruncateExistingFile()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);

		void Act()
		{
			using IMemoryMappedFile _ = FileSystem.MemoryMappedFile.CreateFromFile(
				"data.bin", FileMode.Create, null, -1, MemoryMappedFileAccess.ReadWrite);
		}

		await That(Act).Throws<ArgumentOutOfRangeException>()
			.WithParamName("capacity");
		await That(FileSystem.File.ReadAllBytes("data.bin").Length).IsEqualTo(100)
			.Because("the capacity is validated before the file is opened, so the FileMode.Create must not truncate it");
	}

	[Test]
	public async Task
		CreateFromFile_WithReadOnlyStream_AndReadWriteAccess_ShouldThrowUnauthorizedAccessException()
	{
		Skip.IfNot(FileSystem is MockFileSystem || Test.RunsOnWindows,
			"Mapping a read-write view over a read-only stream is only rejected on Windows; the mock mirrors Windows.");

		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using FileSystemStream stream =
			FileSystem.FileStream.New("data.bin", FileMode.Open, FileAccess.Read);

		void Act()
		{
			using IMemoryMappedFile _ = FileSystem.MemoryMappedFile.CreateFromFile(
				stream, null, 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None,
				leaveOpen: true);
		}

		await That(Act).Throws<UnauthorizedAccessException>()
			.Because("a read-write mapping requires a writable stream");
	}

	[Test]
	public async Task
		CreateFromFile_WithInvalidInheritability_ShouldThrowArgumentOutOfRangeException()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using FileSystemStream stream =
			FileSystem.FileStream.New("data.bin", FileMode.Open, FileAccess.ReadWrite);

		void Act()
		{
			using IMemoryMappedFile _ = FileSystem.MemoryMappedFile.CreateFromFile(
				stream, null, 0, MemoryMappedFileAccess.ReadWrite, (HandleInheritability)5,
				leaveOpen: true);
		}

		await That(Act).Throws<ArgumentOutOfRangeException>()
			.WithParamName("inheritability");
	}

	[Test]
	public async Task CreateFromFile_WithNullFileStream_ShouldThrowArgumentNullException()
	{
		void Act()
		{
			using IMemoryMappedFile _ = FileSystem.MemoryMappedFile.CreateFromFile(
				null!, null, 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None,
				leaveOpen: true);
		}

		await That(Act).Throws<ArgumentNullException>()
			.WithParamName("fileStream");
	}

	[Test]
	public async Task CreateViewAccessor_AfterDispose_ShouldThrowObjectDisposedException()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		IMemoryMappedFile mappedFile =
			FileSystem.MemoryMappedFile.CreateFromFile("data.bin");
		mappedFile.Dispose();

		void Act() => mappedFile.CreateViewAccessor();

		await That(Act).Throws<ObjectDisposedException>();
	}

	[Test]
	public async Task CreateViewStream_AfterDispose_ShouldThrowObjectDisposedException()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		IMemoryMappedFile mappedFile =
			FileSystem.MemoryMappedFile.CreateFromFile("data.bin");
		mappedFile.Dispose();

		void Act() => mappedFile.CreateViewStream();

		await That(Act).Throws<ObjectDisposedException>();
	}

	[Test]
	public async Task
		CreateViewAccessor_WithInvalidAccess_ShouldThrowArgumentOutOfRangeException()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using IMemoryMappedFile mappedFile =
			FileSystem.MemoryMappedFile.CreateFromFile("data.bin");

		void Act() => mappedFile.CreateViewAccessor(0, 0, (MemoryMappedFileAccess)42);

		await That(Act).Throws<ArgumentOutOfRangeException>()
			.WithParamName("access");
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

		await That(stream.CanRead).IsTrue()
			.Because("the stream is still usable after disposing the memory-mapped file");
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
	public async Task CreateFromFile_WithLargerCapacity_WithoutViewWrites_ShouldGrowFile()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[10]);

		using (IMemoryMappedFile _ = FileSystem.MemoryMappedFile.CreateFromFile(
			"data.bin", FileMode.Open, null, 100, MemoryMappedFileAccess.ReadWrite))
		{
			// The file is grown to the capacity even when no view ever writes.
		}

		await That(FileSystem.File.ReadAllBytes("data.bin").Length).IsEqualTo(100);
	}

	[Test]
	public async Task DisposeViews_AfterCallerOwnedStreamWasDisposed_ShouldNotThrow()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		FileSystemStream stream =
			FileSystem.FileStream.New("data.bin", FileMode.Open, FileAccess.ReadWrite);
		IMemoryMappedFile mappedFile = FileSystem.MemoryMappedFile.CreateFromFile(
			stream, null, 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None,
			leaveOpen: true);
		IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();
		MemoryMappedFileSystemViewStream viewStream = mappedFile.CreateViewStream();
		stream.Dispose();

		void Act()
		{
			accessor.Dispose();
			viewStream.Dispose();
			mappedFile.Dispose();
		}

		await That(Act).DoesNotThrow()
			.Because("disposing the views must stay safe after the caller disposed its own stream");
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
	public async Task
		CreateFromFile_WithReadAccess_AndCapacityLargerThanFile_ShouldThrowArgumentException()
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
	public async Task
		CreateFromFile_WithReadExecuteAccess_AndCapacityLargerThanFile_ShouldThrowUnauthorizedAccessException()
	{
		Skip.IfNot(FileSystem is MockFileSystem || Test.RunsOnWindows,
			"Growing a file through a read-only handle is rejected with an UnauthorizedAccessException only on Windows; the mock mirrors Windows.");

		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);

		void Act()
		{
			using IMemoryMappedFile _ = FileSystem.MemoryMappedFile.CreateFromFile(
				"data.bin", FileMode.Open, null, 200, MemoryMappedFileAccess.ReadExecute);
		}

		await That(Act).Throws<UnauthorizedAccessException>()
			.Because("only MemoryMappedFileAccess.Read is special-cased with an ArgumentException by the BCL");
	}

	[Test]
	public async Task
		CreateViewAccessor_WithExecuteAccess_OnMappingWithoutExecuteAccess_ShouldThrowUnauthorizedAccessException()
	{
		Skip.IfNot(FileSystem is MockFileSystem || Test.RunsOnWindows,
			"An execute view on a mapping without execute access is only rejected on Windows; the mock mirrors Windows.");

		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using IMemoryMappedFile mappedFile = FileSystem.MemoryMappedFile.CreateFromFile(
			"data.bin", FileMode.Open, null, 0, MemoryMappedFileAccess.ReadWrite);

		void Act()
		{
			using IMemoryMappedViewAccessor _ = mappedFile.CreateViewAccessor(
				0, 10, MemoryMappedFileAccess.ReadExecute);
		}

		await That(Act).Throws<UnauthorizedAccessException>()
			.Because("the mapping was created without execute page protection");
	}

	[Test]
	public async Task CreateFromFile_WhenCreationFails_ShouldDeleteCreatedFile()
	{
		void Act()
		{
			using IMemoryMappedFile _ =
				FileSystem.MemoryMappedFile.CreateFromFile("new.bin", FileMode.CreateNew);
		}

		await That(Act).Throws<ArgumentException>()
			.Because("a memory-mapped file over an empty file requires a positive capacity");
		await That(FileSystem.File.Exists("new.bin")).IsFalse()
			.Because("the file created by the failed call is deleted again, matching the BCL");
	}

	[Test]
	public async Task CreateFromFile_WithAppendMode_ShouldThrowArgumentException()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);

		void Act()
		{
			using IMemoryMappedFile _ =
				FileSystem.MemoryMappedFile.CreateFromFile("data.bin", FileMode.Append);
		}

		await That(Act).Throws<ArgumentException>()
			.WithParamName("mode");
	}

	[Test]
	public async Task CreateFromFile_WithEmptyMapName_ShouldThrowArgumentException()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);

		void Act()
		{
			using IMemoryMappedFile _ = FileSystem.MemoryMappedFile.CreateFromFile(
				"data.bin", FileMode.Open, "");
		}

		await That(Act).Throws<ArgumentException>();
	}

	[Test]
	public async Task
		CreateFromFile_WithFileSystemStream_AndLeaveOpen_ShouldNotChangeStreamPosition()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using FileSystemStream stream =
			FileSystem.FileStream.New("data.bin", FileMode.Open, FileAccess.ReadWrite);
		stream.Position = 7;

		using IMemoryMappedFile mappedFile = FileSystem.MemoryMappedFile.CreateFromFile(
			stream, null, 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None,
			leaveOpen: true);
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();
		accessor.Write(50, 1234567);
		_ = accessor.ReadInt32(50);

		await That(stream.Position).IsEqualTo(7L)
			.Because("view operations must not move the position of the caller-owned stream");
	}

	[Test]
	public async Task CreateFromFile_WithMapName_OnMockFileSystem_ShouldThrowNotSupportedException()
	{
		Skip.IfNot(FileSystem is MockFileSystem);

		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);

		void Act()
		{
			using IMemoryMappedFile _ = FileSystem.MemoryMappedFile.CreateFromFile(
				"data.bin", FileMode.Open, "myMap");
		}

		await That(Act).Throws<NotSupportedException>()
			.Because("named mappings are operating-system shared memory, which the mock cannot honor");
	}

	[Test]
	public async Task CreateFromFile_WithTruncateMode_ShouldThrowArgumentException_AndKeepFileContent()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);

		void Act()
		{
			using IMemoryMappedFile _ = FileSystem.MemoryMappedFile.CreateFromFile(
				"data.bin", FileMode.Truncate, null, 100, MemoryMappedFileAccess.ReadWrite);
		}

		await That(Act).Throws<ArgumentException>()
			.WithParamName("mode");
		await That(FileSystem.File.ReadAllBytes("data.bin").Length).IsEqualTo(100)
			.Because("the file must not be touched when the mode is rejected");
	}

	[Test]
	public async Task CreateFromFile_WithWriteAccess_ShouldThrowArgumentException()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);

		void Act()
		{
			using IMemoryMappedFile _ = FileSystem.MemoryMappedFile.CreateFromFile(
				"data.bin", FileMode.Open, null, 0, MemoryMappedFileAccess.Write);
		}

		await That(Act).Throws<ArgumentException>()
			.WithParamName("access");
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
		await That(bytes[0]).IsEqualTo(42);
		await That(bytes[1]).IsEqualTo(43);
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
	public async Task
		CreateViewAccessor_WithOffsetBeyondCapacity_ShouldThrowUnauthorizedAccessException()
	{
		Skip.IfNot(FileSystem is MockFileSystem || Test.RunsOnWindows,
			"An offset beyond the capacity is rejected with an UnauthorizedAccessException only on Windows; the mock mirrors Windows.");

		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using IMemoryMappedFile mappedFile =
			FileSystem.MemoryMappedFile.CreateFromFile("data.bin");

		void Act() => mappedFile.CreateViewAccessor(long.MaxValue, 0);

		await That(Act).Throws<UnauthorizedAccessException>();
	}

	[Test]
	public async Task CreateViewAccessor_WithSizeThatWouldOverflow_ShouldThrowIOException()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using IMemoryMappedFile mappedFile =
			FileSystem.MemoryMappedFile.CreateFromFile("data.bin");

		void Act() => mappedFile.CreateViewAccessor(1, long.MaxValue);

		await That(Act).Throws<IOException>()
			.Because(
				"`offset + size` would overflow `long`, so the operating system can never reserve the view");
	}

	[Test]
	public async Task CreateFromFile_WithPath_ShouldOpenFileWithoutSharing()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using IMemoryMappedFile mappedFile =
			FileSystem.MemoryMappedFile.CreateFromFile("data.bin");

		void Act() => FileSystem.File
			.Open("data.bin", FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
			.Dispose();

		await That(Act).Throws<IOException>()
			.Because(
				"the BCL opens the backing file of a path-based memory-mapped file with FileShare.None");
	}

	[Test]
	public async Task
		CreateFromFile_WithCapacityGreaterThan2GB_OnMockFileSystem_ShouldThrowNotSupportedException()
	{
		Skip.IfNot(FileSystem is MockFileSystem,
			"The 2 GB limit only applies to the in-memory content of the MockFileSystem.");

		void Act()
		{
			using IMemoryMappedFile _ = FileSystem.MemoryMappedFile.CreateFromFile(
				"huge.bin", FileMode.CreateNew, null, 3L * 1024 * 1024 * 1024,
				MemoryMappedFileAccess.ReadWrite);
		}

		await That(Act).Throws<NotSupportedException>();
		await That(FileSystem.File.Exists("huge.bin")).IsFalse()
			.Because("the file only created by this call is deleted when the creation fails");
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

		await That(accessor.ReadInt32(0)).IsEqualTo(12345)
			.Because(
				"The memory-mapped file is disposed, but the still-open view keeps working (the file and its views have independent lifetimes)");
		accessor.Write(4, 67890);
		await That(accessor.ReadInt32(4)).IsEqualTo(67890);
		accessor.Dispose();
	}
}
