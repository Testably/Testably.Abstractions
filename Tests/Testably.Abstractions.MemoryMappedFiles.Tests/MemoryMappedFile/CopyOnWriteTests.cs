using System.IO.MemoryMappedFiles;

namespace Testably.Abstractions.MemoryMappedFiles.Tests.MemoryMappedFile;

[FileSystemTests]
public class CopyOnWriteTests(FileSystemTestData testData)
	: FileSystemTestBase(testData)
{
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

			accessor.Write(0, 1234567);

			// The write is visible through the same (copy-on-write) view ...
			await That(accessor.ReadInt32(0)).IsEqualTo(1234567);
		}

		// ... but is never persisted to the underlying file.
		byte[] bytes = FileSystem.File.ReadAllBytes("data.bin");
		await That(BitConverter.ToInt32(bytes, 0)).IsEqualTo(0);
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
