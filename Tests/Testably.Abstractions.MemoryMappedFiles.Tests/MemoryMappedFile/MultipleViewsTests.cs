using System.IO;

namespace Testably.Abstractions.MemoryMappedFiles.Tests.MemoryMappedFile;

[FileSystemTests]
public class MultipleViewsTests(FileSystemTestData testData)
	: FileSystemTestBase(testData)
{
	[Test]
	public async Task ViewsAtDifferentOffsets_ShouldNotInterfere()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using IMemoryMappedFile mappedFile =
			FileSystem.MemoryMappedFile.CreateFromFile("data.bin");
		using IMemoryMappedViewAccessor first = mappedFile.CreateViewAccessor(0, 40);
		using IMemoryMappedViewAccessor second = mappedFile.CreateViewAccessor(40, 40);

		first.Write(0, 111);
		second.Write(0, 222);

		await That(first.ReadInt32(0)).IsEqualTo(111);
		await That(second.ReadInt32(0)).IsEqualTo(222);
	}

	[Test]
	public async Task ViewsOverSameRegion_ShouldShareData()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using IMemoryMappedFile mappedFile =
			FileSystem.MemoryMappedFile.CreateFromFile("data.bin");
		using IMemoryMappedViewAccessor writer = mappedFile.CreateViewAccessor();
		using IMemoryMappedViewAccessor reader = mappedFile.CreateViewAccessor();

		writer.Write(0, 987654);

		await That(reader.ReadInt32(0)).IsEqualTo(987654);
	}

	[Test]
	public async Task ViewStreamAndAccessor_ShouldShareData()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using IMemoryMappedFile mappedFile =
			FileSystem.MemoryMappedFile.CreateFromFile("data.bin");
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();

		using (Stream stream = mappedFile.CreateViewStream(0, 50))
		{
			stream.Write([1, 2, 3, 4,], 0, 4);
			stream.Flush();
		}

		await That(accessor.ReadByte(0)).IsEqualTo(1);
		await That(accessor.ReadByte(3)).IsEqualTo(4);
	}
}
