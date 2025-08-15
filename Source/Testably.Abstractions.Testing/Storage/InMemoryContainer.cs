using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Testably.Abstractions.Helpers;
using Testably.Abstractions.Testing.FileSystem;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.TimeSystem;
using static Testably.Abstractions.Testing.Storage.IStorageContainer;

namespace Testably.Abstractions.Testing.Storage;

internal sealed class InMemoryContainer : IStorageContainer
{
	private FileAttributes _attributes;
	private byte[] _bytes = Array.Empty<byte>();
	private readonly FileSystemExtensibility _extensibility = new();
	private readonly MockFileSystem _fileSystem;
	private bool _isEncrypted;
	private readonly IStorageLocation _location;

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	private UnixFileMode _unixFileMode = UnixFileMode.OtherRead |
	                                     UnixFileMode.GroupRead |
	                                     UnixFileMode.UserWrite |
	                                     UnixFileMode.UserRead;
#endif

	public InMemoryContainer(FileSystemTypes type,
		IStorageLocation location,
		MockFileSystem fileSystem)
	{
		_location = location;
		_fileSystem = fileSystem;
		Type = type;
		this.AdjustTimes(TimeAdjustments.All);
	}

	#region IStorageContainer Members

	/// <inheritdoc cref="IStorageContainer.Attributes" />
	public FileAttributes Attributes
	{
		get => AdjustAttributes(_attributes);
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

			_attributes = value;
		}
	}

	/// <inheritdoc cref="IStorageContainer.CreationTime" />
	public ITimeContainer CreationTime { get; } = new TimeContainer();

	/// <inheritdoc cref="IStorageContainer.Extensibility" />
	public IFileSystemExtensibility Extensibility
		=> _extensibility;

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem => _fileSystem;

	/// <inheritdoc cref="IStorageContainer.LastAccessTime" />
	public ITimeContainer LastAccessTime { get; } = new TimeContainer();

	/// <inheritdoc cref="IStorageContainer.LastWriteTime" />
	public ITimeContainer LastWriteTime { get; } = new TimeContainer();

	/// <inheritdoc cref="IStorageContainer.LinkTarget" />
	public string? LinkTarget { get; set; }

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem => _fileSystem.TimeSystem;

	/// <inheritdoc cref="IStorageContainer.Type" />
	public FileSystemTypes Type { get; }

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	/// <inheritdoc cref="IStorageContainer.UnixFileMode" />
	public UnixFileMode UnixFileMode
	{
		get => _unixFileMode;
		set
		{
			_unixFileMode = value;
			_fileSystem.UnixFileModeStrategy.OnSetUnixFileMode(
				_location.FullPath,
				_extensibility,
				_unixFileMode);
		}
	}
#endif

	/// <inheritdoc cref="IStorageContainer.AppendBytes(byte[])" />
	public void AppendBytes(byte[] bytes)
	{
		WriteBytes(_bytes.Concat(bytes).ToArray());
	}

	/// <inheritdoc cref="IStorageContainer.BytesChanged" />
	public event EventHandler? BytesChanged;

	/// <inheritdoc cref="IStorageContainer.ClearBytes()" />
	public void ClearBytes()
	{
		_location.Drive?.ChangeUsedBytes(0 - _bytes.Length);
		_bytes = Array.Empty<byte>();
		BytesChanged?.Invoke(this, EventArgs.Empty);
	}

	/// <inheritdoc cref="IStorageContainer.Decrypt()" />
	public void Decrypt()
	{
		if (!_isEncrypted)
		{
			return;
		}

		using (RequestAccess(FileAccess.Write, FileShare.Read))
		{
			_isEncrypted = false;
			WriteBytes(EncryptionHelper.Decrypt(GetBytes()));
		}
	}

	/// <inheritdoc cref="IStorageContainer.Encrypt()" />
	public void Encrypt()
	{
		if (_isEncrypted)
		{
			return;
		}

		using (RequestAccess(FileAccess.Write, FileShare.Read))
		{
			_isEncrypted = true;
			WriteBytes(EncryptionHelper.Encrypt(GetBytes()));
		}
	}

	/// <inheritdoc cref="IStorageContainer.GetBytes()" />
	public byte[] GetBytes() => _bytes;

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
			throw ExceptionFactory.AccessToPathDenied();
		}
#if FEATURE_FILESYSTEM_UNIXFILEMODE
		if (!deleteAccess && !_fileSystem.UnixFileModeStrategy
			.IsAccessGranted(_location.FullPath, _extensibility, UnixFileMode, access))
		{
			throw ExceptionFactory.AccessDenied(_location.FullPath);
		}
#endif

		if (!_fileSystem.AccessControlStrategy
			.IsAccessGranted(_location.FullPath, Extensibility))
		{
			throw ExceptionFactory.AccessToPathDenied((onBehalfOfLocation ?? _location).FullPath);
		}

		if (_fileSystem.Storage.TryGetFileAccess(
			_location,
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

	/// <inheritdoc cref="IStorageContainer.WriteBytes(byte[])" />
	public void WriteBytes(byte[] bytes)
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
		_location.Drive?.ChangeUsedBytes(bytes.Length - _bytes.Length);
		_bytes = bytes;
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
		BytesChanged?.Invoke(this, EventArgs.Empty);
	}

	#endregion

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
			return $"{_location.FullPath}: File ({_bytes.Length} bytes)";
		}

		return $"{_location.FullPath}: Unknown Container";
	}

	internal FileAttributes AdjustAttributes(FileAttributes attributes)
	{
		if (_fileSystem.Execute.IsLinux &&
		    _fileSystem.Execute.Path.GetFileName(_location.FullPath).StartsWith('.'))
		{
			attributes |= FileAttributes.Hidden;
		}

#if FEATURE_FILESYSTEM_LINK
		if (LinkTarget != null)
		{
			attributes |= FileAttributes.ReparsePoint;
		}
#endif

		if (_isEncrypted)
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
