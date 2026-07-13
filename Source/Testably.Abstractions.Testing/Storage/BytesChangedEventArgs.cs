using System;

namespace Testably.Abstractions.Testing.Storage;

/// <summary>
///     Event arguments of <see cref="IStorageContainer.BytesChanged" /> when only a range of the
///     file content was changed via <see cref="IStorageContainer.WriteRange(byte[], long)" />, so
///     that subscribers can apply the change without processing the complete file content.
/// </summary>
internal sealed class BytesChangedEventArgs(byte[] bytes, long offset) : EventArgs
{
	/// <summary>
	///     The changed bytes, starting at <see cref="Offset" />.
	/// </summary>
	public byte[] Bytes { get; } = bytes;

	/// <summary>
	///     The offset in the file content at which the <see cref="Bytes" /> were written.
	/// </summary>
	public long Offset { get; } = offset;
}
