#if FEATURE_FILESYSTEM_RANDOMACCESS
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO.Abstractions;

/// <summary>
///     Abstractions for <see cref="RandomAccess" />.
/// </summary>
public interface IRandomAccess : IFileSystemEntity
{
#if FEATURE_RANDOMACCESS_FLUSHTODISK
	/// <inheritdoc cref="RandomAccess.FlushToDisk(SafeFileHandle)" />
	void FlushToDisk(SafeFileHandle handle);
#endif

	/// <inheritdoc cref="RandomAccess.GetLength(SafeFileHandle)" />
	long GetLength(SafeFileHandle handle);

	/// <inheritdoc cref="RandomAccess.Read(SafeFileHandle, Span{byte}, long)" />
	int Read(SafeFileHandle handle, Span<byte> buffer, long fileOffset);

	/// <inheritdoc cref="RandomAccess.Read(SafeFileHandle, IReadOnlyList{Memory{byte}}, long)" />
	long Read(SafeFileHandle handle, IReadOnlyList<Memory<byte>> buffers, long fileOffset);

	/// <inheritdoc cref="RandomAccess.ReadAsync(SafeFileHandle, Memory{byte}, long, CancellationToken)" />
	ValueTask<int> ReadAsync(SafeFileHandle handle, Memory<byte> buffer, long fileOffset,
		CancellationToken cancellationToken = default);

	/// <inheritdoc cref="RandomAccess.ReadAsync(SafeFileHandle, IReadOnlyList{Memory{byte}}, long, CancellationToken)" />
	ValueTask<long> ReadAsync(SafeFileHandle handle, IReadOnlyList<Memory<byte>> buffers,
		long fileOffset, CancellationToken cancellationToken = default);

#if FEATURE_RANDOMACCESS_FLUSHTODISK
	/// <inheritdoc cref="RandomAccess.SetLength(SafeFileHandle, long)" />
	void SetLength(SafeFileHandle handle, long length);
#endif

	/// <inheritdoc cref="RandomAccess.Write(SafeFileHandle, ReadOnlySpan{byte}, long)" />
	void Write(SafeFileHandle handle, ReadOnlySpan<byte> buffer, long fileOffset);

	/// <inheritdoc cref="RandomAccess.Write(SafeFileHandle, IReadOnlyList{ReadOnlyMemory{byte}}, long)" />
	void Write(SafeFileHandle handle, IReadOnlyList<ReadOnlyMemory<byte>> buffers,
		long fileOffset);

	/// <inheritdoc cref="RandomAccess.WriteAsync(SafeFileHandle, ReadOnlyMemory{byte}, long, CancellationToken)" />
	ValueTask WriteAsync(SafeFileHandle handle, ReadOnlyMemory<byte> buffer, long fileOffset,
		CancellationToken cancellationToken = default);

	/// <inheritdoc cref="RandomAccess.WriteAsync(SafeFileHandle, IReadOnlyList{ReadOnlyMemory{byte}}, long, CancellationToken)" />
	ValueTask WriteAsync(SafeFileHandle handle, IReadOnlyList<ReadOnlyMemory<byte>> buffers,
		long fileOffset, CancellationToken cancellationToken = default);
}
#endif
