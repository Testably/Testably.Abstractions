using System;
using System.IO;
using System.Threading;
using Testably.Abstractions.Helpers;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.Storage;

internal sealed class NullContainer : IStorageContainer
{
	private readonly MockFileSystem _fileSystem;
	private FileSystemExtensibility? _extensibility;

	private NullContainer(MockFileSystem fileSystem, ITimeSystem timeSystem, bool isNullDevice)
	{
		_fileSystem = fileSystem;
		TimeSystem = timeSystem;
		if (isNullDevice)
		{
			CreationTime = new InMemoryContainer.TimeContainer();
			LastAccessTime = new InMemoryContainer.TimeContainer();
			LastWriteTime = new InMemoryContainer.TimeContainer();
		}
		else
		{
#if NET8_0_OR_GREATER
			CreationTime = NullTime.For(throwDirectoryNotFound: _fileSystem.Execute.IsMac);
			LastAccessTime = NullTime.For(throwDirectoryNotFound: false);
#else
			CreationTime = NullTime.For(throwDirectoryNotFound: !_fileSystem.Execute.IsWindows);
			LastAccessTime = CreationTime;
#endif
			LastWriteTime = LastAccessTime;
		}
	}

	#region IStorageContainer Members

	/// <inheritdoc cref="IStorageContainer.Attributes" />
	public FileAttributes Attributes
	{
		get => (FileAttributes)(-1);
		set => throw ExceptionFactory.FileNotFound(string.Empty);
	}

	/// <inheritdoc cref="IStorageContainer.CreationTime" />
	public IStorageContainer.ITimeContainer CreationTime { get; }

	/// <inheritdoc cref="IStorageContainer.Extensibility" />
	public IFileSystemExtensibility Extensibility
	{
		get
		{
			if (_extensibility != null)
			{
				return _extensibility;
			}

			FileSystemExtensibility extensibility = new();
			return Interlocked.CompareExchange(ref _extensibility, extensibility, null)
			       ?? extensibility;
		}
	}

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem => _fileSystem;

	/// <inheritdoc cref="IStorageContainer.LastAccessTime" />
	public IStorageContainer.ITimeContainer LastAccessTime { get; }

	/// <inheritdoc cref="IStorageContainer.LastWriteTime" />
	public IStorageContainer.ITimeContainer LastWriteTime { get; }

	/// <inheritdoc cref="IStorageContainer.LinkTarget" />
	public string? LinkTarget
	{
		get => null;
		set => _ = value;
	}

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem { get; }

	/// <inheritdoc cref="IStorageContainer.Type" />
	public FileSystemTypes Type
		=> FileSystemTypes.DirectoryOrFile;

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	/// <inheritdoc cref="IStorageContainer.UnixFileMode" />
	public UnixFileMode UnixFileMode { get; set; } = (UnixFileMode)(-1);
#endif

	/// <inheritdoc cref="IStorageContainer.AppendBytes(byte[])" />
	public void AppendBytes(byte[] bytes)
	{
		// Do nothing in NullContainer
	}

	#pragma warning disable CS0067 // Event is never used
	/// <inheritdoc cref="IStorageContainer.BytesChanged" />
	public event EventHandler? BytesChanged;
	#pragma warning restore CS0067

	/// <inheritdoc cref="IStorageContainer.Unlink()" />
	public void Unlink()
	{
		// Do nothing in NullContainer
	}

	/// <inheritdoc cref="IStorageContainer.Decrypt()" />
	public void Decrypt()
	{
		// Do nothing in NullContainer
	}

	/// <inheritdoc cref="IStorageContainer.Encrypt()" />
	public void Encrypt()
	{
		// Do nothing in NullContainer
	}

	/// <inheritdoc cref="IStorageContainer.GetBytes()" />
	public byte[] GetBytes()
		=> Array.Empty<byte>();

	/// <inheritdoc cref="IStorageContainer.RequestAccess(FileAccess, FileShare, bool, bool, bool, int?, IStorageLocation?)" />
	public IStorageAccessHandle RequestAccess(FileAccess access, FileShare share,
		bool deleteAccess = false,
		bool ignoreFileShare = false,
		bool ignoreMetadataErrors = true,
		int? hResult = null,
		IStorageLocation? onBehalfOfLocation = null)
		=> new NullStorageAccessHandle(access, share, deleteAccess);

	/// <inheritdoc cref="IStorageContainer.UpdateLocation(IStorageLocation)" />
	public IStorageContainer UpdateLocation(IStorageLocation newLocation)
	{
		// Do nothing in NullContainer
		return this;
	}

	/// <inheritdoc cref="IStorageContainer.WriteBytes(byte[])" />
	public void WriteBytes(byte[] bytes)
	{
		// Do nothing in NullContainer
	}

	/// <inheritdoc cref="IStorageContainer.WriteRange(byte[], long)" />
	public void WriteRange(byte[] bytes, long offset)
	{
		// Do nothing in NullContainer
	}

	#endregion

	internal static IStorageContainer New(MockFileSystem fileSystem)
		=> new NullContainer(fileSystem, fileSystem.TimeSystem, isNullDevice: false);

	/// <summary>
	///     Creates a container for the null device, which discards everything written to it and returns no data, but
	///     unlike a missing file accepts the timestamp updates of reads and writes.
	/// </summary>
	internal static IStorageContainer NewNullDevice(MockFileSystem fileSystem)
		=> new NullContainer(fileSystem, fileSystem.TimeSystem, isNullDevice: true);

	private sealed class NullStorageAccessHandle : IStorageAccessHandle
	{
		public NullStorageAccessHandle(FileAccess access, FileShare share,
			bool deleteAccess)
		{
			Access = access;
			Share = share;
			DeleteAccess = deleteAccess;
		}

		#region IStorageAccessHandle Members

		/// <inheritdoc cref="IStorageAccessHandle.Access" />
		public FileAccess Access { get; }

		/// <inheritdoc cref="IStorageAccessHandle.DeleteAccess" />
		public bool DeleteAccess { get; }

		/// <inheritdoc cref="IStorageAccessHandle.Share" />
		public FileShare Share { get; }

		/// <inheritdoc cref="IDisposable.Dispose()" />
		public void Dispose()
		{
			// Nothing to do!
		}

		#endregion
	}

	/// <summary>
	///     The default time returned by the file system if no time has been set.
	///     <seealso href="https://learn.microsoft.com/en-us/windows/win32/sysinfo/file-times" />:
	///     A file time is a 64-bit value that represents the number of 100-nanosecond intervals that have elapsed
	///     since 12:00 A.M. January 1, 1601, Coordinated Universal Time (UTC).
	/// </summary>
	/// <remarks>
	///     Stateless, so that all missing files share the same two instances.
	/// </remarks>
	private sealed class NullTime : IStorageContainer.ITimeContainer
	{
		private static readonly NullTime ThrowingDirectoryNotFound = new(true);
		private static readonly NullTime ThrowingFileNotFound = new(false);

		private static readonly DateTime Time =
			new(1601, 01, 01, 00, 00, 00, DateTimeKind.Utc);

		private readonly bool _throwDirectoryNotFound;

		private NullTime(bool throwDirectoryNotFound)
		{
			_throwDirectoryNotFound = throwDirectoryNotFound;
		}

		#region ITimeContainer Members

		/// <inheritdoc cref="IStorageContainer.ITimeContainer.Get(DateTimeKind)" />
		public DateTime Get(DateTimeKind kind)
			=> kind switch
			{
				DateTimeKind.Utc => Time.ToUniversalTime(),
				DateTimeKind.Local => Time.ToLocalTime(),
				_ => Time,
			};

		/// <inheritdoc cref="IStorageContainer.ITimeContainer.Set(DateTime, DateTimeKind)" />
		public void Set(DateTime time, DateTimeKind kind)
		{
			if (_throwDirectoryNotFound)
			{
				throw ExceptionFactory.DirectoryNotFound(string.Empty);
			}

			throw ExceptionFactory.FileNotFound(string.Empty);
		}

		#endregion

		public static NullTime For(bool throwDirectoryNotFound)
			=> throwDirectoryNotFound ? ThrowingDirectoryNotFound : ThrowingFileNotFound;
	}
}
