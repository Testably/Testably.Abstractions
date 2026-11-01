using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Testably.Abstractions.Helpers;
using Testably.Abstractions.Testing.FileSystem;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.TimeSystem;
using static Testably.Abstractions.Testing.Storage.IStorageContainer;

namespace Testably.Abstractions.Testing.Storage;

internal sealed class InMemoryContainer : IStorageContainer
{
	private readonly FileData _data;
	private readonly MockFileSystem _fileSystem;
	private bool _isUnlinked;
	private IStorageLocation _location;

	public InMemoryContainer(FileSystemTypes type,
		IStorageLocation location,
		MockFileSystem fileSystem)
		: this(new FileData(type), location, fileSystem)
	{
		this.AdjustTimes(TimeAdjustments.All);
	}

	private InMemoryContainer(FileData data,
		IStorageLocation location,
		MockFileSystem fileSystem)
	{
		_data = data;
		_location = location;
		_fileSystem = fileSystem;
	}

	/// <summary>
	///     Identifies the file independent of its path, so that all hard links to it share the same
	///     <see cref="FileShare" /> locks.
	/// </summary>
	internal object AccessKey => _data;

	/// <summary>
	///     Indicates if the file is also stored at another path as a hard link.
	/// </summary>
	internal bool HasOtherLinks => Volatile.Read(ref _data.LinkCount) > 1;

	#region IStorageContainer Members

	/// <inheritdoc cref="IStorageContainer.Attributes" />
	public FileAttributes Attributes
	{
		get => AdjustAttributes(_data.Attributes);
		set
		{
			if (_fileSystem.Execute.IsWindows)
			{
				value &= FileAttributes.Directory |
				         FileAttributes.ReadOnly |
				         FileAttributes.Archive |
				         FileAttributes.Hidden |
				         FileAttributes.NoScrubData |
				         FileAttributes.NotContentIndexed |
				         FileAttributes.Offline |
				         FileAttributes.System |
				         FileAttributes.Temporary;
			}
			else if (_fileSystem.Execute.IsLinux)
			{
				value &= FileAttributes.Directory |
				         FileAttributes.ReadOnly;
			}
			else
			{
				value &= FileAttributes.Hidden |
				         FileAttributes.Directory |
				         FileAttributes.ReadOnly;
			}

			_data.Attributes = value;
		}
	}

	/// <inheritdoc cref="IStorageContainer.CreationTime" />
	public ITimeContainer CreationTime => _data.CreationTime;

	/// <inheritdoc cref="IStorageContainer.Extensibility" />
	public IFileSystemExtensibility Extensibility
		=> _data.Extensibility;

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem => _fileSystem;

	/// <inheritdoc cref="IStorageContainer.LastAccessTime" />
	public ITimeContainer LastAccessTime => _data.LastAccessTime;

	/// <inheritdoc cref="IStorageContainer.LastWriteTime" />
	public ITimeContainer LastWriteTime => _data.LastWriteTime;

	/// <inheritdoc cref="IStorageContainer.LinkTarget" />
	public string? LinkTarget
	{
		get => _data.LinkTarget;
		set => _data.LinkTarget = value;
	}

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem => _fileSystem.TimeSystem;

	/// <inheritdoc cref="IStorageContainer.Type" />
	public FileSystemTypes Type => _data.Type;

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	/// <inheritdoc cref="IStorageContainer.UnixFileMode" />
	public UnixFileMode UnixFileMode
	{
		get => _data.UnixFileMode;
		set
		{
			_data.UnixFileMode = value;
			_fileSystem.UnixFileModeStrategy.OnSetUnixFileMode(
				_location.FullPath,
				_data.Extensibility,
				value);
		}
	}
#endif

	/// <inheritdoc cref="IStorageContainer.AppendBytes(byte[])" />
	public void AppendBytes(byte[] bytes)
	{
		WriteBytes(_data.Bytes.Concat(bytes).ToArray());
	}

	/// <inheritdoc cref="IStorageContainer.BytesChanged" />
	public event EventHandler? BytesChanged
	{
		add => _data.BytesChanged += value;
		remove => _data.BytesChanged -= value;
	}

	/// <inheritdoc cref="IStorageContainer.Unlink()" />
	public void Unlink()
	{
		_isUnlinked = true;
		if (Interlocked.Decrement(ref _data.LinkCount) == 0)
		{
			_location.Drive?.ChangeUsedBytes(0 - _data.Bytes.Length);
		}
	}

	/// <inheritdoc cref="IStorageContainer.Decrypt()" />
	public void Decrypt()
	{
		if (!_data.IsEncrypted)
		{
			return;
		}

		using (RequestAccess(FileAccess.Write, FileShare.Read))
		{
			_data.IsEncrypted = false;
			WriteBytes(EncryptionHelper.Decrypt(GetBytes()));
		}
	}

	/// <inheritdoc cref="IStorageContainer.Encrypt()" />
	public void Encrypt()
	{
		if (_data.IsEncrypted)
		{
			return;
		}

		using (RequestAccess(FileAccess.Write, FileShare.Read))
		{
			_data.IsEncrypted = true;
			WriteBytes(EncryptionHelper.Encrypt(GetBytes()));
		}
	}

	/// <inheritdoc cref="IStorageContainer.GetBytes()" />
	public byte[] GetBytes() => _data.Bytes;

	/// <inheritdoc cref="IStorageContainer.RequestAccess(FileAccess, FileShare, bool, bool, bool, int?, IStorageLocation?)" />
	public IStorageAccessHandle RequestAccess(FileAccess access, FileShare share,
		bool deleteAccess = false,
		bool ignoreFileShare = false,
		bool ignoreMetadataErrors = true,
		int? hResult = null,
		IStorageLocation? onBehalfOfLocation = null)
	{
		if (FileSystemRegistration.IsInitializing())
		{
			return FileHandle.Ignore;
		}

		if (_location.Drive == null)
		{
			throw ExceptionFactory.DirectoryNotFound(_location.FullPath);
		}

		if (!_location.Drive.IsReady)
		{
			throw ExceptionFactory.NetworkPathNotFound(_location.FullPath);
		}

		if (_fileSystem.Execute.IsWindows &&
		    !ignoreMetadataErrors &&
		    Attributes.HasFlag(FileAttributes.ReadOnly))
		{
			throw ExceptionFactory.AccessToPathDenied(_fileSystem.Execute.IsNetFramework
				? _location.FriendlyName
				: _location.FullPath);
		}
#if FEATURE_FILESYSTEM_UNIXFILEMODE
		if (!deleteAccess && !_fileSystem.UnixFileModeStrategy
			.IsAccessGranted(_location.FullPath, _data.Extensibility, UnixFileMode, access))
		{
			throw ExceptionFactory.AccessDenied(_location.FullPath);
		}
#endif

		if (!_fileSystem.AccessControlStrategy
			.IsAccessGranted(_location.FullPath, Extensibility,
				new AccessRequest(access, deleteAccess)))
		{
			throw ExceptionFactory.AccessToPathDenied((onBehalfOfLocation ?? _location).FullPath);
		}

		if (_fileSystem.Storage.TryGetFileAccess(
			this,
			access,
			share,
			deleteAccess,
			ignoreFileShare,
			out FileHandle? fileHandle))
		{
			return fileHandle;
		}

		throw ExceptionFactory.ProcessCannotAccessTheFile(_location.FullPath,
			hResult ?? -2147024864);
	}

	/// <inheritdoc cref="IStorageContainer.UpdateLocation(IStorageLocation)" />
	public IStorageContainer UpdateLocation(IStorageLocation newLocation)
	{
		_location = newLocation;
		return this;
	}

	/// <inheritdoc cref="IStorageContainer.WriteBytes(byte[])" />
	public void WriteBytes(byte[] bytes)
		=> WriteBytesInternal(bytes, EventArgs.Empty);

	/// <inheritdoc cref="IStorageContainer.WriteRange(byte[], long)" />
	public void WriteRange(byte[] bytes, long offset)
	{
		byte[] currentBytes = _data.Bytes;
		long newLength = Math.Max(currentBytes.Length, offset + bytes.Length);
		byte[] newBytes = new byte[newLength];
		Array.Copy(currentBytes, newBytes, currentBytes.Length);
		Array.Copy(bytes, 0L, newBytes, offset, bytes.Length);
		WriteBytesInternal(newBytes, new BytesChangedEventArgs(bytes, offset));
	}

	private void WriteBytesInternal(byte[] bytes, EventArgs eventArgs)
	{
		NotifyFilters notifyFilters = NotifyFilters.LastAccess |
		                              NotifyFilters.LastWrite |
		                              NotifyFilters.Size;
		if (_fileSystem.Execute.IsLinux)
		{
			notifyFilters |= NotifyFilters.Security;
		}
		else if (_fileSystem.Execute.IsMac)
		{
			notifyFilters |= NotifyFilters.CreationTime;
		}

		TimeAdjustments timeAdjustment = TimeAdjustments.LastWriteTime;
		if (_fileSystem.Execute.IsWindows)
		{
			timeAdjustment |= TimeAdjustments.LastAccessTime;
		}

		ChangeDescription fileSystemChange =
			_fileSystem.ChangeHandler.NotifyPendingChange(WatcherChangeTypes.Changed,
				FileSystemTypes.File,
				notifyFilters,
				_location);
		if (_isUnlinked)
		{
			if (_data.LinkCount > 0)
			{
				// The file still exists under another hard link, but no longer at the path of this container.
				_location.Drive?.ChangeUsedBytes(bytes.Length - _data.Bytes.Length);
			}

			_data.Bytes = bytes;
			this.AdjustTimes(timeAdjustment);
			_data.RaiseBytesChanged(eventArgs);
			return;
		}

		_location.Drive?.ChangeUsedBytes(bytes.Length - _data.Bytes.Length);
		_data.Bytes = bytes;
		this.AdjustTimes(timeAdjustment);
		if (_fileSystem.Execute.IsWindows)
		{
			IStorageContainer? directoryContainer =
				_fileSystem.Storage.GetContainer(_location.GetParent());
			if (directoryContainer != null &&
			    directoryContainer is not NullContainer)
			{
				directoryContainer.AdjustTimes(TimeAdjustments.LastAccessTime);
			}
		}

		_fileSystem.ChangeHandler.NotifyCompletedChange(fileSystemChange);
		_data.RaiseBytesChanged(eventArgs);
	}

	#endregion

	/// <summary>
	///     Creates a new entry for the same file at the <paramref name="location" />, which shares the content and
	///     metadata with this container, like a hard link on a real file system.
	/// </summary>
	/// <remarks>
	///     The new entry only counts as a link to the file once it is registered with
	///     <see cref="TryRegisterLink()" />.
	/// </remarks>
	internal InMemoryContainer CreateLink(IStorageLocation location)
		=> new(_data, location, _fileSystem);

	/// <summary>
	///     Counts this container as an additional link to the file, so that the content is only released when the
	///     last link is removed with <see cref="Unlink()" />.
	/// </summary>
	/// <returns>
	///     <see langword="false" />, if the last link to the file was removed in the meantime, so that the file no
	///     longer exists; otherwise <see langword="true" />.
	/// </returns>
	internal bool TryRegisterLink()
	{
		int linkCount = Volatile.Read(ref _data.LinkCount);
		while (linkCount > 0)
		{
			int previous = Interlocked.CompareExchange(ref _data.LinkCount, linkCount + 1,
				linkCount);
			if (previous == linkCount)
			{
				return true;
			}

			linkCount = previous;
		}

		return false;
	}

	/// <summary>
	///     Create a new directory on the <paramref name="location" />.
	/// </summary>
	public static IStorageContainer NewDirectory(IStorageLocation location,
		MockFileSystem fileSystem)
	{
		return new InMemoryContainer(FileSystemTypes.Directory, location,
			fileSystem);
	}

	/// <summary>
	///     Create a new file on the <paramref name="location" />.
	/// </summary>
	public static IStorageContainer NewFile(IStorageLocation location,
		MockFileSystem fileSystem)
	{
		return new InMemoryContainer(FileSystemTypes.File, location,
			fileSystem);
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
	{
		if (Type == FileSystemTypes.Directory)
		{
			return $"{_location.FullPath}: Directory";
		}

		if (Type == FileSystemTypes.File)
		{
			return $"{_location.FullPath}: File ({_data.Bytes.Length} bytes)";
		}

		return $"{_location.FullPath}: Unknown Container";
	}

	internal FileAttributes AdjustAttributes(FileAttributes attributes)
	{
		if ((_fileSystem.Execute.IsLinux || _fileSystem.Execute.IsMac)
		    && _fileSystem.Execute.Path.GetFileName(_location.FullPath).StartsWith('.', StringComparison.Ordinal))
		{
			attributes |= FileAttributes.Hidden;
		}

#if FEATURE_FILESYSTEM_LINK
		if (LinkTarget != null)
		{
			attributes |= FileAttributes.ReparsePoint;
		}
#endif

		if (_data.IsEncrypted)
		{
			attributes |= FileAttributes.Encrypted;
		}

		if (Type == FileSystemTypes.Directory)
		{
			attributes |= FileAttributes.Directory;
		}
		else if (Type == FileSystemTypes.File)
		{
			attributes &= ~FileAttributes.Directory;
		}

		if (attributes == 0)
		{
			return FileAttributes.Normal;
		}

		return attributes;
	}

	/// <summary>
	///     The content and metadata of a file or directory, independent of the path it is stored at.
	/// </summary>
	private sealed class FileData(FileSystemTypes type)
	{
		public FileAttributes Attributes;
		public byte[] Bytes = Array.Empty<byte>();
		public readonly ITimeContainer CreationTime = new TimeContainer();
		public readonly FileSystemExtensibility Extensibility = new();
		public bool IsEncrypted;
		public readonly ITimeContainer LastAccessTime = new TimeContainer();
		public readonly ITimeContainer LastWriteTime = new TimeContainer();
		public string? LinkTarget;

		/// <summary>
		///     The number of paths in the storage that refer to this file.
		/// </summary>
		public int LinkCount = 1;

		public readonly FileSystemTypes Type = type;
#if FEATURE_FILESYSTEM_UNIXFILEMODE
		public UnixFileMode UnixFileMode = UnixFileMode.OtherRead |
		                                   UnixFileMode.GroupRead |
		                                   UnixFileMode.UserWrite |
		                                   UnixFileMode.UserRead;
#endif

		public event EventHandler? BytesChanged;

		public void RaiseBytesChanged(EventArgs eventArgs)
			=> BytesChanged?.Invoke(this, eventArgs);
	}

	internal sealed class TimeContainer : ITimeContainer
	{
		private DateTime _time;

		#region ITimeContainer Members

		/// <inheritdoc cref="ITimeContainer.Get(DateTimeKind)" />
		public DateTime Get(DateTimeKind kind)
			=> kind switch
			{
				DateTimeKind.Utc => _time.ToUniversalTime(),
				DateTimeKind.Local => _time.ToLocalTime(),
				_ => _time,
			};

		/// <inheritdoc cref="ITimeContainer.Set(DateTime, DateTimeKind)" />
		public void Set(DateTime time, DateTimeKind kind)
		{
			if (time.Kind == DateTimeKind.Unspecified)
			{
				_time = DateTime.SpecifyKind(time, kind);
			}
			else
			{
				_time = time;
			}
		}

		#endregion

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
			=> _time.ToUniversalTime()
				.ToString("yyyy-MM-dd HH:mm:ssZ", CultureInfo.InvariantCulture);
	}
}
