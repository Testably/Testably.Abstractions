using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;

namespace Testably.Abstractions;

/// <summary>
///     A view accessor backed directly by the (in-memory) bytes of a
///     <see cref="FileSystemStream" /> of the <c>MockFileSystem</c>.
/// </summary>
internal sealed class MemoryMappedViewAccessorMock : IMemoryMappedViewAccessor
{
	private readonly MemoryMappedFileAccess _access;
	private readonly IDisposable _backingOwner;
	private readonly long _offset;
	private readonly long _size;
	private readonly Stream _stream;

	public MemoryMappedViewAccessorMock(IFileSystem fileSystem, Stream stream,
		long offset, long size, MemoryMappedFileAccess access, IDisposable backingOwner)
	{
		FileSystem = fileSystem;
		_stream = stream;
		_offset = offset;
		_size = size;
		_access = access;
		_backingOwner = backingOwner;
	}

	#region IMemoryMappedViewAccessor Members

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Capacity" />
	public long Capacity
		=> _size;

	/// <inheritdoc cref="IMemoryMappedViewAccessor.CanRead" />
	public bool CanRead
		=> _access is not MemoryMappedFileAccess.Write;

	/// <inheritdoc cref="IMemoryMappedViewAccessor.CanWrite" />
	public bool CanWrite
		=> _access is not (MemoryMappedFileAccess.Read
			or MemoryMappedFileAccess.ReadExecute);

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem { get; }

	/// <inheritdoc cref="IMemoryMappedViewAccessor.PointerOffset" />
	public long PointerOffset
		=> 0;

	/// <inheritdoc cref="System.IDisposable.Dispose()" />
	public void Dispose()
		// Releases this view's reference to the shared backing (disposing the underlying stream
		// once the memory-mapped file and all views are gone) or disposes the private
		// copy-on-write copy this view owns.
		=> _backingOwner.Dispose();

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Flush()" />
	public void Flush()
		=> _stream.Flush();

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Read{T}(long, out T)" />
	public void Read<T>(long position, out T structure) where T : struct
	{
		int size = Unsafe.SizeOf<T>();
		byte[] bytes = ReadBytes(position, size);
		structure = size == 0 ? default : Unsafe.ReadUnaligned<T>(ref bytes[0]);
	}

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadArray{T}(long, T[], int, int)" />
	public int ReadArray<T>(long position, T[] array, int offset, int count)
		where T : struct
	{
		ValidateArrayArguments(array, offset, count);
		if (position < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(position), position,
				$"position ('{position}') must be a non-negative value.");
		}

		int structureSize = Unsafe.SizeOf<T>();
		int read = 0;
		for (int i = 0; i < count; i++)
		{
			long itemPosition = position + ((long)i * structureSize);
			if (itemPosition + structureSize > _size)
			{
				break;
			}

			Read(itemPosition, out T value);
			array[offset + i] = value;
			read++;
		}

		return read;
	}

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadBoolean(long)" />
	public bool ReadBoolean(long position)
		=> ReadBytes(position, 1)[0] != 0;

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadByte(long)" />
	public byte ReadByte(long position)
		=> ReadBytes(position, 1)[0];

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadChar(long)" />
	public char ReadChar(long position)
		=> (char)BitConverter.ToUInt16(ReadBytes(position, 2), 0);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadDecimal(long)" />
	public decimal ReadDecimal(long position)
	{
		byte[] bytes = ReadBytes(position, 16);
		int[] bits = new int[4];
		for (int i = 0; i < 4; i++)
		{
			bits[i] = BitConverter.ToInt32(bytes, i * 4);
		}

		return new decimal(bits);
	}

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadDouble(long)" />
	public double ReadDouble(long position)
		=> BitConverter.ToDouble(ReadBytes(position, 8), 0);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadInt16(long)" />
	public short ReadInt16(long position)
		=> BitConverter.ToInt16(ReadBytes(position, 2), 0);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadInt32(long)" />
	public int ReadInt32(long position)
		=> BitConverter.ToInt32(ReadBytes(position, 4), 0);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadInt64(long)" />
	public long ReadInt64(long position)
		=> BitConverter.ToInt64(ReadBytes(position, 8), 0);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadSByte(long)" />
	public sbyte ReadSByte(long position)
		=> (sbyte)ReadBytes(position, 1)[0];

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadSingle(long)" />
	public float ReadSingle(long position)
		=> BitConverter.ToSingle(ReadBytes(position, 4), 0);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadUInt16(long)" />
	public ushort ReadUInt16(long position)
		=> BitConverter.ToUInt16(ReadBytes(position, 2), 0);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadUInt32(long)" />
	public uint ReadUInt32(long position)
		=> BitConverter.ToUInt32(ReadBytes(position, 4), 0);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.ReadUInt64(long)" />
	public ulong ReadUInt64(long position)
		=> BitConverter.ToUInt64(ReadBytes(position, 8), 0);

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, bool)" />
	public void Write(long position, bool value)
		=> WriteBytes(position, new[] { (byte)(value ? 1 : 0) });

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, byte)" />
	public void Write(long position, byte value)
		=> WriteBytes(position, new[] { value });

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, char)" />
	public void Write(long position, char value)
		=> WriteBytes(position, BitConverter.GetBytes(value));

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, decimal)" />
	public void Write(long position, decimal value)
	{
		int[] bits = decimal.GetBits(value);
		byte[] bytes = new byte[16];
		for (int i = 0; i < 4; i++)
		{
			BitConverter.GetBytes(bits[i]).CopyTo(bytes, i * 4);
		}

		WriteBytes(position, bytes);
	}

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, double)" />
	public void Write(long position, double value)
		=> WriteBytes(position, BitConverter.GetBytes(value));

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, short)" />
	public void Write(long position, short value)
		=> WriteBytes(position, BitConverter.GetBytes(value));

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, int)" />
	public void Write(long position, int value)
		=> WriteBytes(position, BitConverter.GetBytes(value));

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, long)" />
	public void Write(long position, long value)
		=> WriteBytes(position, BitConverter.GetBytes(value));

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, sbyte)" />
	public void Write(long position, sbyte value)
		=> WriteBytes(position, new[] { (byte)value });

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, float)" />
	public void Write(long position, float value)
		=> WriteBytes(position, BitConverter.GetBytes(value));

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, ushort)" />
	public void Write(long position, ushort value)
		=> WriteBytes(position, BitConverter.GetBytes(value));

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, uint)" />
	public void Write(long position, uint value)
		=> WriteBytes(position, BitConverter.GetBytes(value));

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write(long, ulong)" />
	public void Write(long position, ulong value)
		=> WriteBytes(position, BitConverter.GetBytes(value));

	/// <inheritdoc cref="IMemoryMappedViewAccessor.Write{T}(long, ref T)" />
	public void Write<T>(long position, ref T structure) where T : struct
	{
		int size = Unsafe.SizeOf<T>();
		byte[] bytes = new byte[size];
		if (size > 0)
		{
			Unsafe.WriteUnaligned(ref bytes[0], structure);
		}

		WriteBytes(position, bytes);
	}

	/// <inheritdoc cref="IMemoryMappedViewAccessor.WriteArray{T}(long, T[], int, int)" />
	public void WriteArray<T>(long position, T[] array, int offset, int count)
		where T : struct
	{
		ValidateArrayArguments(array, offset, count);
		if (!CanWrite)
		{
			throw new NotSupportedException("Accessor does not support writing.");
		}

		if (position < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(position), position,
				$"position ('{position}') must be a non-negative value.");
		}

		int structureSize = Unsafe.SizeOf<T>();
		// Validate up-front so the write is atomic: the BCL rejects the whole call before writing
		// any element when the array does not fit, rather than writing partially and then failing.
		if (count > 0 && position > _size - ((long)count * structureSize))
		{
#pragma warning disable MA0015 // Matches the parameter-less BCL message for this combination.
			throw new ArgumentException(
				"There are not enough bytes remaining in the accessor to write at this position.",
				nameof(array));
#pragma warning restore MA0015
		}

		for (int i = 0; i < count; i++)
		{
			T value = array[offset + i];
			Write(position + ((long)i * structureSize), ref value);
		}
	}

	#endregion

	private static void ValidateArrayArguments<T>(T[] array, int offset, int count)
	{
		if (array == null)
		{
			throw new ArgumentNullException(nameof(array));
		}

		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(offset), offset,
				$"offset ('{offset}') must be a non-negative value.");
		}

		if (count < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(count), count,
				$"count ('{count}') must be a non-negative value.");
		}

		if (array.Length - offset < count)
		{
#pragma warning disable MA0015 // Matches the parameter-less BCL message for this combination.
			throw new ArgumentException(
				"The number of bytes requested does not fit into the buffer.");
#pragma warning restore MA0015
		}
	}

	private byte[] ReadBytes(long position, int count)
	{
		if (!CanRead)
		{
			throw new NotSupportedException("Accessor does not support reading.");
		}

		EnsureInBounds(position, count, forReading: true);
		_stream.Position = _offset + position;
		byte[] buffer = new byte[count];
		int read = 0;
		while (read < count)
		{
			int r = _stream.Read(buffer, read, count - read);
			if (r == 0)
			{
				break;
			}

			read += r;
		}

		return buffer;
	}

	private void WriteBytes(long position, byte[] bytes)
	{
		if (!CanWrite)
		{
			throw new NotSupportedException("Accessor does not support writing.");
		}

		EnsureInBounds(position, bytes.Length, forReading: false);
		_stream.Position = _offset + position;
		_stream.Write(bytes, 0, bytes.Length);
		_stream.Flush();
	}

	private void EnsureInBounds(long position, int count, bool forReading)
	{
		if (position < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(position), position,
				$"position ('{position}') must be a non-negative value.");
		}

		if (position > _size - count)
		{
			throw new ArgumentException(
				forReading
					? "There are not enough bytes remaining in the accessor to read at this position."
					: "There are not enough bytes remaining in the accessor to write at this position.",
				nameof(position));
		}
	}
}
