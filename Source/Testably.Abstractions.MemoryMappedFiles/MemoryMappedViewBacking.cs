namespace Testably.Abstractions;

/// <summary>
///     Positional read/write access to the bytes backing a view of a memory-mapped file.
/// </summary>
/// <remarks>
///     Views never own a position: every operation takes an explicit position, so views can be
///     used concurrently and never disturb the state their backing is built on.
///     <see cref="StreamViewBacking" /> multiplexes the single shared (possibly caller-owned)
///     stream; <see cref="CopyOnWriteViewBacking" /> adds page privatization on top of it.
/// </remarks>
internal abstract class MemoryMappedViewBacking
{
	/// <summary>
	///     Flushes any written bytes to the underlying file.
	/// </summary>
	public abstract void Flush();

	/// <summary>
	///     Reads <paramref name="count" /> bytes at the absolute <paramref name="position" />
	///     into <paramref name="buffer" />. A range beyond the end of the underlying content
	///     (because the caller truncated a shared backing stream) is zero-filled, matching the
	///     zeroed pages the real memory-mapped view exposes.
	/// </summary>
	public abstract void ReadAt(long position, byte[] buffer, int offset, int count);

	/// <summary>
	///     Writes the given bytes at the absolute <paramref name="position" />.
	/// </summary>
	public abstract void WriteAt(long position, byte[] buffer, int offset, int count);
}
