using System;

namespace Testably.Abstractions.Testing.Storage;

/// <summary>
///     Event arguments of <see cref="IStorageContainer.BytesChanged" /> when only a range of the
///     file content was changed via <see cref="IStorageContainer.WriteRange(byte[], int, int, long)" />, so
///     that subscribers can apply the change without processing the complete file content.
/// </summary>
internal sealed class BytesChangedEventArgs(byte[] content, long offset, int count) : EventArgs
{
	/// <summary>
	///     The complete file content after the change.
	/// </summary>
	public byte[] Content { get; } = content;

	/// <summary>
	///     The number of changed bytes in the <see cref="Content" />, starting at <see cref="Offset" />.
	/// </summary>
	public int Count { get; } = count;

	/// <summary>
	///     The offset in the <see cref="Content" /> at which the bytes were written.
	/// </summary>
	public long Offset { get; } = offset;
}
