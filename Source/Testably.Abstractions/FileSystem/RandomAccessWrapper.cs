#if FEATURE_FILESYSTEM_RANDOMACCESS
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.FileSystem;

internal sealed class RandomAccessWrapper : IRandomAccess
{
	internal RandomAccessWrapper(RealFileSystem fileSystem)
	{
		FileSystem = fileSystem;
	}

	#region IRandomAccess Members

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem { get; }

#if FEATURE_RANDOMACCESS_FLUSHTODISK
	/// <inheritdoc cref="IRandomAccess.FlushToDisk(SafeFileHandle)" />
	public void FlushToDisk(SafeFileHandle handle)
		=> RandomAccess.FlushToDisk(handle);
#endif

	/// <inheritdoc cref="IRandomAccess.GetLength(SafeFileHandle)" />
	public long GetLength(SafeFileHandle handle)
		=> RandomAccess.GetLength(handle);

	/// <inheritdoc cref="IRandomAccess.Read(SafeFileHandle, Span{byte}, long)" />
	public int Read(SafeFileHandle handle, Span<byte> buffer, long fileOffset)
		=> RandomAccess.Read(handle, buffer, fileOffset);

	/// <inheritdoc cref="IRandomAccess.Read(SafeFileHandle, IReadOnlyList{Memory{byte}}, long)" />
	public long Read(SafeFileHandle handle, IReadOnlyList<Memory<byte>> buffers, long fileOffset)
		=> RandomAccess.Read(handle, buffers, fileOffset);

	/// <inheritdoc cref="IRandomAccess.ReadAsync(SafeFileHandle, Memory{byte}, long, CancellationToken)" />
	public ValueTask<int> ReadAsync(SafeFileHandle handle, Memory<byte> buffer, long fileOffset,
		CancellationToken cancellationToken = default)
		=> RandomAccess.ReadAsync(handle, buffer, fileOffset, cancellationToken);

	/// <inheritdoc cref="IRandomAccess.ReadAsync(SafeFileHandle, IReadOnlyList{Memory{byte}}, long, CancellationToken)" />
	public ValueTask<long> ReadAsync(SafeFileHandle handle, IReadOnlyList<Memory<byte>> buffers,
		long fileOffset, CancellationToken cancellationToken = default)
		=> RandomAccess.ReadAsync(handle, buffers, fileOffset, cancellationToken);

#if FEATURE_RANDOMACCESS_FLUSHTODISK
	/// <inheritdoc cref="IRandomAccess.SetLength(SafeFileHandle, long)" />
	public void SetLength(SafeFileHandle handle, long length)
		=> RandomAccess.SetLength(handle, length);
#endif

	/// <inheritdoc cref="IRandomAccess.Write(SafeFileHandle, ReadOnlySpan{byte}, long)" />
	public void Write(SafeFileHandle handle, ReadOnlySpan<byte> buffer, long fileOffset)
		=> RandomAccess.Write(handle, buffer, fileOffset);

	/// <inheritdoc cref="IRandomAccess.Write(SafeFileHandle, IReadOnlyList{ReadOnlyMemory{byte}}, long)" />
	public void Write(SafeFileHandle handle, IReadOnlyList<ReadOnlyMemory<byte>> buffers,
		long fileOffset)
		=> RandomAccess.Write(handle, buffers, fileOffset);

	/// <inheritdoc cref="IRandomAccess.WriteAsync(SafeFileHandle, ReadOnlyMemory{byte}, long, CancellationToken)" />
	public ValueTask WriteAsync(SafeFileHandle handle, ReadOnlyMemory<byte> buffer,
		long fileOffset, CancellationToken cancellationToken = default)
		=> RandomAccess.WriteAsync(handle, buffer, fileOffset, cancellationToken);

	/// <inheritdoc cref="IRandomAccess.WriteAsync(SafeFileHandle, IReadOnlyList{ReadOnlyMemory{byte}}, long, CancellationToken)" />
	public ValueTask WriteAsync(SafeFileHandle handle,
		IReadOnlyList<ReadOnlyMemory<byte>> buffers,
		long fileOffset, CancellationToken cancellationToken = default)
		=> RandomAccess.WriteAsync(handle, buffers, fileOffset, cancellationToken);

	#endregion
}
#endif
