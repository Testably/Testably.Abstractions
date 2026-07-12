using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using Testably.Abstractions.Internal;

namespace Testably.Abstractions;

/// <summary>
///     A view stream backed directly by the (in-memory) bytes of a
///     <see cref="FileSystemStream" /> of the <c>MockFileSystem</c>.
/// </summary>
internal sealed class MemoryMappedViewStreamMock(
	MemoryMappedViewBacking backing,
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
			=> !_disposed && _canRead;

		/// <inheritdoc cref="Stream.CanSeek" />
		public override bool CanSeek
			=> !_disposed;

		/// <inheritdoc cref="Stream.CanWrite" />
		public override bool CanWrite
			=> !_disposed && _canWrite;

		/// <inheritdoc cref="Stream.Length" />
		public override long Length
		{
			get
			{
				ThrowIfDisposed();
				return _size;
			}
		}

		/// <inheritdoc cref="Stream.Position" />
		public override long Position
		{
			get
			{
				ThrowIfDisposed();
				return _position;
			}
			set
			{
				ThrowIfDisposed();
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value,
						"Non-negative number required.");
				}

				_position = value;
			}
		}

		private readonly MemoryMappedViewBacking _backing;
		private readonly IDisposable _backingOwner;
		private readonly bool _canRead;
		private readonly bool _canWrite;
		private bool _disposed;
		private readonly long _offset;
		private long _position;
		private readonly long _size;

		public BoundedViewStream(MemoryMappedViewBacking backing, long offset, long size,
			MemoryMappedFileAccess access, IDisposable backingOwner)
		{
			_backing = backing;
			_offset = offset;
			_size = size;
			_backingOwner = backingOwner;
			_canRead = access.SupportsReading();
			_canWrite = access.SupportsWriting();
		}

		/// <inheritdoc cref="Stream.Flush()" />
		public override void Flush()
		{
			ThrowIfDisposed();
			_backing.Flush();
		}

		/// <inheritdoc cref="Stream.Read(byte[], int, int)" />
		public override int Read(byte[] buffer, int offset, int count)
		{
			EnsureValidBufferRange(buffer, offset, count);
			ThrowIfDisposed();
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
			int read = _backing.ReadAt(_offset + _position, buffer, offset, toRead);
			_position += read;
			return read;
		}

		/// <inheritdoc cref="Stream.Seek(long, SeekOrigin)" />
		public override long Seek(long offset, SeekOrigin origin)
		{
			ThrowIfDisposed();
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
			ThrowIfDisposed();
			if (!_canWrite)
			{
				throw new NotSupportedException("Stream does not support writing.");
			}

			if (_position > _size - count)
			{
				throw new NotSupportedException(
					"Unable to expand length of this stream beyond its capacity.");
			}

			_backing.WriteAt(_offset + _position, buffer, offset, count);
			_position += count;
		}

		/// <inheritdoc cref="Stream.Dispose(bool)" />
		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				try
				{
					// Pending writes are flushed to the underlying file when the view is
					// disposed, matching the real memory-mapped view, which writes its dirty
					// pages on unmap.
					_backing.Flush();
				}
				catch (ObjectDisposedException)
				{
					// The caller-owned stream (`leaveOpen: true`) was already disposed, so
					// there is nothing left to flush; disposing the view must not throw.
				}
				finally
				{
					// Releases this view's reference to the shared backing, disposing the
					// underlying stream once the memory-mapped file and all views are gone.
					// (The private pages of a copy-on-write view are plain managed memory
					// reclaimed by the garbage collector.)
					_backingOwner.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		private void ThrowIfDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(null, "Cannot access a closed Stream.");
			}
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
