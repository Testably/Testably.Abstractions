using System.IO.MemoryMappedFiles;

namespace Testably.Abstractions;

internal sealed class MemoryMappedViewAccessorWrapper : IMemoryMappedViewAccessor
{
	private readonly MemoryMappedViewAccessor _instance;

	public MemoryMappedViewAccessorWrapper(IFileSystem fileSystem,
		MemoryMappedViewAccessor instance)
	{
		_instance = instance;
		FileSystem = fileSystem;
	}

	#region IMemoryMappedViewAccessor Members

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Capacity" />
	public long Capacity
		=> _instance.Capacity;

	/// <inheritdoc cref="IMemoryMappedViewAccessor.CanRead" />
	public bool CanRead
		=> _instance.CanRead;

	/// <inheritdoc cref="IMemoryMappedViewAccessor.CanWrite" />
	public bool CanWrite
		=> _instance.CanWrite;

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem { get; }

	/// <inheritdoc cref="IMemoryMappedViewAccessor.PointerOffset" />
	public long PointerOffset
		=> _instance.PointerOffset;

	/// <inheritdoc cref="System.IDisposable.Dispose()" />
	public void Dispose()
		=> _instance.Dispose();

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Flush()" />
	public void Flush()
		=> _instance.Flush();

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Read{T}(long, out T)" />
	public void Read<T>(long position, out T structure) where T : struct
		=> _instance.Read(position, out structure);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadArray{T}(long, T[], int, int)" />
	public int ReadArray<T>(long position, T[] array, int offset, int count)
		where T : struct
		=> _instance.ReadArray(position, array, offset, count);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadBoolean(long)" />
	public bool ReadBoolean(long position)
		=> _instance.ReadBoolean(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadByte(long)" />
	public byte ReadByte(long position)
		=> _instance.ReadByte(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadChar(long)" />
	public char ReadChar(long position)
		=> _instance.ReadChar(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadDecimal(long)" />
	public decimal ReadDecimal(long position)
		=> _instance.ReadDecimal(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadDouble(long)" />
	public double ReadDouble(long position)
		=> _instance.ReadDouble(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadInt16(long)" />
	public short ReadInt16(long position)
		=> _instance.ReadInt16(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadInt32(long)" />
	public int ReadInt32(long position)
		=> _instance.ReadInt32(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadInt64(long)" />
	public long ReadInt64(long position)
		=> _instance.ReadInt64(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadSByte(long)" />
	public sbyte ReadSByte(long position)
		=> _instance.ReadSByte(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadSingle(long)" />
	public float ReadSingle(long position)
		=> _instance.ReadSingle(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadUInt16(long)" />
	public ushort ReadUInt16(long position)
		=> _instance.ReadUInt16(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadUInt32(long)" />
	public uint ReadUInt32(long position)
		=> _instance.ReadUInt32(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadUInt64(long)" />
	public ulong ReadUInt64(long position)
		=> _instance.ReadUInt64(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, bool)" />
	public void Write(long position, bool value)
		=> _instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, byte)" />
	public void Write(long position, byte value)
		=> _instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, char)" />
	public void Write(long position, char value)
		=> _instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, decimal)" />
	public void Write(long position, decimal value)
		=> _instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, double)" />
	public void Write(long position, double value)
		=> _instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, short)" />
	public void Write(long position, short value)
		=> _instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, int)" />
	public void Write(long position, int value)
		=> _instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, long)" />
	public void Write(long position, long value)
		=> _instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, sbyte)" />
	public void Write(long position, sbyte value)
		=> _instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, float)" />
	public void Write(long position, float value)
		=> _instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, ushort)" />
	public void Write(long position, ushort value)
		=> _instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, uint)" />
	public void Write(long position, uint value)
		=> _instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, ulong)" />
	public void Write(long position, ulong value)
		=> _instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write{T}(long, ref T)" />
	public void Write<T>(long position, ref T structure) where T : struct
		=> _instance.Write(position, ref structure);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.WriteArray{T}(long, T[], int, int)" />
	public void WriteArray<T>(long position, T[] array, int offset, int count)
		where T : struct
		=> _instance.WriteArray(position, array, offset, count);

	#endregion
}
