using System.IO.MemoryMappedFiles;

namespace Testably.Abstractions;

internal sealed class MemoryMappedViewAccessorWrapper(
	IFileSystem fileSystem,
	MemoryMappedViewAccessor instance)
	: IMemoryMappedViewAccessor
{
	#region IMemoryMappedViewAccessor Members

	/// <inheritdoc cref="IMemoryMappedViewAccessor.CanRead" />
	public bool CanRead
		=> instance.CanRead;

	/// <inheritdoc cref="IMemoryMappedViewAccessor.CanWrite" />
	public bool CanWrite
		=> instance.CanWrite;

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Capacity" />
	public long Capacity
		=> instance.Capacity;

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem { get; } = fileSystem;

	/// <inheritdoc cref="IMemoryMappedViewAccessor.PointerOffset" />
	public long PointerOffset
		=> instance.PointerOffset;

	/// <inheritdoc cref="System.IDisposable.Dispose()" />
	public void Dispose()
		=> instance.Dispose();

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Flush()" />
	public void Flush()
		=> instance.Flush();

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Read{T}(long, out T)" />
	public void Read<T>(long position, out T structure) where T : struct
		=> instance.Read(position, out structure);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadArray{T}(long, T[], int, int)" />
	public int ReadArray<T>(long position, T[] array, int offset, int count)
		where T : struct
		=> instance.ReadArray(position, array, offset, count);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadBoolean(long)" />
	public bool ReadBoolean(long position)
		=> instance.ReadBoolean(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadByte(long)" />
	public byte ReadByte(long position)
		=> instance.ReadByte(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadChar(long)" />
	public char ReadChar(long position)
		=> instance.ReadChar(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadDecimal(long)" />
	public decimal ReadDecimal(long position)
		=> instance.ReadDecimal(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadDouble(long)" />
	public double ReadDouble(long position)
		=> instance.ReadDouble(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadInt16(long)" />
	public short ReadInt16(long position)
		=> instance.ReadInt16(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadInt32(long)" />
	public int ReadInt32(long position)
		=> instance.ReadInt32(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadInt64(long)" />
	public long ReadInt64(long position)
		=> instance.ReadInt64(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadSByte(long)" />
	public sbyte ReadSByte(long position)
		=> instance.ReadSByte(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadSingle(long)" />
	public float ReadSingle(long position)
		=> instance.ReadSingle(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadUInt16(long)" />
	public ushort ReadUInt16(long position)
		=> instance.ReadUInt16(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadUInt32(long)" />
	public uint ReadUInt32(long position)
		=> instance.ReadUInt32(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadUInt64(long)" />
	public ulong ReadUInt64(long position)
		=> instance.ReadUInt64(position);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, bool)" />
	public void Write(long position, bool value)
		=> instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, byte)" />
	public void Write(long position, byte value)
		=> instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, char)" />
	public void Write(long position, char value)
		=> instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, decimal)" />
	public void Write(long position, decimal value)
		=> instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, double)" />
	public void Write(long position, double value)
		=> instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, short)" />
	public void Write(long position, short value)
		=> instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, int)" />
	public void Write(long position, int value)
		=> instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, long)" />
	public void Write(long position, long value)
		=> instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, sbyte)" />
	public void Write(long position, sbyte value)
		=> instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, float)" />
	public void Write(long position, float value)
		=> instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, ushort)" />
	public void Write(long position, ushort value)
		=> instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, uint)" />
	public void Write(long position, uint value)
		=> instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, ulong)" />
	public void Write(long position, ulong value)
		=> instance.Write(position, value);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write{T}(long, ref T)" />
	public void Write<T>(long position, ref T structure) where T : struct
		=> instance.Write(position, ref structure);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.WriteArray{T}(long, T[], int, int)" />
	public void WriteArray<T>(long position, T[] array, int offset, int count)
		where T : struct
		=> instance.WriteArray(position, array, offset, count);

	#endregion
}
