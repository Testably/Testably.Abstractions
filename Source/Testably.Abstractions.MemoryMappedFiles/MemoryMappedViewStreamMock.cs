using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Testably.Abstractions;

/// <summary>
///     A view stream backed directly by the (in-memory) bytes of a
///     <see cref="FileSystemStream" /> of the <c>MockFileSystem</c>.
/// </summary>
internal sealed class MemoryMappedViewStreamMock(
	Stream backing,
	long offset,
	long size,
	MemoryMappedFileAccess access,
	IDisposable backingOwner)
	: MemoryMappedFileSystemViewStream(new BoundedViewStream(backing, offset, size, access,
		backingOwner))
{
	/// <inheritdoc cref="MemoryMappedFileSystemViewStream.PointerOffset" />
	public override long PointerOffset
		=> 0;

	private sealed class BoundedViewStream : Stream
	{
		/// <inheritdoc cref="Stream.CanRead" />
		public override bool CanRead
			=> _canRead;

		/// <inheritdoc cref="Stream.CanSeek" />
		public override bool CanSeek
			=> true;

		/// <inheritdoc cref="Stream.CanWrite" />
		public override bool CanWrite
			=> _canWrite;

		/// <inheritdoc cref="Stream.Length" />
		public override long Length
			=> _size;

		/// <inheritdoc cref="Stream.Position" />
		public override long Position
		{
			get => _position;
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value,
						"Non-negative number required.");
				}

				_position = value;
			}
		}

		private readonly Stream _backing;
		private readonly IDisposable _backingOwner;
		private readonly bool _canRead;
		private readonly bool _canWrite;
		private readonly long _offset;
		private long _position;
		private readonly long _size;

		public BoundedViewStream(Stream backing, long offset, long size,
			MemoryMappedFileAccess access, IDisposable backingOwner)
		{
			_backing = backing;
			_offset = offset;
			_size = size;
			_backingOwner = backingOwner;
			_canRead = access is not MemoryMappedFileAccess.Write;
			_canWrite = access is not (MemoryMappedFileAccess.Read
				or MemoryMappedFileAccess.ReadExecute);
		}

		/// <inheritdoc cref="Stream.Flush()" />
		public override void Flush()
			=> _backing.Flush();

		/// <inheritdoc cref="Stream.Read(byte[], int, int)" />
		public override int Read(byte[] buffer, int offset, int count)
		{
			EnsureValidBufferRange(buffer, offset, count);
			if (!_canRead)
			{
				throw new NotSupportedException("Stream does not support reading.");
			}

			long remaining = _size - _position;
			if (remaining <= 0)
			{
				return 0;
			}

			int toRead = (int)Math.Min(count, remaining);
			_backing.Position = _offset + _position;
			int read = _backing.Read(buffer, offset, toRead);
			_position += read;
			return read;
		}

		/// <inheritdoc cref="Stream.Seek(long, SeekOrigin)" />
		public override long Seek(long offset, SeekOrigin origin)
		{
			long target = origin switch
			{
				SeekOrigin.Begin => offset,
				SeekOrigin.Current => _position + offset,
				SeekOrigin.End => _size + offset,
				_ => throw new ArgumentException("Invalid seek origin.", nameof(origin)),
			};
			if (target < 0)
			{
				throw new IOException(
					"An attempt was made to move the position before the beginning of the stream.");
			}

			_position = target;
			return _position;
		}

		/// <inheritdoc cref="Stream.SetLength(long)" />
		public override void SetLength(long value)
			=> throw new NotSupportedException(
				"The length of a memory-mapped view stream cannot be changed.");

		/// <inheritdoc cref="Stream.Write(byte[], int, int)" />
		public override void Write(byte[] buffer, int offset, int count)
		{
			EnsureValidBufferRange(buffer, offset, count);
			if (!_canWrite)
			{
				throw new NotSupportedException("Stream does not support writing.");
			}

			if (_position + count > _size)
			{
				throw new NotSupportedException(
					"Unable to expand length of this stream beyond its capacity.");
			}

			_backing.Position = _offset + _position;
			_backing.Write(buffer, offset, count);
			_backing.Flush();
			_position += count;
		}

		/// <inheritdoc cref="Stream.Dispose(bool)" />
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				// Releases this view's reference to the shared backing (disposing the underlying
				// stream once the memory-mapped file and all views are gone) or disposes the
				// private copy-on-write copy this view owns.
				_backingOwner.Dispose();
			}

			base.Dispose(disposing);
		}

		private static void EnsureValidBufferRange(byte[] buffer, int offset, int count)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException(nameof(buffer));
			}

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(offset), offset,
					"Non-negative number required.");
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), count,
					"Non-negative number required.");
			}

			if (buffer.Length - offset < count)
			{
				#pragma warning disable MA0015 // Matches the parameter-less BCL message for this combination.
				throw new ArgumentException(
					"Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
				#pragma warning restore MA0015
			}
		}
	}
}
