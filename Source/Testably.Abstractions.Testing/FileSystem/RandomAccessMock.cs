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
		// test can assert that a durability barrier was requested. The runtime accepts read-only handles on every OS.
		_ = GetContainer(handle, required: null);
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
			.RandomAccess.RegisterMethod<SafeFileHandle, byte, long>(nameof(Read), handle, buffer, fileOffset);

		return ReadInto(handle, buffer, fileOffset);
	}

	/// <inheritdoc cref="IRandomAccess.Read(SafeFileHandle, IReadOnlyList{Memory{byte}}, long)" />
	public long Read(SafeFileHandle handle, IReadOnlyList<Memory<byte>> buffers, long fileOffset)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.RandomAccess.RegisterMethod(nameof(Read), handle, buffers, fileOffset);

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
			.RandomAccess.RegisterMethod(nameof(ReadAsync), handle, buffer, fileOffset,
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
			.RandomAccess.RegisterMethod(nameof(ReadAsync), handle, buffers, fileOffset,
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
			throw ExceptionFactory.NonNegativeNumberRequired(nameof(length));
		}

		IStorageContainer container = GetContainerForResize(handle);
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
			.RandomAccess.RegisterMethod<SafeFileHandle, byte, long>(nameof(Write), handle, buffer, fileOffset);

		WriteBytes(handle, buffer.ToArray(), fileOffset);
	}

	/// <inheritdoc cref="IRandomAccess.Write(SafeFileHandle, IReadOnlyList{ReadOnlyMemory{byte}}, long)" />
	public void Write(SafeFileHandle handle, IReadOnlyList<ReadOnlyMemory<byte>> buffers,
		long fileOffset)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.RandomAccess.RegisterMethod(nameof(Write), handle, buffers, fileOffset);

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
			.RandomAccess.RegisterMethod(nameof(WriteAsync), handle, buffer, fileOffset,
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
			.RandomAccess.RegisterMethod(nameof(WriteAsync), handle, buffers, fileOffset,
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

#if FEATURE_RANDOMACCESS_FLUSHTODISK
	private IStorageContainer GetContainerForResize(SafeFileHandle handle)
	{
		(IStorageContainer container, FileAccess access) = ResolveEntry(handle);
		if (access.HasFlag(FileAccess.Write))
		{
			return container;
		}

		// `ftruncate` on a read-only descriptor fails with `EINVAL`, where Windows reports access denied.
		throw _fileSystem.Execute.IsWindows
			? ExceptionFactory.AccessToPathDenied()
			: ExceptionFactory.InvalidArgument(
				_fileSystem.SafeFileHandleRegistry.Map(handle).Path);
	}
#endif

	private IStorageContainer GetContainer(SafeFileHandle handle, FileAccess? required)
	{
		(IStorageContainer container, FileAccess access) = ResolveEntry(handle);
		if (required is { } requiredAccess && !access.HasFlag(requiredAccess))
		{
			throw ExceptionFactory.AccessToPathDenied();
		}

		return container;
	}

	private (IStorageContainer Container, FileAccess Access) ResolveEntry(SafeFileHandle handle)
	{
		if (handle is null)
		{
			throw new ArgumentNullException(nameof(handle));
		}

		if (handle.IsInvalid)
		{
			throw ExceptionFactory.HandleIsInvalid();
		}

		return _fileSystem.SafeFileHandleRegistry.GetContainer(handle);
	}

	private int ReadInto(SafeFileHandle handle, Span<byte> buffer, long fileOffset)
	{
		if (fileOffset < 0)
		{
			throw ExceptionFactory.NonNegativeNumberRequired(nameof(fileOffset));
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
			throw ExceptionFactory.NonNegativeNumberRequired(nameof(fileOffset));
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
			throw ExceptionFactory.NonNegativeNumberRequired(nameof(fileOffset));
		}

		IStorageContainer container = GetContainer(handle, FileAccess.Write);
		if (buffer.Length == 0)
		{
			return;
		}

		// The content is a single array, so an offset beyond its maximum length cannot be written, just like one
		// beyond the maximum file size of a real file system.
		if (fileOffset > Array.MaxLength - buffer.Length)
		{
			throw ExceptionFactory.FileTooLarge(
				_fileSystem.SafeFileHandleRegistry.Map(handle).Path);
		}

		// `RandomAccess` permits concurrent writes at distinct offsets, which would lose each other if two of them
		// started from the same snapshot.
		lock (Gate(container))
		{
			container.WriteRange(buffer, fileOffset);
		}
	}

	private object Gate(IStorageContainer container)
		=> _gates.GetOrCreateValue(container);
}
#endif
