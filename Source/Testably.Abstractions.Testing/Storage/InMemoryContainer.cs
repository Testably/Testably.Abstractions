using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.Helpers;
using Testably.Abstractions.Testing.FileSystem;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.TimeSystem;
using static Testably.Abstractions.Testing.Storage.IStorageContainer;

namespace Testably.Abstractions.Testing.Storage;

internal class InMemoryContainer : IStorageContainer
{
	private FileAttributes _attributes;
	private byte[] _bytes = Array.Empty<byte>();
	private readonly ConcurrentDictionary<Guid, FileHandle> _fileHandles = new();
	private readonly MockFileSystem _fileSystem;
	private bool _isEncrypted;
	private readonly IStorageLocation _location;
	private readonly FileSystemExtensibility _extensibility = new();

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
			value &= _fileSystem.Execute.OnWindows(
				() => FileAttributes.Directory |
				      FileAttributes.ReadOnly |
				      FileAttributes.Archive |
				      FileAttributes.Hidden |
				      FileAttributes.NoScrubData |
				      FileAttributes.NotContentIndexed |
				      FileAttributes.Offline |
				      FileAttributes.System |
				      FileAttributes.Temporary,
				() => _fileSystem.Execute.OnLinux(
					() => FileAttributes.Directory |
					      FileAttributes.ReadOnly,
					() => FileAttributes.Hidden |
					      FileAttributes.Directory |
					      FileAttributes.ReadOnly));

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
	public UnixFileMode UnixFileMode { get; set; } = (UnixFileMode)(-1);
#endif

	/// <inheritdoc cref="IStorageContainer.AppendBytes(byte[])" />
	public void AppendBytes(byte[] bytes)
	{
		WriteBytes(_bytes.Concat(bytes).ToArray());
	}

	/// <inheritdoc cref="IStorageContainer.ClearBytes()" />
	public void ClearBytes()
	{
		_location.Drive?.ChangeUsedBytes(0 - _bytes.Length);
		_bytes = Array.Empty<byte>();
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

	/// <inheritdoc cref="IStorageContainer.RequestAccess(FileAccess, FileShare, bool, bool, int?)" />
	public IStorageAccessHandle RequestAccess(FileAccess access, FileShare share,
		bool deleteAccess = false,
		bool ignoreMetadataErrors = true,
		int? hResult = null)
	{
		if (_location.Drive == null)
		{
			throw ExceptionFactory.DirectoryNotFound(_location.FullPath);
		}

		if (!_location.Drive.IsReady)
		{
			throw ExceptionFactory.NetworkPathNotFound(_location.FullPath);
		}

		_fileSystem.Execute.OnWindowsIf(
			!ignoreMetadataErrors && Attributes.HasFlag(FileAttributes.ReadOnly),
			() => throw ExceptionFactory.AccessToPathDenied());

		if (!_fileSystem.AccessControlStrategy
			.IsAccessGranted(_location.FullPath, Extensibility))
		{
			throw ExceptionFactory.AclAccessToPathDenied(_location.FullPath);
		}

		if (CanGetAccess(access, share, deleteAccess))
		{
			Guid guid = Guid.NewGuid();
			FileHandle fileHandle = new(
				_fileSystem, guid, ReleaseAccess, access, share, deleteAccess);
			_fileHandles.TryAdd(guid, fileHandle);
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
		_fileSystem.Execute.OnLinux(()
			=> notifyFilters |= NotifyFilters.Security);
		_fileSystem.Execute.OnMac(()
			=> notifyFilters |= NotifyFilters.CreationTime);

		TimeAdjustments timeAdjustment = TimeAdjustments.LastWriteTime;
		_fileSystem.Execute.OnWindows(()
			=> timeAdjustment |= TimeAdjustments.LastAccessTime);

		ChangeDescription fileSystemChange =
			_fileSystem.ChangeHandler.NotifyPendingChange(WatcherChangeTypes.Changed,
				FileSystemTypes.File,
				notifyFilters,
				_location);
		_location.Drive?.ChangeUsedBytes(bytes.Length - _bytes.Length);
		_bytes = bytes;
		this.AdjustTimes(timeAdjustment);
		_fileSystem.Execute.OnWindows(() =>
		{
			IStorageContainer? directoryContainer =
				_fileSystem.Storage.GetContainer(_location.GetParent());
			if (directoryContainer != null &&
			    directoryContainer is not NullContainer)
			{
				directoryContainer.AdjustTimes(TimeAdjustments.LastAccessTime);
			}
		});
		_fileSystem.ChangeHandler.NotifyCompletedChange(fileSystemChange);
	}

	#endregion

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

	internal FileAttributes AdjustAttributes(FileAttributes attributes)
	{
		if (Path.GetFileName(_location.FullPath).StartsWith('.'))
		{
			FileAttributes attr = attributes;
			attributes = _fileSystem.Execute.OnLinux(
				() => attr | FileAttributes.Hidden,
				() => attr);
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

	private bool CanGetAccess(FileAccess access, FileShare share, bool deleteAccess)
	{
		foreach (KeyValuePair<Guid, FileHandle> fileHandle in _fileHandles)
		{
			if (!fileHandle.Value.GrantAccess(access, share, deleteAccess))
			{
				return false;
			}
		}

		return true;
	}

	private void ReleaseAccess(Guid guid)
	{
		_fileHandles.TryRemove(guid, out _);
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
				_ => _time
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
			=> _time.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ssZ");
	}

	private sealed class FileHandle : IStorageAccessHandle
	{
		private readonly Guid _key;
		private readonly MockFileSystem _fileSystem;
		private readonly Action<Guid> _releaseCallback;

		public FileHandle(MockFileSystem fileSystem,
			Guid key,
			Action<Guid> releaseCallback,
			FileAccess access,
			FileShare share,
			bool deleteAccess)
		{
			_fileSystem = fileSystem;
			_releaseCallback = releaseCallback;
			Access = access;
			DeleteAccess = deleteAccess;
			Share = _fileSystem.Execute.OnWindows(
				() => share,
				() => share == FileShare.None
					? FileShare.None
					: FileShare.ReadWrite);

			_key = key;
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
			_releaseCallback.Invoke(_key);
		}

		#endregion

		public bool GrantAccess(FileAccess access, FileShare share, bool deleteAccess)
		{
			FileShare usedShare = share;
			_fileSystem.Execute.NotOnWindows(()
				=> usedShare = FileShare.ReadWrite);
			if (deleteAccess)
			{
				return !_fileSystem.Execute.IsWindows || Share == FileShare.Delete;
			}

			return CheckAccessWithShare(access, Share) &&
			       CheckAccessWithShare(Access, usedShare);
		}

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
			=> $"{(DeleteAccess ? "Delete" : Access)} | {Share}";

		private static bool CheckAccessWithShare(FileAccess access, FileShare share)
		{
			switch (access)
			{
				case FileAccess.Read:
					return share.HasFlag(FileShare.Read);
				case FileAccess.Write:
					return share.HasFlag(FileShare.Write);
				default:
					return share == FileShare.ReadWrite;
			}
		}
	}
}
