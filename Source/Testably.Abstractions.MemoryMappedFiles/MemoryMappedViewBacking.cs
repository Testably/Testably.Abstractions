using System.IO;

namespace Testably.Abstractions;

/// <summary>
///     Positional read/write access to the backing <see cref="Stream" /> of a view of a
///     memory-mapped file.
/// </summary>
/// <remarks>
///     All views of a memory-mapped file share a single seekable backing stream (which can even
///     be a caller-owned <see cref="FileSystemStream" /> when it was created with
///     <c>leaveOpen: true</c>). Every operation therefore takes an explicit position, restores
///     the stream position afterwards and serializes access via the shared sync root, so views
///     never disturb the position of the shared stream and can be used concurrently.
/// </remarks>
internal sealed class MemoryMappedViewBacking(Stream stream, object syncRoot)
{
	/// <summary>
	///     Flushes the backing <see cref="Stream" />.
	/// </summary>
	public void Flush()
	{
		lock (syncRoot)
		{
			stream.Flush();
		}
	}

	/// <summary>
	///     Reads up to <paramref name="count" /> bytes at the absolute <paramref name="position" />
	///     into <paramref name="buffer" /> and returns the number of bytes read.
	/// </summary>
	public int ReadAt(long position, byte[] buffer, int offset, int count)
	{
		lock (syncRoot)
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
	public void WriteAt(long position, byte[] buffer, int offset, int count)
	{
		lock (syncRoot)
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
