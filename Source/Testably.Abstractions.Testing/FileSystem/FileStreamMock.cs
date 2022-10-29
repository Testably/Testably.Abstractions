using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.FileSystem;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     A mocked file stream in the <see cref="InMemoryStorage" />.
/// </summary>
internal sealed class FileStreamMock : FileSystemStream
{
	/// <inheritdoc cref="FileSystemStream.CanRead" />
	public override bool CanRead
		=> _access.HasFlag(FileAccess.Read);

	/// <inheritdoc cref="FileSystemStream.CanWrite" />
	public override bool CanWrite
		=> _access.HasFlag(FileAccess.Write);

	private readonly FileAccess _access;
	private readonly IDisposable _accessLock;
	private readonly IStorageContainer _container;
	private readonly MockFileSystem _fileSystem;
	private bool _isContentChanged;
	private bool _isDisposed;
	private readonly FileMode _mode;
	private readonly FileOptions _options;
	private readonly MemoryStream _stream;

	internal FileStreamMock(MockFileSystem fileSystem,
							string? path,
							FileMode mode,
							FileAccess access,
							FileShare share = FileShare.Read,
							int bufferSize = 4096,
							FileOptions options = FileOptions.None)
		: this(new MemoryStream(), fileSystem, path, mode, access, share, bufferSize,
			options)
	{
	}

	private FileStreamMock(MemoryStream stream,
						   MockFileSystem fileSystem,
						   string? path,
						   FileMode mode,
						   FileAccess access,
						   FileShare share,
						   int bufferSize,
						   FileOptions options)
		: base(
			stream,
			path == null ? null : fileSystem.Path.GetFullPath(path),
			(options & FileOptions.Asynchronous) != 0)
	{
		ThrowIfInvalidModeAccess(mode, access);

		_stream = stream;
		_fileSystem = fileSystem;
		_mode = mode;
		_access = access;
		_ = bufferSize;
		_options = options;

		IStorageLocation location = _fileSystem.Storage.GetLocation(Name);
		IStorageContainer file = _fileSystem.Storage.GetContainer(location);
		if (file is NullContainer)
		{
			if (_mode.Equals(FileMode.Open) ||
				_mode.Equals(FileMode.Truncate))
			{
				throw ExceptionFactory.FileNotFound(
					_fileSystem.Path.GetFullPath(Name));
			}

			file = _fileSystem.Storage.GetOrCreateContainer(location,
				InMemoryContainer.NewFile);
		}
		else if (file.Type == FileSystemTypes.Directory)
		{
			Execute.OnWindows(
				() =>
					throw ExceptionFactory.AccessToPathDenied(
						_fileSystem.Path.GetFullPath(Name)),
				() =>
					throw ExceptionFactory.FileAlreadyExists(
						_fileSystem.Path.GetFullPath(Name)));
		}
		else if (_mode.Equals(FileMode.CreateNew))
		{
			throw ExceptionFactory.FileAlreadyExists(
				_fileSystem.Path.GetFullPath(Name));
		}

		_accessLock = file.RequestAccess(access, share);

		_container = file;

		InitializeStream();
	}

	/// <inheritdoc cref="FileSystemStream.ExtensionContainer" />
	public override IFileSystemExtensionContainer ExtensionContainer
		=> _container.ExtensionContainer;

	/// <inheritdoc cref="FileSystemStream.CopyTo(Stream, int)" />
	public override void CopyTo(Stream destination, int bufferSize)
	{
		_container.AdjustTimes(TimeAdjustments.LastAccessTime);
		base.CopyTo(destination, bufferSize);
	}
	
	/// <inheritdoc cref="FileSystemStream.CopyToAsync(Stream, int, CancellationToken)" />
	public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
	{
		_container.AdjustTimes(TimeAdjustments.LastAccessTime);
		return base.CopyToAsync(destination, bufferSize, cancellationToken);
	}

	/// <inheritdoc cref="FileSystemStream.EndRead(IAsyncResult)" />
	public override int EndRead(IAsyncResult asyncResult)
	{
		_container.AdjustTimes(TimeAdjustments.LastAccessTime);
		return base.EndRead(asyncResult);
	}

	/// <inheritdoc cref="FileSystemStream.EndWrite(IAsyncResult)" />
	public override void EndWrite(IAsyncResult asyncResult)
	{
		_isContentChanged = true;
		base.EndWrite(asyncResult);
	}

	/// <inheritdoc cref="FileSystemStream.Flush()" />
	public override void Flush()
	{
		InternalFlush();
	}

	/// <inheritdoc cref="FileSystemStream.Read(byte[], int, int)" />
	public override int Read(byte[] buffer, int offset, int count)
	{
		_container.AdjustTimes(TimeAdjustments.LastAccessTime);
		return base.Read(buffer, offset, count);
	}

#if FEATURE_SPAN
	/// <inheritdoc cref="FileSystemStream.Read(Span{byte})" />
	public override int Read(Span<byte> buffer)
	{
		_container.AdjustTimes(TimeAdjustments.LastAccessTime);
		return base.Read(buffer);
	}
#endif

	/// <inheritdoc cref="FileSystemStream.ReadAsync(byte[], int, int, CancellationToken)" />
	public override Task<int> ReadAsync(byte[] buffer, int offset, int count,
										CancellationToken cancellationToken)
	{
		_container.AdjustTimes(TimeAdjustments.LastAccessTime);
		return base.ReadAsync(buffer, offset, count, cancellationToken);
	}

#if FEATURE_SPAN
	/// <inheritdoc cref="FileSystemStream.ReadAsync(Memory{byte}, CancellationToken)" />
	public override ValueTask<int> ReadAsync(Memory<byte> buffer,
											 CancellationToken cancellationToken =
												 new())
	{
		_container.AdjustTimes(TimeAdjustments.LastAccessTime);
		return base.ReadAsync(buffer, cancellationToken);
	}
#endif

	/// <inheritdoc cref="FileSystemStream.ReadByte()" />
	public override int ReadByte()
	{
		_container.AdjustTimes(TimeAdjustments.LastAccessTime);
		return base.ReadByte();
	}

	/// <inheritdoc />
	public override void SetLength(long value)
	{
		if (!_access.HasFlag(FileAccess.Write))
		{
			throw ExceptionFactory.StreamDoesNotSupportWriting();
		}

		base.SetLength(value);
	}

	/// <inheritdoc cref="FileSystemStream.Write(byte[], int, int)" />
	public override void Write(byte[] buffer, int offset, int count)
	{
		_isContentChanged = true;
		base.Write(buffer, offset, count);
	}

#if FEATURE_SPAN
	/// <inheritdoc cref="FileSystemStream.Write(ReadOnlySpan{byte})" />
	public override void Write(ReadOnlySpan<byte> buffer)
	{
		_isContentChanged = true;
		base.Write(buffer);
	}
#endif

	/// <inheritdoc cref="FileSystemStream.WriteAsync(byte[], int, int, CancellationToken)" />
	public override Task WriteAsync(byte[] buffer, int offset, int count,
									CancellationToken cancellationToken)
	{
		_isContentChanged = true;
		return base.WriteAsync(buffer, offset, count, cancellationToken);
	}

#if FEATURE_SPAN
	/// <inheritdoc cref="FileSystemStream.WriteAsync(ReadOnlyMemory{byte}, CancellationToken)" />
	public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer,
										 CancellationToken cancellationToken = new())
	{
		_isContentChanged = true;
		return base.WriteAsync(buffer, cancellationToken);
	}
#endif

	/// <inheritdoc cref="FileSystemStream.WriteByte(byte)" />
	public override void WriteByte(byte value)
	{
		_isContentChanged = true;
		base.WriteByte(value);
	}

	/// <inheritdoc cref="FileSystemStream.Dispose(bool)" />
	protected override void Dispose(bool disposing)
	{
		if (_isDisposed)
		{
			return;
		}

		_accessLock.Dispose();
		InternalFlush();
		base.Dispose(disposing);
		OnClose();
		_isDisposed = true;
	}

	private void InitializeStream()
	{
		if (_mode != FileMode.Create &&
			_mode != FileMode.Truncate)
		{
			byte[] existingContents = _container.GetBytes();
			_stream.Write(existingContents, 0, existingContents.Length);
			_stream.Seek(0, _mode == FileMode.Append
				? SeekOrigin.End
				: SeekOrigin.Begin);
		}
		else
		{
			_isContentChanged = true;
		}
	}

	private void InternalFlush()
	{
		if (!_isContentChanged)
		{
			return;
		}

		_isContentChanged = false;
		long position = _stream.Position;
		_stream.Seek(0, SeekOrigin.Begin);
		byte[] data = new byte[Length];
		_ = _stream.Read(data, 0, (int)Length);
		_stream.Seek(position, SeekOrigin.Begin);
		_container.WriteBytes(data);
	}

	private void OnClose()
	{
		if (_options.HasFlag(FileOptions.DeleteOnClose))
		{
			_fileSystem.Storage.DeleteContainer(
				_fileSystem.Storage.GetLocation(Name));
		}
	}

	private static void ThrowIfInvalidModeAccess(FileMode mode, FileAccess access)
	{
		if (mode == FileMode.Append)
		{
			if (access == FileAccess.Read)
			{
				throw ExceptionFactory.InvalidAccessCombination(mode, access);
			}

			if (access != FileAccess.Write)
			{
				throw ExceptionFactory.AppendAccessOnlyInWriteOnlyMode();
			}
		}

		if (!access.HasFlag(FileAccess.Write) &&
			(mode == FileMode.Truncate || mode == FileMode.CreateNew ||
			 mode == FileMode.Create || mode == FileMode.Append))
		{
			throw ExceptionFactory.InvalidAccessCombination(mode, access);
		}
	}
}