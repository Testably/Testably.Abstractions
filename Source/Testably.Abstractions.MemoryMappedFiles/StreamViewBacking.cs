using System.IO;

namespace Testably.Abstractions;

/// <summary>
///     Positional read/write access to the backing <see cref="Stream" /> shared by all views of a
///     memory-mapped file.
/// </summary>
/// <remarks>
///     All views of a memory-mapped file share a single seekable backing stream (which can even
///     be a caller-owned <see cref="FileSystemStream" /> when it was created with
///     <c>leaveOpen: true</c>). Every operation therefore restores the stream position afterwards
///     and serializes access via an internal lock, so views never disturb the position of the
///     shared stream and can be used concurrently.
/// </remarks>
internal sealed class StreamViewBacking(Stream stream) : MemoryMappedViewBacking
{
#if NET9_0_OR_GREATER
	private readonly System.Threading.Lock _lock = new();
#else
	private readonly object _lock = new();
#endif

	/// <inheritdoc cref="MemoryMappedViewBacking.Flush()" />
	public override void Flush()
	{
		lock (_lock)
		{
			stream.Flush();
		}
	}

	/// <inheritdoc cref="MemoryMappedViewBacking.ReadAt(long, byte[], int, int)" />
	public override int ReadAt(long position, byte[] buffer, int offset, int count)
	{
		lock (_lock)
		{
			long previousPosition = stream.Position;
			try
			{
				stream.Position = position;
				int read = 0;
				while (read < count)
				{
					int r = stream.Read(buffer, offset + read, count - read);
					if (r == 0)
					{
						break;
					}

					read += r;
				}

				return read;
			}
			finally
			{
				stream.Position = previousPosition;
			}
		}
	}

	/// <summary>
	///     Writes the given bytes at the absolute <paramref name="position" /> and flushes the
	///     backing <see cref="Stream" />, so that the change is immediately visible to other
	///     readers of the file (matching the coherence of real memory-mapped views).
	/// </summary>
	public override void WriteAt(long position, byte[] buffer, int offset, int count)
	{
		lock (_lock)
		{
			long previousPosition = stream.Position;
			try
			{
				stream.Position = position;
				stream.Write(buffer, offset, count);
				stream.Flush();
			}
			finally
			{
				stream.Position = previousPosition;
			}
		}
	}
}
