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
