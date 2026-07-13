using System.IO;
using System.IO.MemoryMappedFiles;
using Skip = Testably.Abstractions.TestHelpers.Skip;

namespace Testably.Abstractions.MemoryMappedFiles.Tests.MemoryMappedFile;

[FileSystemTests]
public class CopyOnWriteTests(FileSystemTestData testData)
	: FileSystemTestBase(testData)
{
	[Test]
	public async Task CopyOnWriteMapping_CopyOnWriteView_ShouldBeAllowed()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using IMemoryMappedFile mappedFile = FileSystem.MemoryMappedFile.CreateFromFile(
			"data.bin", FileMode.Open, null, 0, MemoryMappedFileAccess.CopyOnWrite);
		using IMemoryMappedViewAccessor accessor =
			mappedFile.CreateViewAccessor(0, 100, MemoryMappedFileAccess.CopyOnWrite);

		accessor.Write(0, 42);

		await That(accessor.ReadInt32(0)).IsEqualTo(42);
	}

	[Test]
	public async Task
		CopyOnWriteMapping_WithCapacityLargerThanFile_ShouldThrowIOException_AndNotGrowFile()
	{
		Skip.IfNot(FileSystem is MockFileSystem || Test.RunsOnWindows,
			"The behavior of a copy-on-write mapping with a capacity larger than the file size is platform-specific; the mock mirrors Windows.");

		FileSystem.File.WriteAllBytes("data.bin", new byte[10]);

		void Act()
		{
			using IMemoryMappedFile _ = FileSystem.MemoryMappedFile.CreateFromFile(
				"data.bin", FileMode.Open, null, 100, MemoryMappedFileAccess.CopyOnWrite);
		}

		await That(Act).Throws<IOException>();
		await That(FileSystem.File.ReadAllBytes("data.bin").Length).IsEqualTo(10)
			.Because("a copy-on-write mapping must never modify the underlying file");
	}

	[Test]
	public async Task CopyOnWriteView_AfterOwnWrite_ShouldNotSeeWritesToThePrivatizedPage()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using IMemoryMappedFile mappedFile =
			FileSystem.MemoryMappedFile.CreateFromFile("data.bin");
		using IMemoryMappedViewAccessor copyOnWrite =
			mappedFile.CreateViewAccessor(0, 100, MemoryMappedFileAccess.CopyOnWrite);
		using IMemoryMappedViewAccessor readWrite = mappedFile.CreateViewAccessor(0, 100);

		copyOnWrite.Write(50, 42);
		readWrite.Write(52, 777);

		await That(copyOnWrite.ReadInt32(52)).IsEqualTo(0)
			.Because(
				"the copy-on-write view privatized the whole page when it wrote at offset 50, freezing it against later writes from other views");
	}

	[Test]
	public async Task CopyOnWriteView_BeforeOwnWrite_ShouldSeeWritesFromOtherViews()
	{
		Skip.If(FileSystem is not MockFileSystem && Test.RunsOnMac,
			"POSIX leaves it unspecified whether a copy-on-write view sees later writes made through other views; macOS snapshots the pages eagerly, while Windows and Linux privatize them lazily. The mock mirrors Windows.");

		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using IMemoryMappedFile mappedFile =
			FileSystem.MemoryMappedFile.CreateFromFile("data.bin");
		using IMemoryMappedViewAccessor copyOnWrite =
			mappedFile.CreateViewAccessor(0, 100, MemoryMappedFileAccess.CopyOnWrite);
		using IMemoryMappedViewAccessor readWrite = mappedFile.CreateViewAccessor(0, 100);

		readWrite.Write(0, 1234567);

		await That(copyOnWrite.ReadInt32(0)).IsEqualTo(1234567)
			.Because(
				"pages the copy-on-write view has not written are only privatized lazily, so they keep reflecting writes made through other views");
	}

	[Test]
	public async Task CopyOnWriteMapping_OverReadOnlyStream_ShouldBeSupported()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using FileSystemStream stream =
			FileSystem.FileStream.New("data.bin", FileMode.Open, FileAccess.Read);
		using IMemoryMappedFile mappedFile = FileSystem.MemoryMappedFile.CreateFromFile(
			stream, null, 0, MemoryMappedFileAccess.CopyOnWrite, HandleInheritability.None,
			leaveOpen: true);
		using IMemoryMappedViewAccessor accessor =
			mappedFile.CreateViewAccessor(0, 100, MemoryMappedFileAccess.CopyOnWrite);

		accessor.Write(0, 42);

		await That(accessor.ReadInt32(0)).IsEqualTo(42)
			.Because("a copy-on-write mapping never writes to the file, so it only needs read access to it");
	}

	[Test]
	public async Task CopyOnWriteMapping_WritableView_ShouldThrowUnauthorizedAccessException()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using IMemoryMappedFile mappedFile = FileSystem.MemoryMappedFile.CreateFromFile(
			"data.bin", FileMode.Open, null, 0, MemoryMappedFileAccess.CopyOnWrite);

		void Act() => mappedFile.CreateViewAccessor();

		await That(Act).Throws<UnauthorizedAccessException>()
			.Because("a copy-on-write mapping only permits Read or CopyOnWrite views");
	}

	[Test]
	public async Task CopyOnWrite_ShouldBeWritable_ButNotPersistToUnderlyingFile()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);

		using (IMemoryMappedFile mappedFile =
			FileSystem.MemoryMappedFile.CreateFromFile("data.bin"))
		{
			using IMemoryMappedViewAccessor accessor =
				mappedFile.CreateViewAccessor(0, 100, MemoryMappedFileAccess.CopyOnWrite);

			await That(accessor.CanWrite).IsTrue();

			accessor.Write(13, 1234567);

			// The write is visible through the same (copy-on-write) view ...
			await That(accessor.ReadInt32(13)).IsEqualTo(1234567);
		}

		// ... but is never persisted to the underlying file.
		byte[] bytes = FileSystem.File.ReadAllBytes("data.bin");
		await That(BitConverter.ToInt32(bytes, 13)).IsEqualTo(0);
	}

	[Test]
	public async Task CopyOnWriteView_AfterBackingStreamWasTruncated_ShouldReadZerosFromSharedPages()
	{
		Skip.IfNot(FileSystem is MockFileSystem,
			"The operating system rejects truncating a file with a user-mapped section.");

		// Two pages of 4096 bytes; the copy-on-write view only privatizes the first one.
		FileSystem.File.WriteAllBytes("data.bin", new byte[8192]);
		using FileSystemStream stream = FileSystem.FileStream.New(
			"data.bin", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
		using IMemoryMappedFile mappedFile = FileSystem.MemoryMappedFile.CreateFromFile(
			stream, null, 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None,
			leaveOpen: true);
		using IMemoryMappedViewAccessor accessor =
			mappedFile.CreateViewAccessor(0, 8192, MemoryMappedFileAccess.CopyOnWrite);

		accessor.Write(0, 42);
		stream.SetLength(4096);
		byte[] buffer = new byte[8];
		int read = accessor.ReadArray(8000, buffer, 0, 8);

		await That(read).IsEqualTo(8)
			.Because("the view spans the full capacity, which does not shrink with the file");
		await That(buffer).IsEqualTo(new byte[8])
			.Because("reads of the truncated range on a non-privatized page return zeros");
	}

	[Test]
	public async Task CopyOnWrite_ShouldNotBeVisibleToOtherViews()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using IMemoryMappedFile mappedFile =
			FileSystem.MemoryMappedFile.CreateFromFile("data.bin");
		using IMemoryMappedViewAccessor copyOnWrite =
			mappedFile.CreateViewAccessor(0, 100, MemoryMappedFileAccess.CopyOnWrite);
		using IMemoryMappedViewAccessor readWrite =
			mappedFile.CreateViewAccessor(0, 100);

		copyOnWrite.Write(0, 1234567);

		await That(readWrite.ReadInt32(0)).IsEqualTo(0);
	}
}
