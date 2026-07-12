using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using Skip = Testably.Abstractions.TestHelpers.Skip;

namespace Testably.Abstractions.MemoryMappedFiles.Tests.MemoryMappedFile;

[FileSystemTests]
public class Tests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	public async Task CreateFromFile_CreateViewAccessor_ShouldRoundtripGenericStruct()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		Point value = new()
		{
			X = 3,
			Y = 7,
		};

		using IMemoryMappedFile mappedFile =
			FileSystem.MemoryMappedFile.CreateFromFile("data.bin");
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();

		accessor.Write(16, ref value);
		accessor.Read(16, out Point result);

		await That(result.X).IsEqualTo(3);
		await That(result.Y).IsEqualTo(7);
	}

	[Test]
	public async Task CreateFromFile_CreateViewAccessor_ShouldRoundtripPrimitive()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);

		using IMemoryMappedFile mappedFile =
			FileSystem.MemoryMappedFile.CreateFromFile("data.bin");
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();

		accessor.Write(8, 1234567);

		await That(accessor.ReadInt32(8)).IsEqualTo(1234567);
	}

	[Test]
	public async Task CreateFromFile_CreateViewStream_ShouldRoundtrip()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		byte[] payload = [1, 2, 3, 4, 5,];

		using IMemoryMappedFile mappedFile =
			FileSystem.MemoryMappedFile.CreateFromFile("data.bin");

		using (Stream writeStream = mappedFile.CreateViewStream())
		{
			writeStream.Write(payload, 0, payload.Length);
			writeStream.Flush();
		}

		byte[] result = new byte[payload.Length];
		using (Stream readStream = mappedFile.CreateViewStream())
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
	public async Task CreateFromFile_WithFileSystemStream_ShouldRoundtrip()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);

		using FileSystemStream stream =
			FileSystem.FileStream.New("data.bin", FileMode.Open, FileAccess.ReadWrite);
		using IMemoryMappedFile mappedFile = FileSystem.MemoryMappedFile.CreateFromFile(
			stream, null, 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None,
			leaveOpen: true);
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();

		accessor.Write(0, 98765);

		await That(accessor.ReadInt32(0)).IsEqualTo(98765);
	}

	[Test]
	public async Task CreateNewAndOpenExisting_OnRealFileSystem_ShouldRoundtrip()
	{
		Skip.If(FileSystem is MockFileSystem);
		Skip.IfNot(Test.RunsOnWindows);

		string mapName = Guid.NewGuid().ToString("N");

		using IMemoryMappedFile created =
			FileSystem.MemoryMappedFile.CreateNew(mapName, 1024);
		using (IMemoryMappedViewAccessor writeAccessor = created.CreateViewAccessor())
		{
			writeAccessor.Write(0, 4242);
		}

		#pragma warning disable CA1416
		using IMemoryMappedFile opened =
			FileSystem.MemoryMappedFile.OpenExisting(mapName);
		#pragma warning restore CA1416
		using IMemoryMappedViewAccessor readAccessor = opened.CreateViewAccessor();

		await That(readAccessor.ReadInt32(0)).IsEqualTo(4242);
	}

	[Test]
	public async Task FilelessFactoryMethods_OnMockFileSystem_ShouldThrowNotSupported()
	{
		Skip.IfNot(FileSystem is MockFileSystem);

		string mapName = Guid.NewGuid().ToString("N");

		void CreateNew()
			=> FileSystem.MemoryMappedFile.CreateNew(mapName, 1024);

		#pragma warning disable CA1416
		void CreateOrOpen()
			=> FileSystem.MemoryMappedFile.CreateOrOpen(mapName, 1024);

		void OpenExisting()
			=> FileSystem.MemoryMappedFile.OpenExisting(mapName);
		#pragma warning restore CA1416

		await That(CreateNew).Throws<NotSupportedException>();
		await That(CreateOrOpen).Throws<NotSupportedException>();
		await That(OpenExisting).Throws<NotSupportedException>();
	}

	[Test]
	public async Task FileSystemExtension_ShouldBeSet()
	{
		IMemoryMappedFileFactory result = FileSystem.MemoryMappedFile;

		await That(result.FileSystem).IsSameAs(FileSystem);
	}

	[StructLayout(LayoutKind.Sequential)]
	private record struct Point
	{
		public int X;
		public int Y;
	}
}
