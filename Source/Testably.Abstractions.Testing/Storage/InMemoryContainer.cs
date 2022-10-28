using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.FileSystem;
using Testably.Abstractions.Testing.Internal;
using Testably.Abstractions.TimeSystem;
using static Testably.Abstractions.Testing.MockFileSystem;
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
	private readonly FileSystemExtensionContainer _extensionContainer = new();

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
			value &= Execute.OnWindows(
				() => FileAttributes.Directory |
				      FileAttributes.ReadOnly |
				      FileAttributes.Archive |
				      FileAttributes.Hidden |
				      FileAttributes.NoScrubData |
				      FileAttributes.NotContentIndexed |
				      FileAttributes.Offline |
				      FileAttributes.System |
				      FileAttributes.Temporary,
				() => Execute.OnLinux(
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

	/// <inheritdoc cref="IStorageContainer.ExtensionContainer" />
	public IFileSystemExtensionContainer ExtensionContainer
		=> _extensionContainer;

	/// <inheritdoc cref="IFileSystemExtensionPoint.FileSystem" />
	public IFileSystem FileSystem => _fileSystem;

	/// <inheritdoc cref="IStorageContainer.LastAccessTime" />
	public ITimeContainer LastAccessTime { get; } = new TimeContainer();

	/// <inheritdoc cref="IStorageContainer.LastWriteTime" />
	public ITimeContainer LastWriteTime { get; } = new TimeContainer();

	/// <inheritdoc cref="IStorageContainer.LinkTarget" />
	public string? LinkTarget { get; set; }

	/// <inheritdoc cref="ITimeSystemExtensionPoint.TimeSystem" />
	public ITimeSystem TimeSystem => _fileSystem.TimeSystem;

	/// <inheritdoc cref="IStorageContainer.Type" />
	public FileSystemTypes Type { get; }

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

	/// <inheritdoc cref="IStorageContainer.RequestAccess(FileAccess, FileShare, bool)" />
	public IStorageAccessHandle RequestAccess(FileAccess access, FileShare share,
	                                          bool ignoreMetadataError = true)
	{
		if (_location.Drive == null)
		{
			throw ExceptionFactory.DirectoryNotFound(_location.FullPath);
		}

		if (!_location.Drive.IsReady)
		{
			throw ExceptionFactory.NetworkPathNotFound(_location.FullPath);
		}

		Execute.OnWindowsIf(
			!ignoreMetadataError && Attributes.HasFlag(FileAttributes.ReadOnly),
			() => throw ExceptionFactory.AccessToPathDenied());

		if (CanGetAccess(access, share))
		{
			Guid guid = Guid.NewGuid();
			FileHandle fileHandle = new(guid, ReleaseAccess, access, share);
			_fileHandles.TryAdd(guid, fileHandle);
			return fileHandle;
		}

		throw ExceptionFactory.ProcessCannotAccessTheFile(_location.FullPath);
	}

	/// <inheritdoc cref="IStorageContainer.WriteBytes(byte[])" />
	public void WriteBytes(byte[] bytes)
	{
		NotifyFilters notifyFilters = NotifyFilters.LastAccess |
		                              NotifyFilters.LastWrite |
		                              NotifyFilters.Size;
		Execute.OnLinux(()
			=> notifyFilters |= NotifyFilters.Security);
		Execute.OnMac(()
			=> notifyFilters |= NotifyFilters.CreationTime);

		TimeAdjustments timeAdjustment = TimeAdjustments.LastWriteTime;
		Execute.OnWindows(()
			=> timeAdjustment |= TimeAdjustments.LastAccessTime);

		ChangeDescription fileSystemChange =
			_fileSystem.ChangeHandler.NotifyPendingChange(WatcherChangeTypes.Changed,
				FileSystemTypes.File,
				notifyFilters,
				_location);
		_location.Drive?.ChangeUsedBytes(bytes.Length - _bytes.Length);
		_bytes = bytes;
		this.AdjustTimes(timeAdjustment);
		_fileSystem.ChangeHandler.NotifyCompletedChange(fileSystemChange);
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

	internal FileAttributes AdjustAttributes(FileAttributes attributes)
	{
		if (Path.GetFileName(_location.FullPath).StartsWith("."))
		{
			FileAttributes attr = attributes;
			attributes = Execute.OnLinux(
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

		if (attributes == 0)
		{
			return FileAttributes.Normal;
		}

		return attributes;
	}

	private bool CanGetAccess(FileAccess access, FileShare share)
	{
		foreach (KeyValuePair<Guid, FileHandle> fileHandle in _fileHandles)
		{
			if (!fileHandle.Value.GrantAccess(access, share))
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
	}

	private sealed class FileHandle : IStorageAccessHandle
	{
		private readonly Guid _key;
		private readonly Action<Guid> _releaseCallback;

		public FileHandle(Guid key, Action<Guid> releaseCallback, FileAccess access,
		                  FileShare share)
		{
			_releaseCallback = releaseCallback;
			Access = access;
			Share = Execute.OnWindows(
				() => share,
				() => share == FileShare.None
					? FileShare.None
					: FileShare.ReadWrite);

			_key = key;
		}

		#region IStorageAccessHandle Members

		/// <inheritdoc cref="IStorageAccessHandle.Access" />
		public FileAccess Access { get; }

		/// <inheritdoc cref="IStorageAccessHandle.Share" />
		public FileShare Share { get; }

		/// <inheritdoc cref="IDisposable.Dispose()" />
		public void Dispose()
		{
			_releaseCallback.Invoke(_key);
		}

		#endregion

		public bool GrantAccess(FileAccess access, FileShare share)
		{
			Execute.NotOnWindows(()
				=> share = FileShare.ReadWrite);

			return CheckAccessWithShare(access, Share) &&
			       CheckAccessWithShare(Access, share);
		}

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