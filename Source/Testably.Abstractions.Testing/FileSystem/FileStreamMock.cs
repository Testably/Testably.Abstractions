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
	{
		get
		{
			using IDisposable registration = RegisterProperty(nameof(CanRead), PropertyAccess.Get);

			return _access.HasFlag(FileAccess.Read);
		}
	}

	/// <inheritdoc cref="FileSystemStream.CanSeek" />
	public override bool CanSeek
	{
		get
		{
			using IDisposable registration = RegisterProperty(nameof(CanSeek), PropertyAccess.Get);

			return base.CanSeek;
		}
	}

	/// <inheritdoc cref="FileSystemStream.CanTimeout" />
	public override bool CanTimeout
	{
		get
		{
			using IDisposable registration =
				RegisterProperty(nameof(CanTimeout), PropertyAccess.Get);

			return base.CanTimeout;
		}
	}

	/// <inheritdoc cref="FileSystemStream.CanWrite" />
	public override bool CanWrite
	{
		get
		{
			using IDisposable registration = RegisterProperty(nameof(CanWrite), PropertyAccess.Get);

			return _access.HasFlag(FileAccess.Write);
		}
	}

	/// <inheritdoc cref="FileSystemStream.IsAsync" />
	public override bool IsAsync
	{
		get
		{
			using IDisposable registration = RegisterProperty(nameof(IsAsync), PropertyAccess.Get);

			return base.IsAsync;
		}
	}

	/// <inheritdoc cref="FileSystemStream.Length" />
	public override long Length
	{
		get
		{
			using IDisposable registration = RegisterProperty(nameof(Length), PropertyAccess.Get);

			return base.Length;
		}
	}

	/// <inheritdoc cref="FileSystemStream.Name" />
	public override string Name
	{
		get
		{
			using IDisposable registration = RegisterProperty(nameof(Name), PropertyAccess.Get);

			return base.Name;
		}
	}

	/// <inheritdoc cref="FileSystemStream.Position" />
	public override long Position
	{
		get
		{
			using IDisposable registration = RegisterProperty(nameof(Position), PropertyAccess.Get);

			return base.Position;
		}
		set
		{
			using IDisposable registration = RegisterProperty(nameof(Position), PropertyAccess.Set);

			base.Position = value;
		}
	}

	/// <inheritdoc cref="FileSystemStream.ReadTimeout" />
	public override int ReadTimeout
	{
		get
		{
			using IDisposable registration =
				RegisterProperty(nameof(ReadTimeout), PropertyAccess.Get);

			return base.ReadTimeout;
		}
		set
		{
			using IDisposable registration =
				RegisterProperty(nameof(ReadTimeout), PropertyAccess.Set);

			base.ReadTimeout = value;
		}
	}

	/// <inheritdoc cref="FileSystemStream.WriteTimeout" />
	public override int WriteTimeout
	{
		get
		{
			using IDisposable registration =
				RegisterProperty(nameof(WriteTimeout), PropertyAccess.Get);

			return base.WriteTimeout;
		}
		set
		{
			using IDisposable registration =
				RegisterProperty(nameof(WriteTimeout), PropertyAccess.Set);

			base.WriteTimeout = value;
		}
	}

	private readonly FileAccess _access;
	private readonly IDisposable _accessLock;
	private readonly IStorageContainer _container;
	private readonly MockFileSystem _fileSystem;
	private readonly long _initialPosition;
	private bool _isContentChanged;
	private bool _isDisposed;
	private readonly IStorageLocation _location;
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
			path == null ? "" : fileSystem.Execute.Path.GetFullPath(path),
			(options & FileOptions.Asynchronous) != 0)
	{
		ThrowIfInvalidModeAccess(mode, access);

		_stream = stream;
		_fileSystem = fileSystem;
		_mode = mode;
		_access = access;
		_ = bufferSize;
		_options = options;
		_initialPosition = base.Position;

		_location = _fileSystem.Storage.GetLocation(base.Name);
		_location.ThrowExceptionIfNotFound(_fileSystem, true);
		IStorageContainer file = _fileSystem.Storage.GetContainer(_location);
		if (file is NullContainer)
		{
			if (_mode.Equals(FileMode.Open) ||
			    _mode.Equals(FileMode.Truncate))
			{
				throw ExceptionFactory.FileNotFound(
					_fileSystem.Execute.Path.GetFullPath(base.Name));
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
						_fileSystem.Execute.Path.GetFullPath(base.Name)),
				() =>
					throw ExceptionFactory.FileAlreadyExists(
						_fileSystem.Execute.Path.GetFullPath(base.Name), 17));
		}
		else if (_mode.Equals(FileMode.CreateNew))
		{
			throw ExceptionFactory.FileAlreadyExists(
				_fileSystem.Execute.Path.GetFullPath(Name),
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

	#region IFileSystemExtensibility Members

	/// <inheritdoc cref="RetrieveMetadata{T}(string)" />
	public T? RetrieveMetadata<T>(string key)
		=> _container.Extensibility.RetrieveMetadata<T>(key);

	/// <inheritdoc cref="StoreMetadata{T}(string, T)" />
	public void StoreMetadata<T>(string key, T? value)
		=> _container.Extensibility.StoreMetadata(key, value);

	/// <inheritdoc cref="IFileSystemExtensibility.TryGetWrappedInstance{T}" />
	public bool TryGetWrappedInstance<T>([NotNullWhen(true)] out T? wrappedInstance)
		=> _container.Extensibility.TryGetWrappedInstance(out wrappedInstance);

	#endregion

	/// <inheritdoc cref="FileSystemStream.BeginRead(byte[], int, int, AsyncCallback?, object?)" />
	public override IAsyncResult BeginRead(byte[] buffer,
		int offset,
		int count,
		AsyncCallback? callback,
		object? state)
	{
		using IDisposable registration = RegisterMethod(nameof(BeginRead),
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
		using IDisposable registration = RegisterMethod(nameof(BeginWrite),
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
		using IDisposable registration = RegisterMethod(nameof(CopyTo),
			destination, bufferSize);

		_fileSystem.Execute.NotOnWindows(() =>
			_container.AdjustTimes(TimeAdjustments.LastAccessTime));
		base.CopyTo(destination, bufferSize);
	}

	/// <inheritdoc cref="FileSystemStream.CopyToAsync(Stream, int, CancellationToken)" />
	public override Task CopyToAsync(Stream destination, int bufferSize,
		CancellationToken cancellationToken)
	{
		using IDisposable registration = RegisterMethod(nameof(CopyToAsync),
			destination, bufferSize, cancellationToken);

		_container.AdjustTimes(TimeAdjustments.LastAccessTime);
		return base.CopyToAsync(destination, bufferSize, cancellationToken);
	}

	/// <inheritdoc cref="FileSystemStream.EndRead(IAsyncResult)" />
	public override int EndRead(IAsyncResult asyncResult)
	{
		using IDisposable registration = RegisterMethod(nameof(EndRead),
			asyncResult);

		_container.AdjustTimes(TimeAdjustments.LastAccessTime);
		return base.EndRead(asyncResult);
	}

	/// <inheritdoc cref="FileSystemStream.EndWrite(IAsyncResult)" />
	public override void EndWrite(IAsyncResult asyncResult)
	{
		using IDisposable registration = RegisterMethod(nameof(EndWrite),
			asyncResult);

		_isContentChanged = true;
		base.EndWrite(asyncResult);
	}

	/// <inheritdoc cref="FileSystemStream.Flush()" />
	public override void Flush()
	{
		using IDisposable registration = RegisterMethod(nameof(Flush));

		ThrowIfDisposed();
		InternalFlush();
	}

	/// <inheritdoc cref="FileSystemStream.Flush(bool)" />
	public override void Flush(bool flushToDisk)
	{
		using IDisposable registration = RegisterMethod(nameof(Flush),
			flushToDisk);

		Flush();
	}

	/// <inheritdoc cref="FileSystemStream.FlushAsync(CancellationToken)" />
	public override Task FlushAsync(CancellationToken cancellationToken)
	{
		using IDisposable registration = RegisterMethod(nameof(FlushAsync),
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
		using IDisposable registration = RegisterMethod(nameof(Read),
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
		using IDisposable registration = RegisterMethod(nameof(Read),
			buffer);

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
		using IDisposable registration = RegisterMethod(nameof(ReadAsync),
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
		using IDisposable registration = RegisterMethod(nameof(ReadAsync),
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
		using IDisposable registration = RegisterMethod(nameof(ReadByte));

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
		using IDisposable registration = RegisterMethod(nameof(Seek),
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
		using IDisposable registration = RegisterMethod(nameof(SetLength),
			value);

		ThrowIfDisposed();
		if (!CanWrite)
		{
			throw ExceptionFactory.StreamDoesNotSupportWriting();
		}

		base.SetLength(value);
	}

	/// <inheritdoc cref="FileSystemStream.ToString()" />
	public override string? ToString()
	{
		using IDisposable registration = RegisterMethod(nameof(ToString));

		return base.ToString();
	}

	/// <inheritdoc cref="FileSystemStream.Write(byte[], int, int)" />
	public override void Write(byte[] buffer, int offset, int count)
	{
		using IDisposable registration = RegisterMethod(nameof(Write),
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
		using IDisposable registration = RegisterMethod(nameof(Write),
			buffer);

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
		using IDisposable registration = RegisterMethod(nameof(WriteAsync),
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
		using IDisposable registration = RegisterMethod(nameof(WriteAsync),
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
		using IDisposable registration = RegisterMethod(nameof(WriteByte),
			value);

		if (!CanWrite)
		{
			throw ExceptionFactory.StreamDoesNotSupportWriting();
		}

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

	private IDisposable RegisterMethod(string name)
		=> _fileSystem.StatisticsRegistration.FileStream.RegisterMethod(_location.FullPath, name);

	private IDisposable RegisterMethod<T1>(string name, T1 parameter1)
		=> _fileSystem.StatisticsRegistration.FileStream.RegisterMethod(_location.FullPath, name,
			ParameterDescription.FromParameter(parameter1));

#if FEATURE_SPAN
	private IDisposable RegisterMethod<T1>(string name, Span<T1> parameter1)
		=> _fileSystem.StatisticsRegistration.FileStream.RegisterMethod(_location.FullPath, name,
			ParameterDescription.FromParameter(parameter1));
#endif

#if FEATURE_SPAN
	private IDisposable RegisterMethod<T1>(string name, ReadOnlySpan<T1> parameter1)
		=> _fileSystem.StatisticsRegistration.FileStream.RegisterMethod(_location.FullPath, name,
			ParameterDescription.FromParameter(parameter1));
#endif

	private IDisposable RegisterMethod<T1, T2>(string name, T1 parameter1, T2 parameter2)
		=> _fileSystem.StatisticsRegistration.FileStream.RegisterMethod(_location.FullPath, name,
			ParameterDescription.FromParameter(parameter1),
			ParameterDescription.FromParameter(parameter2));

	private IDisposable RegisterMethod<T1, T2, T3>(string name, T1 parameter1, T2 parameter2,
		T3 parameter3)
		=> _fileSystem.StatisticsRegistration.FileStream.RegisterMethod(_location.FullPath, name,
			ParameterDescription.FromParameter(parameter1),
			ParameterDescription.FromParameter(parameter2),
			ParameterDescription.FromParameter(parameter3));

	private IDisposable RegisterMethod<T1, T2, T3, T4>(string name, T1 parameter1, T2 parameter2,
		T3 parameter3, T4 parameter4)
		=> _fileSystem.StatisticsRegistration.FileStream.RegisterMethod(_location.FullPath, name,
			ParameterDescription.FromParameter(parameter1),
			ParameterDescription.FromParameter(parameter2),
			ParameterDescription.FromParameter(parameter3),
			ParameterDescription.FromParameter(parameter4));

	private IDisposable RegisterMethod<T1, T2, T3, T4, T5>(string name, T1 parameter1,
		T2 parameter2, T3 parameter3, T4 parameter4, T5 parameter5)
		=> _fileSystem.StatisticsRegistration.FileStream.RegisterMethod(_location.FullPath, name,
			ParameterDescription.FromParameter(parameter1),
			ParameterDescription.FromParameter(parameter2),
			ParameterDescription.FromParameter(parameter3),
			ParameterDescription.FromParameter(parameter4),
			ParameterDescription.FromParameter(parameter5));

	private IDisposable RegisterProperty(string name, PropertyAccess access)
		=> _fileSystem.StatisticsRegistration.FileStream.RegisterProperty(_location.FullPath, name,
			access);

	private void ThrowIfDisposed()
	{
		if (_isDisposed)
		{
			throw new ObjectDisposedException("", "Cannot access a closed file.");
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
