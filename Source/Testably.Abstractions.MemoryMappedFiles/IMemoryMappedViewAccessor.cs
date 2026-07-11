using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Testably.Abstractions;

/// <inheritdoc cref="MemoryMappedViewAccessor" />
public interface IMemoryMappedViewAccessor : IFileSystemEntity, IDisposable
{
	/// <inheritdoc cref="UnmanagedMemoryAccessor.Capacity" />
	long Capacity { get; }

	/// <inheritdoc cref="UnmanagedMemoryAccessor.CanRead" />
	bool CanRead { get; }

	/// <inheritdoc cref="UnmanagedMemoryAccessor.CanWrite" />
	bool CanWrite { get; }

	/// <inheritdoc cref="MemoryMappedViewAccessor.PointerOffset" />
	long PointerOffset { get; }

	/// <inheritdoc cref="MemoryMappedViewAccessor.Flush()" />
	void Flush();

	/// <inheritdoc cref="UnmanagedMemoryAccessor.Read{T}(long, out T)" />
	void Read<T>(long position, out T structure) where T : struct;

	/// <inheritdoc cref="UnmanagedMemoryAccessor.ReadArray{T}(long, T[], int, int)" />
	int ReadArray<T>(long position, T[] array, int offset, int count) where T : struct;

	/// <inheritdoc cref="UnmanagedMemoryAccessor.ReadBoolean(long)" />
	bool ReadBoolean(long position);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.ReadByte(long)" />
	byte ReadByte(long position);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.ReadChar(long)" />
	char ReadChar(long position);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.ReadDecimal(long)" />
	decimal ReadDecimal(long position);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.ReadDouble(long)" />
	double ReadDouble(long position);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.ReadInt16(long)" />
	short ReadInt16(long position);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.ReadInt32(long)" />
	int ReadInt32(long position);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.ReadInt64(long)" />
	long ReadInt64(long position);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.ReadSByte(long)" />
	sbyte ReadSByte(long position);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.ReadSingle(long)" />
	float ReadSingle(long position);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.ReadUInt16(long)" />
	ushort ReadUInt16(long position);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.ReadUInt32(long)" />
	uint ReadUInt32(long position);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.ReadUInt64(long)" />
	ulong ReadUInt64(long position);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.Write(long, bool)" />
	void Write(long position, bool value);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.Write(long, byte)" />
	void Write(long position, byte value);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.Write(long, char)" />
	void Write(long position, char value);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.Write(long, decimal)" />
	void Write(long position, decimal value);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.Write(long, double)" />
	void Write(long position, double value);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.Write(long, short)" />
	void Write(long position, short value);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.Write(long, int)" />
	void Write(long position, int value);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.Write(long, long)" />
	void Write(long position, long value);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.Write(long, sbyte)" />
	void Write(long position, sbyte value);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.Write(long, float)" />
	void Write(long position, float value);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.Write(long, ushort)" />
	void Write(long position, ushort value);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.Write(long, uint)" />
	void Write(long position, uint value);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.Write(long, ulong)" />
	void Write(long position, ulong value);

	/// <inheritdoc cref="UnmanagedMemoryAccessor.Write{T}(long, ref T)" />
	void Write<T>(long position, ref T structure) where T : struct;

	/// <inheritdoc cref="UnmanagedMemoryAccessor.WriteArray{T}(long, T[], int, int)" />
	void WriteArray<T>(long position, T[] array, int offset, int count) where T : struct;
}
