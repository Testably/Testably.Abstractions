using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Helpers;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     A mocked file stream in the <see cref="InMemoryStorage" />.
/// </summary>
internal sealed class FileStreamMock : FileSystemStream, IFileSystemExtensibility
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
	private readonly long _initialPosition;
	private bool _isContentChanged;
	private bool _isDisposed;
	private readonly FileMode _mode;
	private readonly FileOptions _options;
	private readonly MemoryStream _stream;
	private readonly IStorageLocation _location;

	internal FileStreamMock(MockFileSystem fileSystem,
		string? path,
		FileMode mode,
		FileAccess access,
		FileShare share = FileShare.Read,
		int bufferSize = 4096,
		FileOptions options = FileOptions.None)
		: this(new MemoryStream(),
			fileSystem,
			path.EnsureValidFormat(fileSystem, nameof(path)),
			mode,
			access,
			share,
			bufferSize,
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
			path == null ? "" : fileSystem.Path.GetFullPath(path),
			(options & FileOptions.Asynchronous) != 0)
	{
		ThrowIfInvalidModeAccess(mode, access);

		_stream = stream;
		_fileSystem = fileSystem;
		_mode = mode;
		_access = access;
		_ = bufferSize;
		_options = options;
		_initialPosition = Position;

		_location = _fileSystem.Storage.GetLocation(Name);
		_location.ThrowExceptionIfNotFound(_fileSystem, true);
		IStorageContainer file = _fileSystem.Storage.GetContainer(_location);
		if (file is NullContainer)
		{
			if (_mode.Equals(FileMode.Open) ||
			    _mode.Equals(FileMode.Truncate))
			{
				throw ExceptionFactory.FileNotFound(
					_fileSystem.Path.GetFullPath(Name));
			}

			file = _fileSystem.Storage.GetOrCreateContainer(_location,
				InMemoryContainer.NewFile,
				this);
		}
		else if (file.Type == FileSystemTypes.Directory)
		{
			_fileSystem.Execute.OnWindows(
				() =>
					throw ExceptionFactory.AccessToPathDenied(
						_fileSystem.Path.GetFullPath(Name)),
				() =>
					throw ExceptionFactory.FileAlreadyExists(
						_fileSystem.Path.GetFullPath(Name), 17));
		}
		else if (_mode.Equals(FileMode.CreateNew))
		{
			throw ExceptionFactory.FileAlreadyExists(
				_fileSystem.Path.GetFullPath(Name),
				_fileSystem.Execute.IsWindows ? -2147024816 : 17);
		}

		if (file.Attributes.HasFlag(FileAttributes.ReadOnly) &&
		    access.HasFlag(FileAccess.Write))
		{
			throw ExceptionFactory.AccessToPathDenied(_location.FullPath);
		}

		_accessLock = file.RequestAccess(access, share);

		_container = file;

		InitializeStream();
	}

	/// <inheritdoc cref="FileSystemStream.BeginRead(byte[], int, int, AsyncCallback?, object?)" />
	public override IAsyncResult BeginRead(byte[] buffer,
		int offset,
		int count,
		AsyncCallback? callback,
		object? state)
	{
		using IDisposable registration = Register(nameof(BeginRead),
			buffer, offset, count, callback, state);

		ThrowIfDisposed();
		if (!CanRead)
		{
			throw ExceptionFactory.StreamDoesNotSupportReading();
		}

		return base.BeginRead(buffer, offset, count, callback, state);
	}

	/// <inheritdoc cref="FileSystemStream.BeginWrite(byte[], int, int, AsyncCallback?, object?)" />
	public override IAsyncResult BeginWrite(byte[] buffer,
		int offset,
		int count,
		AsyncCallback? callback,
		object? state)
	{
		using IDisposable registration = Register(nameof(BeginWrite),
			buffer, offset, count, callback, state);

		ThrowIfDisposed();
		if (!CanWrite)
		{
			throw ExceptionFactory.StreamDoesNotSupportWriting();
		}

		return base.BeginWrite(buffer, offset, count, callback, state);
	}

	/// <inheritdoc cref="FileSystemStream.CopyTo(Stream, int)" />
	public override void CopyTo(Stream destination, int bufferSize)
	{
		using IDisposable registration = Register(nameof(CopyTo),
			destination, bufferSize);

		_fileSystem.Execute.NotOnWindows(() =>
			_container.AdjustTimes(TimeAdjustments.LastAccessTime));
		base.CopyTo(destination, bufferSize);
	}

	/// <inheritdoc cref="FileSystemStream.CopyToAsync(Stream, int, CancellationToken)" />
	public override Task CopyToAsync(Stream destination, int bufferSize,
		CancellationToken cancellationToken)
	{
		using IDisposable registration = Register(nameof(CopyToAsync),
			destination, bufferSize, cancellationToken);

		_container.AdjustTimes(TimeAdjustments.LastAccessTime);
		return base.CopyToAsync(destination, bufferSize, cancellationToken);
	}

	/// <inheritdoc cref="FileSystemStream.EndRead(IAsyncResult)" />
	public override int EndRead(IAsyncResult asyncResult)
	{
		using IDisposable registration = Register(nameof(EndRead),
			asyncResult);

		_container.AdjustTimes(TimeAdjustments.LastAccessTime);
		return base.EndRead(asyncResult);
	}

	/// <inheritdoc cref="FileSystemStream.EndWrite(IAsyncResult)" />
	public override void EndWrite(IAsyncResult asyncResult)
	{
		using IDisposable registration = Register(nameof(EndWrite),
			asyncResult);

		_isContentChanged = true;
		base.EndWrite(asyncResult);
	}

	/// <inheritdoc cref="FileSystemStream.Flush()" />
	public override void Flush()
	{
		using IDisposable registration = Register(nameof(Flush));

		ThrowIfDisposed();
		InternalFlush();
	}

	/// <inheritdoc cref="FileSystemStream.Flush(bool)" />
	public override void Flush(bool flushToDisk)
	{
		using IDisposable registration = Register(nameof(Flush),
			flushToDisk);

		Flush();
	}

	/// <inheritdoc cref="FileSystemStream.FlushAsync(CancellationToken)" />
	public override Task FlushAsync(CancellationToken cancellationToken)
	{
		using IDisposable registration = Register(nameof(FlushAsync),
			cancellationToken);

		if (cancellationToken.IsCancellationRequested)
		{
			throw ExceptionFactory.TaskWasCanceled();
		}

		Flush();
		return Task.CompletedTask;
	}

	/// <inheritdoc cref="FileSystemStream.Read(byte[], int, int)" />
	public override int Read(byte[] buffer, int offset, int count)
	{
		using IDisposable registration = Register(nameof(Read),
			buffer, offset, count);

		if (!CanRead)
		{
			throw ExceptionFactory.StreamDoesNotSupportReading();
		}

		_fileSystem.Execute.NotOnWindows(() =>
			_container.AdjustTimes(TimeAdjustments.LastAccessTime));
		return base.Read(buffer, offset, count);
	}

#if FEATURE_SPAN
	/// <inheritdoc cref="FileSystemStream.Read(Span{byte})" />
	public override int Read(Span<byte> buffer)
	{
		using IDisposable registration = Register(nameof(Read),
			new SpanProvider<byte>(buffer));

		if (!CanRead)
		{
			throw ExceptionFactory.StreamDoesNotSupportReading();
		}

		_fileSystem.Execute.NotOnWindows(() =>
			_container.AdjustTimes(TimeAdjustments.LastAccessTime));
		return base.Read(buffer);
	}
#endif

	/// <inheritdoc cref="FileSystemStream.ReadAsync(byte[], int, int, CancellationToken)" />
	public override Task<int> ReadAsync(byte[] buffer, int offset, int count,
		CancellationToken cancellationToken)
	{
		using IDisposable registration = Register(nameof(ReadAsync),
			buffer, offset, count, cancellationToken);

		if (!CanRead)
		{
			throw ExceptionFactory.StreamDoesNotSupportReading();
		}

		_fileSystem.Execute.NotOnWindows(() =>
			_container.AdjustTimes(TimeAdjustments.LastAccessTime));
		return base.ReadAsync(buffer, offset, count, cancellationToken);
	}

#if FEATURE_SPAN
	/// <inheritdoc cref="FileSystemStream.ReadAsync(Memory{byte}, CancellationToken)" />
	public override ValueTask<int> ReadAsync(Memory<byte> buffer,
		CancellationToken cancellationToken = new())
	{
		using IDisposable registration = Register(nameof(ReadAsync),
			buffer, cancellationToken);

		if (!CanRead)
		{
			throw ExceptionFactory.StreamDoesNotSupportReading();
		}

		_fileSystem.Execute.NotOnWindows(() =>
			_container.AdjustTimes(TimeAdjustments.LastAccessTime));
		return base.ReadAsync(buffer, cancellationToken);
	}
#endif

	/// <inheritdoc cref="FileSystemStream.ReadByte()" />
	public override int ReadByte()
	{
		using IDisposable registration = Register(nameof(ReadByte));

		if (!CanRead)
		{
			throw ExceptionFactory.StreamDoesNotSupportReading();
		}

		_fileSystem.Execute.NotOnWindows(() =>
			_container.AdjustTimes(TimeAdjustments.LastAccessTime));
		return base.ReadByte();
	}

	/// <inheritdoc cref="FileSystemStream.Seek(long, SeekOrigin)" />
	public override long Seek(long offset, SeekOrigin origin)
	{
		using IDisposable registration = Register(nameof(Seek),
			offset, origin);

		if (_mode == FileMode.Append && offset <= _initialPosition)
		{
			throw ExceptionFactory.SeekBackwardNotPossibleInAppendMode();
		}

		return base.Seek(offset, origin);
	}

	/// <inheritdoc cref="FileSystemStream.SetLength(long)" />
	public override void SetLength(long value)
	{
		using IDisposable registration = Register(nameof(SetLength),
			value);

		ThrowIfDisposed();
		if (!CanWrite)
		{
			throw ExceptionFactory.StreamDoesNotSupportWriting();
		}

		base.SetLength(value);
	}

	/// <inheritdoc cref="FileSystemStream.Write(byte[], int, int)" />
	public override void Write(byte[] buffer, int offset, int count)
	{
		using IDisposable registration = Register(nameof(Write),
			buffer, offset, count);

		if (!CanWrite)
		{
			throw ExceptionFactory.StreamDoesNotSupportWriting();
		}

		_isContentChanged = true;
		base.Write(buffer, offset, count);
	}

#if FEATURE_SPAN
	/// <inheritdoc cref="FileSystemStream.Write(ReadOnlySpan{byte})" />
	public override void Write(ReadOnlySpan<byte> buffer)
	{
		using IDisposable registration = Register(nameof(Write),
			new SpanProvider<byte>(buffer));

		if (!CanWrite)
		{
			throw ExceptionFactory.StreamDoesNotSupportWriting();
		}

		_isContentChanged = true;
		base.Write(buffer);
	}
#endif

	/// <inheritdoc cref="FileSystemStream.WriteAsync(byte[], int, int, CancellationToken)" />
	public override Task WriteAsync(byte[] buffer, int offset, int count,
		CancellationToken cancellationToken)
	{
		using IDisposable registration = Register(nameof(WriteAsync),
			buffer, offset, count, cancellationToken);

		if (!CanWrite)
		{
			throw ExceptionFactory.StreamDoesNotSupportWriting();
		}

		_isContentChanged = true;
		return base.WriteAsync(buffer, offset, count, cancellationToken);
	}

#if FEATURE_SPAN
	/// <inheritdoc cref="FileSystemStream.WriteAsync(ReadOnlyMemory{byte}, CancellationToken)" />
	public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer,
		CancellationToken cancellationToken = new())
	{
		using IDisposable registration = Register(nameof(WriteAsync),
			buffer, cancellationToken);

		if (!CanWrite)
		{
			throw ExceptionFactory.StreamDoesNotSupportWriting();
		}

		_isContentChanged = true;
		return base.WriteAsync(buffer, cancellationToken);
	}
#endif

	/// <inheritdoc cref="FileSystemStream.WriteByte(byte)" />
	public override void WriteByte(byte value)
	{
		using IDisposable registration = Register(nameof(WriteByte),
			value);

		if (!CanWrite)
		{
			throw ExceptionFactory.StreamDoesNotSupportWriting();
		}

		_isContentChanged = true;
		base.WriteByte(value);
	}

	/// <inheritdoc cref="FileSystemStream.ToString()" />
	public override string? ToString()
	{
		using IDisposable registration = Register(nameof(ToString));

		return base.ToString();
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

	private void ThrowIfDisposed()
	{
		if (_isDisposed)
		{
			throw new ObjectDisposedException("", "Cannot access a closed file.");
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

	/// <inheritdoc cref="IFileSystemExtensibility.TryGetWrappedInstance{T}" />
	public bool TryGetWrappedInstance<T>([NotNullWhen(true)] out T? wrappedInstance)
		=> _container.Extensibility.TryGetWrappedInstance(out wrappedInstance);

	/// <inheritdoc cref="StoreMetadata{T}(string, T)" />
	public void StoreMetadata<T>(string key, T? value)
		=> _container.Extensibility.StoreMetadata(key, value);

	/// <inheritdoc cref="RetrieveMetadata{T}(string)" />
	public T? RetrieveMetadata<T>(string key)
		=> _container.Extensibility.RetrieveMetadata<T>(key);

	private IDisposable Register(string name, params object?[] parameters)
		=> _fileSystem.FileSystemStatistics.FileStreamStatistics.Register(_location.FullPath, name, parameters);
}
