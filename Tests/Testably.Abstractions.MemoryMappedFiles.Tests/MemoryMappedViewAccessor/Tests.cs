using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace Testably.Abstractions.MemoryMappedFiles.Tests.MemoryMappedViewAccessor;

[FileSystemTests]
public class Tests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	public async Task Capacity_WithExplicitSize_ShouldMatchRequestedSize()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();

		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor(10, 20);

		await That(accessor.Capacity).IsEqualTo(20L);
	}

	[Test]
	public async Task DefaultAccessor_OnReadOnlyMapping_ShouldThrowUnauthorizedAccessException()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using IMemoryMappedFile mappedFile = FileSystem.MemoryMappedFile.CreateFromFile(
			"data.bin", FileMode.Open, null, 0, MemoryMappedFileAccess.Read);

		void Act() => mappedFile.CreateViewAccessor();

		await That(Act).Throws<UnauthorizedAccessException>()
			.Because("the default view access is `ReadWrite`, which the read-only mapping does not permit");
	}

	[Test]
	public async Task DefaultAccessor_ShouldSupportReadAndWrite()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();

		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();

		await That(accessor.CanRead).IsTrue();
		await That(accessor.CanWrite).IsTrue();
	}

	[Test]
	public async Task PointerOffset_ForViewAtStart_ShouldBeZero()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();

		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor(0, 20);

		await That(accessor.PointerOffset).IsEqualTo(0L);
	}

	[Test]
	public async Task ReadArray_OnWriteOnlyAccessor_ShouldThrowNotSupportedException()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor =
			mappedFile.CreateViewAccessor(0, 100, MemoryMappedFileAccess.Write);

		void Act() => accessor.ReadArray(0, new int[1], 0, 1);

		await That(Act).Throws<NotSupportedException>();
	}

	[Test]
	public async Task ReadByte_AtCapacity_ShouldThrowArgumentOutOfRangeException()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor(0, 10);

		void Act() => accessor.ReadByte(10);

		await That(Act).Throws<ArgumentOutOfRangeException>()
			.WithParamName("position")
			.Because("the BCL distinguishes a position at or beyond the capacity from a partially fitting read");
	}

	[Test]
	public async Task Write_AtCapacity_ShouldThrowArgumentOutOfRangeException()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor(0, 10);

		void Act() => accessor.Write(10, (byte)1);

		await That(Act).Throws<ArgumentOutOfRangeException>()
			.WithParamName("position")
			.Because("the BCL distinguishes a position at or beyond the capacity from a partially fitting write");
	}

	[Test]
	public async Task Read_BeyondCapacity_ShouldThrowArgumentException()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor(0, 100);

		void Act() => accessor.ReadInt32(98);

		await That(Act).Throws<ArgumentException>()
			.WithParamName("position");
	}

	[Test]
	public async Task Read_OnWriteOnlyAccessor_ShouldThrowNotSupportedException()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor =
			mappedFile.CreateViewAccessor(0, 100, MemoryMappedFileAccess.Write);

		void Act() => accessor.ReadInt32(0);

		await That(Act).Throws<NotSupportedException>();
	}

	[Test]
	public async Task Read_WithNegativePosition_ShouldThrowArgumentOutOfRangeException()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();

		void Act() => accessor.ReadInt32(-1);

		await That(Act).Throws<ArgumentOutOfRangeException>()
			.WithParamName("position");
	}

	[Test]
	public async Task ReadAccessor_ShouldNotSupportWriting()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using IMemoryMappedFile mappedFile = FileSystem.MemoryMappedFile.CreateFromFile(
			"data.bin", FileMode.Open, null, 0, MemoryMappedFileAccess.Read);

		using IMemoryMappedViewAccessor accessor =
			mappedFile.CreateViewAccessor(0, 100, MemoryMappedFileAccess.Read);

		await That(accessor.CanRead).IsTrue();
		await That(accessor.CanWrite).IsFalse();
	}

	[Test]
	public async Task ReadArray_WhenPartiallyOutOfBounds_ShouldReturnItemsThatFit()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor(0, 100);
		int[] target = new int[10];

		int read = accessor.ReadArray(90, target, 0, target.Length);

		await That(read).IsEqualTo(2)
			.Because("only 10 bytes remain from position 90, so 2 four-byte integers fit.");
	}

	[Test]
	public async Task ReadArray_WithNegativePosition_ShouldThrowArgumentOutOfRangeException()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();

		void Act() => accessor.ReadArray(-1, new int[4], 0, 4);

		await That(Act).Throws<ArgumentOutOfRangeException>()
			.WithParamName("position");
	}

	[Test]
	public async Task ReadArray_WithNullArray_ShouldThrowArgumentNullException()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();

		void Act() => accessor.ReadArray<int>(0, null!, 0, 1);

		await That(Act).Throws<ArgumentNullException>()
			.WithParamName("array");
	}

	[Test]
	public async Task ReadWrite_Boolean_ShouldRoundtrip()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();

		accessor.Write(3, true);

		await That(accessor.ReadBoolean(3)).IsTrue();
	}

	[Test]
	public async Task ReadWrite_Byte_ShouldRoundtrip()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();

		accessor.Write(3, (byte)200);

		await That(accessor.ReadByte(3)).IsEqualTo(200);
	}

	[Test]
	public async Task ReadWrite_Char_ShouldRoundtrip()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();

		accessor.Write(3, 'Z');

		await That(accessor.ReadChar(3)).IsEqualTo('Z');
	}

	[Test]
	public async Task ReadWrite_Decimal_ShouldRoundtrip()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();

		accessor.Write(8, 79228162514.264337593543950335m);

		await That(accessor.ReadDecimal(8)).IsEqualTo(79228162514.264337593543950335m);
	}

	[Test]
	public async Task ReadWrite_Double_ShouldRoundtrip()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();

		accessor.Write(8, 2.718281828459045);

		await That(accessor.ReadDouble(8)).IsEqualTo(2.718281828459045);
	}

	[Test]
	public async Task ReadWrite_GenericStruct_ShouldRoundtrip()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();
		Point value = new()
		{
			X = 3,
			Y = 7,
		};

		accessor.Write(16, ref value);
		accessor.Read(16, out Point result);

		await That(result.X).IsEqualTo(3);
		await That(result.Y).IsEqualTo(7);
	}

	[Test]
	public async Task ReadWrite_Int16_ShouldRoundtrip()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();

		accessor.Write(3, (short)-12345);

		await That(accessor.ReadInt16(3)).IsEqualTo(-12345);
	}

	[Test]
	public async Task ReadWrite_Int32_ShouldRoundtrip()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();

		accessor.Write(8, 1234567);

		await That(accessor.ReadInt32(8)).IsEqualTo(1234567);
	}

	[Test]
	public async Task ReadWrite_Int64_ShouldRoundtrip()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();

		accessor.Write(8, -9_000_000_000L);

		await That(accessor.ReadInt64(8)).IsEqualTo(-9_000_000_000L);
	}

	[Test]
	public async Task ReadWrite_SByte_ShouldRoundtrip()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();

		accessor.Write(3, (sbyte)-42);

		await That(accessor.ReadSByte(3)).IsEqualTo(-42);
	}

	[Test]
	public async Task ReadWrite_Single_ShouldRoundtrip()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();

		accessor.Write(8, 3.14159f);

		await That(accessor.ReadSingle(8)).IsEqualTo(3.14159f);
	}

	[Test]
	public async Task ReadWrite_UInt16_ShouldRoundtrip()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();

		accessor.Write(3, (ushort)54321);

		await That(accessor.ReadUInt16(3)).IsEqualTo(54321);
	}

	[Test]
	public async Task ReadWrite_UInt32_ShouldRoundtrip()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();

		accessor.Write(8, 3_000_000_000u);

		await That(accessor.ReadUInt32(8)).IsEqualTo(3_000_000_000u);
	}

	[Test]
	public async Task ReadWrite_UInt64_ShouldRoundtrip()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();

		accessor.Write(8, 18_000_000_000_000_000_000UL);

		await That(accessor.ReadUInt64(8)).IsEqualTo(18_000_000_000_000_000_000UL);
	}

	[Test]
	public async Task ReadWriteArray_ShouldRoundtrip()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();
		int[] source = [10, 20, 30, 40, 50];

		accessor.WriteArray(8, source, 0, source.Length);
		int[] target = new int[source.Length];
		int read = accessor.ReadArray(8, target, 0, target.Length);

		await That(read).IsEqualTo(5);
		await That(target).IsEqualTo(source);
	}

	[Test]
	public async Task Write_BeyondCapacity_ShouldThrowArgumentException()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor(0, 100);

		void Act() => accessor.Write(98, 1234567);

		await That(Act).Throws<ArgumentException>()
			.WithParamName("position");
	}

	[Test]
	public async Task Write_OnReadOnlyAccessor_ShouldThrowNotSupportedException()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using IMemoryMappedFile mappedFile = FileSystem.MemoryMappedFile.CreateFromFile(
			"data.bin", FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
		using IMemoryMappedViewAccessor accessor =
			mappedFile.CreateViewAccessor(0, 100, MemoryMappedFileAccess.Read);

		void Act() => accessor.Write(0, 1234567);

		await That(Act).Throws<NotSupportedException>();
	}

	[Test]
	public async Task WriteAccessor_OnReadOnlyMapping_ShouldThrowUnauthorizedAccessException()
	{
		FileSystem.File.WriteAllBytes("data.bin", new byte[100]);
		using IMemoryMappedFile mappedFile = FileSystem.MemoryMappedFile.CreateFromFile(
			"data.bin", FileMode.Open, null, 0, MemoryMappedFileAccess.Read);

		void Act() =>
			mappedFile.CreateViewAccessor(0, 100, MemoryMappedFileAccess.ReadWrite);

		await That(Act).Throws<UnauthorizedAccessException>();
	}

	[Test]
	public async Task WriteAccessor_ShouldNotSupportReading()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();

		using IMemoryMappedViewAccessor accessor =
			mappedFile.CreateViewAccessor(0, 100, MemoryMappedFileAccess.Write);

		await That(accessor.CanRead).IsFalse();
		await That(accessor.CanWrite).IsTrue();
	}

	[Test]
	public async Task WriteArray_WhenTooLargeForRemainingCapacity_ShouldThrowAndNotWritePartially()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor(0, 100);
		int[] source = new int[30];
		for (int i = 0; i < source.Length; i++)
		{
			source[i] = i + 1;
		}

		void Act() => accessor.WriteArray(0, source, 0, source.Length);

		await That(Act).Throws<ArgumentException>()
			.Because("30 four-byte integers (120 bytes) do not fit into the 100-byte view");
		await That(accessor.ReadInt32(0)).IsEqualTo(0)
			.Because("The failure is atomic: no element was written, so the region is still zero");
		await That(accessor.ReadInt32(4)).IsEqualTo(0);
	}

	[Test]
	public async Task WriteArray_WithNegativePosition_ShouldThrowArgumentOutOfRangeException()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();

		void Act() => accessor.WriteArray(-1, new int[4], 0, 4);

		await That(Act).Throws<ArgumentOutOfRangeException>()
			.WithParamName("position");
	}

	[Test]
	public async Task WriteArray_WithNullArray_ShouldThrowArgumentNullException()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();

		void Act() => accessor.WriteArray<int>(0, null!, 0, 1);

		await That(Act).Throws<ArgumentNullException>()
			.WithParamName("array");
	}

	[Test]
	public async Task WriteGenericStruct_ShouldUseManagedSize_NotMarshalledSize()
	{
		// A struct with a single bool has a managed size of 1 byte, but a marshalled
		// size of 4 bytes. The view must use the managed size, matching the BCL, so the
		// three bytes following the bool must remain untouched.
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();
		accessor.Write(1, byte.MaxValue);
		accessor.Write(2, byte.MaxValue);
		accessor.Write(3, byte.MaxValue);
		WithBool value = new()
		{
			Flag = false,
		};

		accessor.Write(0, ref value);

		await That(accessor.ReadByte(0)).IsEqualTo(0);
		await That(accessor.ReadByte(1)).IsEqualTo(byte.MaxValue);
		await That(accessor.ReadByte(2)).IsEqualTo(byte.MaxValue);
		await That(accessor.ReadByte(3)).IsEqualTo(byte.MaxValue);
	}

	[Test]
	public async Task WriteGenericStruct_ShouldUseSequentialLayout()
	{
		using IMemoryMappedFile mappedFile = CreateMappedFile();
		using IMemoryMappedViewAccessor accessor = mappedFile.CreateViewAccessor();
		Point value = new()
		{
			X = 111,
			Y = 222,
		};

		accessor.Write(16, ref value);

		await That(accessor.ReadInt32(16)).IsEqualTo(111);
		await That(accessor.ReadInt32(20)).IsEqualTo(222);
	}

	private IMemoryMappedFile CreateMappedFile(int size = 100, string path = "data.bin")
	{
		FileSystem.File.WriteAllBytes(path, new byte[size]);
		return FileSystem.MemoryMappedFile.CreateFromFile(path);
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct Point
	{
		public int X;
		public int Y;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct WithBool
	{
		public bool Flag;
	}
}
