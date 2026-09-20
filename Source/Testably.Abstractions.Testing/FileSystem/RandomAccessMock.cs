#if FEATURE_FILESYSTEM_RANDOMACCESS
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.FileSystem;

internal sealed class RandomAccessMock : IRandomAccess
{
	private readonly MockFileSystem _fileSystem;

	private readonly System.Runtime.CompilerServices.ConditionalWeakTable<IStorageContainer, object>
		_gates = new();

	internal RandomAccessMock(MockFileSystem fileSystem)
	{
		_fileSystem = fileSystem;
	}

	#region IRandomAccess Members

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem
		=> _fileSystem;

#if FEATURE_RANDOMACCESS_FLUSHTODISK
	/// <inheritdoc cref="IRandomAccess.FlushToDisk(SafeFileHandle)" />
	public void FlushToDisk(SafeFileHandle handle)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.RandomAccess.RegisterMethod(nameof(FlushToDisk), handle);

		// Nothing to flush without a write-back cache, but the handle is still resolved and the call counted, so a
		// test can assert that a durability barrier was requested.
		_ = GetContainer(handle, FileAccess.Write);
	}
#endif

	/// <inheritdoc cref="IRandomAccess.GetLength(SafeFileHandle)" />
	public long GetLength(SafeFileHandle handle)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.RandomAccess.RegisterMethod(nameof(GetLength), handle);

		// Querying the length is a metadata operation: it works on a write-only handle too.
		return GetContainer(handle, required: null).GetBytes().Length;
	}

	/// <inheritdoc cref="IRandomAccess.Read(SafeFileHandle, Span{byte}, long)" />
	public int Read(SafeFileHandle handle, Span<byte> buffer, long fileOffset)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.RandomAccess.RegisterMethod(nameof(Read), handle, fileOffset);

		return ReadInto(handle, buffer, fileOffset);
	}

	/// <inheritdoc cref="IRandomAccess.Read(SafeFileHandle, IReadOnlyList{Memory{byte}}, long)" />
	public long Read(SafeFileHandle handle, IReadOnlyList<Memory<byte>> buffers, long fileOffset)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.RandomAccess.RegisterMethod(nameof(Read), handle, fileOffset);

		if (buffers is null)
		{
			throw new ArgumentNullException(nameof(buffers));
		}

		return ReadInto(handle, buffers, fileOffset);
	}

	/// <inheritdoc cref="IRandomAccess.ReadAsync(SafeFileHandle, Memory{byte}, long, CancellationToken)" />
	public ValueTask<int> ReadAsync(SafeFileHandle handle, Memory<byte> buffer, long fileOffset,
		CancellationToken cancellationToken = default)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return ValueTask.FromCanceled<int>(cancellationToken);
		}

		using IDisposable registration = _fileSystem.StatisticsRegistration
			.RandomAccess.RegisterMethod(nameof(ReadAsync), handle, fileOffset,
				cancellationToken);

		return new ValueTask<int>(Read(handle, buffer.Span, fileOffset));
	}

	/// <inheritdoc cref="IRandomAccess.ReadAsync(SafeFileHandle, IReadOnlyList{Memory{byte}}, long, CancellationToken)" />
	public ValueTask<long> ReadAsync(SafeFileHandle handle, IReadOnlyList<Memory<byte>> buffers,
		long fileOffset, CancellationToken cancellationToken = default)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return ValueTask.FromCanceled<long>(cancellationToken);
		}

		using IDisposable registration = _fileSystem.StatisticsRegistration
			.RandomAccess.RegisterMethod(nameof(ReadAsync), handle, fileOffset,
				cancellationToken);

		return new ValueTask<long>(Read(handle, buffers, fileOffset));
	}

#if FEATURE_RANDOMACCESS_FLUSHTODISK
	/// <inheritdoc cref="IRandomAccess.SetLength(SafeFileHandle, long)" />
	public void SetLength(SafeFileHandle handle, long length)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.RandomAccess.RegisterMethod(nameof(SetLength), handle, length);

		if (length < 0)
		{
			throw ExceptionFactory.NonNegativeNumberRequired("length");
		}

		IStorageContainer container = GetContainer(handle, FileAccess.Write);
		lock (Gate(container))
		{
			byte[] bytes = container.GetBytes();
			byte[] resized = new byte[length];
			Array.Copy(bytes, resized, Math.Min(bytes.Length, length));
			container.WriteBytes(resized);
		}
	}
#endif

	/// <inheritdoc cref="IRandomAccess.Write(SafeFileHandle, ReadOnlySpan{byte}, long)" />
	public void Write(SafeFileHandle handle, ReadOnlySpan<byte> buffer, long fileOffset)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.RandomAccess.RegisterMethod(nameof(Write), handle, fileOffset);

		WriteBytes(handle, buffer.ToArray(), fileOffset);
	}

	/// <inheritdoc cref="IRandomAccess.Write(SafeFileHandle, IReadOnlyList{ReadOnlyMemory{byte}}, long)" />
	public void Write(SafeFileHandle handle, IReadOnlyList<ReadOnlyMemory<byte>> buffers,
		long fileOffset)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.RandomAccess.RegisterMethod(nameof(Write), handle, fileOffset);

		if (buffers is null)
		{
			throw new ArgumentNullException(nameof(buffers));
		}

		WriteBytes(handle, Gather(buffers), fileOffset);
	}

	/// <inheritdoc cref="IRandomAccess.WriteAsync(SafeFileHandle, ReadOnlyMemory{byte}, long, CancellationToken)" />
	public ValueTask WriteAsync(SafeFileHandle handle, ReadOnlyMemory<byte> buffer,
		long fileOffset, CancellationToken cancellationToken = default)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return ValueTask.FromCanceled(cancellationToken);
		}

		using IDisposable registration = _fileSystem.StatisticsRegistration
			.RandomAccess.RegisterMethod(nameof(WriteAsync), handle, fileOffset,
				cancellationToken);

		Write(handle, buffer.Span, fileOffset);
		return default;
	}

	/// <inheritdoc cref="IRandomAccess.WriteAsync(SafeFileHandle, IReadOnlyList{ReadOnlyMemory{byte}}, long, CancellationToken)" />
	public ValueTask WriteAsync(SafeFileHandle handle,
		IReadOnlyList<ReadOnlyMemory<byte>> buffers,
		long fileOffset, CancellationToken cancellationToken = default)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return ValueTask.FromCanceled(cancellationToken);
		}

		using IDisposable registration = _fileSystem.StatisticsRegistration
			.RandomAccess.RegisterMethod(nameof(WriteAsync), handle, fileOffset,
				cancellationToken);

		Write(handle, buffers, fileOffset);
		return default;
	}

	#endregion

	private static byte[] Gather(IReadOnlyList<ReadOnlyMemory<byte>> buffers)
	{
		int length = 0;
		foreach (ReadOnlyMemory<byte> buffer in buffers)
		{
			length += buffer.Length;
		}

		byte[] bytes = new byte[length];
		int position = 0;
		foreach (ReadOnlyMemory<byte> buffer in buffers)
		{
			buffer.Span.CopyTo(bytes.AsSpan(position));
			position += buffer.Length;
		}

		return bytes;
	}

	private IStorageContainer GetContainer(SafeFileHandle handle, FileAccess? required)
		=> Resolve(handle, required).Container;

	private (IStorageContainer Container, FileMode Mode) Resolve(
		SafeFileHandle handle, FileAccess? required)
	{
		if (handle is null)
		{
			throw new ArgumentNullException(nameof(handle));
		}

		if (handle.IsInvalid)
		{
			throw ExceptionFactory.HandleIsInvalid();
		}

		(IStorageContainer container, FileAccess access, FileMode mode) =
			_fileSystem.SafeFileHandleRegistry.GetContainer(handle);

		if (required is { } requiredAccess && !access.HasFlag(requiredAccess))
		{
			throw ExceptionFactory.AccessToPathDenied();
		}

		return (container, mode);
	}

	private int ReadInto(SafeFileHandle handle, Span<byte> buffer, long fileOffset)
	{
		if (fileOffset < 0)
		{
			throw ExceptionFactory.NonNegativeNumberRequired("fileOffset");
		}

		IStorageContainer container = GetContainer(handle, FileAccess.Read);
		lock (Gate(container))
		{
			byte[] bytes = container.GetBytes();
			if (fileOffset >= bytes.Length)
			{
				return 0;
			}

			int count = (int)Math.Min(buffer.Length, bytes.Length - fileOffset);
			bytes.AsSpan((int)fileOffset, count).CopyTo(buffer);
			return count;
		}
	}

	private long ReadInto(SafeFileHandle handle, IReadOnlyList<Memory<byte>> buffers,
		long fileOffset)
	{
		if (fileOffset < 0)
		{
			throw ExceptionFactory.NonNegativeNumberRequired("fileOffset");
		}

		IStorageContainer container = GetContainer(handle, FileAccess.Read);
		lock (Gate(container))
		{
			byte[] bytes = container.GetBytes();
			long copied = 0;
			foreach (Memory<byte> buffer in buffers)
			{
				long position = fileOffset + copied;
				if (position >= bytes.Length)
				{
					break;
				}

				int count = (int)Math.Min(buffer.Length, bytes.Length - position);
				bytes.AsSpan((int)position, count).CopyTo(buffer.Span);
				copied += count;
			}

			return copied;
		}
	}

	private void WriteBytes(SafeFileHandle handle, byte[] buffer, long fileOffset)
	{
		if (fileOffset < 0)
		{
			throw ExceptionFactory.NonNegativeNumberRequired("fileOffset");
		}

		(IStorageContainer container, FileMode mode) = Resolve(handle, FileAccess.Write);
		if (buffer.Length == 0)
		{
			return;
		}

		// `RandomAccess` permits concurrent writes at distinct offsets, which would lose each other if two of them
		// started from the same snapshot.
		lock (Gate(container))
		{
			byte[] bytes = container.GetBytes();
			// Linux `pwrite(2)` appends when the descriptor carries `O_APPEND`, whatever offset is passed, contrary
			// to POSIX; Windows and macOS honour the offset.
			if (mode == FileMode.Append && _fileSystem.Execute.IsLinux)
			{
				fileOffset = bytes.Length;
			}

			long required = fileOffset + buffer.Length;
			if (required > bytes.Length)
			{
				byte[] grown = new byte[required];
				Array.Copy(bytes, grown, bytes.Length);
				bytes = grown;
			}

			Array.Copy(buffer, 0, bytes, fileOffset, buffer.Length);
			container.WriteBytes(bytes);
		}
	}

	private object Gate(IStorageContainer container)
		=> _gates.GetOrCreateValue(container);

}
#endif
