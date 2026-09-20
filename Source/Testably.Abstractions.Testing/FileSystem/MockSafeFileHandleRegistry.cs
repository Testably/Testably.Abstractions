#if FEATURE_FILESYSTEM_RANDOMACCESS
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     Keeps track of the <see cref="SafeFileHandle" />s that the <see cref="MockFileSystem" /> created itself in
///     <see cref="IFile.OpenHandle(string, FileMode, FileAccess, FileShare, FileOptions, long)" />.
///     <para />
///     A <see cref="SafeFileHandle" /> is sealed and wraps an operating system handle, so the mock cannot create one
///     that a real syscall would accept. Instead it hands out a handle with a synthetic value and remembers which file
///     that value stands for, which is the same indirection that <see cref="ISafeFileHandleStrategy" /> provides for
///     handles created outside of the <see cref="MockFileSystem" />.
/// </summary>
internal sealed class MockSafeFileHandleRegistry
{
	/// <summary>
	///     Synthetic handle values start far above any plausible file descriptor or handle value, so that a mock handle
	///     which is accidentally passed to a real syscall fails with an invalid-handle error rather than addressing an
	///     unrelated file.
	/// </summary>
	private const long FirstHandleValue = 0x4000_0000L;

	private readonly Dictionary<IntPtr, Entry> _entries = new();
	private readonly MockFileSystem _fileSystem;
#if NET9_0_OR_GREATER
	private readonly System.Threading.Lock _lock = new();
#else
	private readonly object _lock = new();
#endif
	/// <summary>
	///     Deletions waiting for the last handle to the file to close.
	/// </summary>
	private readonly List<Entry> _pendingDeletes = [];

	private volatile bool _hasWork;
	private long _nextHandleValue = FirstHandleValue;
	private bool _sweeping;

	internal MockSafeFileHandleRegistry(MockFileSystem fileSystem)
	{
		_fileSystem = fileSystem;
	}

	internal SafeFileHandle Open(string path,
		FileMode mode,
		FileAccess access,
		FileShare share,
		FileOptions options,
		long preallocationSize)
	{
		if (preallocationSize < 0)
		{
			throw ExceptionFactory.NonNegativeNumberRequired("preallocationSize");
		}

		FileModeHelper.ThrowIfInvalidModeAccess(mode, access);

		ReleaseClosedHandles();

		IStorageLocation location = _fileSystem.Storage
			.GetLocation(path.EnsureValidFormat(_fileSystem));
		location.ThrowExceptionIfNotFound(_fileSystem, true);

		IStorageContainer container = _fileSystem.Storage.GetContainer(location);
		if (container is NullContainer)
		{
			if (mode == FileMode.Open || mode == FileMode.Truncate)
			{
				throw ExceptionFactory.FileNotFound(location.FullPath);
			}

			container = _fileSystem.Storage
				.GetOrCreateContainer(location, InMemoryContainer.NewFile);
		}
		else if (container.Type == FileSystemTypes.Directory)
		{
			if (_fileSystem.Execute.IsWindows)
			{
				throw ExceptionFactory.AccessToPathDenied(location.FullPath);
			}

			throw ExceptionFactory.FileAlreadyExists(location.FullPath, 17);
		}
		else if (mode == FileMode.CreateNew)
		{
			throw ExceptionFactory.FileAlreadyExists(location.FullPath,
				_fileSystem.Execute.IsWindows ? -2147024816 : 17);
		}

		if (container.Attributes.HasFlag(FileAttributes.ReadOnly) &&
		    access.HasFlag(FileAccess.Write))
		{
			throw ExceptionFactory.AccessToPathDenied(location.FullPath);
		}

		// `deleteAccess` marks a delete *operation* — `FileHandle.GrantAccess` requires every other handle to have
		// been opened with exactly `FileShare.Delete` on Windows — so it is not what `FileOptions.DeleteOnClose`
		// means. Opening for deletion-on-close is an ordinary open; the deletion happens when the handle closes.
		IStorageAccessHandle accessLock = container.RequestAccess(access, share);

		if (mode == FileMode.Create || mode == FileMode.Truncate)
		{
			container.WriteBytes([]);
		}

		lock (_lock)
		{
			IntPtr value = new(_nextHandleValue++);
			SafeFileHandle handle = new(value, ownsHandle: false);
			_entries[value] = new Entry(
				handle,
				new SafeFileHandleMock(location.FullPath, mode, share),
				accessLock,
				location,
				container,
				access,
				mode,
				options);
			_hasWork = true;
			return handle;
		}
	}

	internal Entry? Resolve(SafeFileHandle handle)
	{
		ReleaseClosedHandles();

		lock (_lock)
		{
			IntPtr value = handle.DangerousGetHandle();
			if (_entries.TryGetValue(value, out Entry? entry))
			{
				if (handle.IsClosed)
				{
					throw ExceptionFactory.HandleIsClosed();
				}

				return entry;
			}

			// Handle values are issued sequentially and never reused, so a value within the issued range that is no
			// longer registered belonged to a handle this registry created and the caller has since closed.
			if (WasIssued(value))
			{
				throw ExceptionFactory.HandleIsClosed();
			}

			return null;
		}
	}

	internal bool IsKnown(SafeFileHandle handle)
	{
		lock (_lock)
		{
			IntPtr value = handle.DangerousGetHandle();
			return _entries.ContainsKey(value) || WasIssued(value);
		}
	}

	internal SafeFileHandleMock Map(SafeFileHandle handle)
		=> Resolve(handle)?.Mock
		   ?? _fileSystem.SafeFileHandleStrategy.MapSafeFileHandle(handle);

	/// <summary>
	///     A handle refers to the file that was opened, not to its name, so one this registry created resolves to the
	///     container it was opened on. Only a handle from elsewhere, which carries nothing but a path, is looked up by
	///     name.
	/// </summary>
	internal (IStorageContainer Container, FileAccess Access, FileMode Mode) GetContainer(
		SafeFileHandle handle)
	{
		if (handle is null)
		{
			throw new ArgumentNullException(nameof(handle));
		}

		if (Resolve(handle) is { } entry)
		{
			return (entry.Container, entry.Access, entry.Mode);
		}

		if (handle.IsClosed)
		{
			throw ExceptionFactory.HandleIsClosed();
		}

		SafeFileHandleMock mock = _fileSystem.SafeFileHandleStrategy.MapSafeFileHandle(handle);
		IStorageContainer container = _fileSystem.Storage
			.GetContainer(_fileSystem.Storage.GetLocation(mock.Path)
				.ThrowExceptionIfNotFound(_fileSystem));
		if (container is NullContainer)
		{
			throw ExceptionFactory.FileNotFound(mock.Path);
		}

		return (container, FileAccess.ReadWrite, mock.Mode);
	}

	/// <summary>
	///     <see cref="SafeFileHandle" /> is sealed, so the mock cannot be notified when one is closed and instead
	///     notices here, on the next registry access.
	/// </summary>
	internal void ReleaseClosedHandles()
	{
		if (!_hasWork)
		{
			return;
		}

		List<Entry>? released = null;
		lock (_lock)
		{
			if (_sweeping)
			{
				return;
			}

			List<IntPtr>? closed = null;
			foreach (KeyValuePair<IntPtr, Entry> item in _entries)
			{
				if (item.Value.Handle.IsClosed)
				{
					(closed ??= []).Add(item.Key);
				}
			}

			if (closed is null)
			{
				return;
			}

			foreach (IntPtr key in closed)
			{
				(released ??= []).Add(_entries[key]);
				_entries.Remove(key);
			}

			_sweeping = true;
		}

		try
		{
			foreach (Entry entry in released!)
			{
				entry.AccessLock.Dispose();
				if (!entry.Options.HasFlag(FileOptions.DeleteOnClose))
				{
					continue;
				}

				if (_fileSystem.Execute.IsWindows)
				{
					_pendingDeletes.Add(entry);
				}
				else
				{
					// Unix unlinks the name that was opened, as soon as this handle closes and whatever else still
					// holds the file open — so a file renamed since then survives, and a replacement under the old
					// name does not.
					_fileSystem.Storage.DeleteContainer(entry.Location, FileSystemTypes.File);
				}
			}

			// Windows removes the file once the last handle to it closes, and follows it across a rename.
			for (int i = _pendingDeletes.Count - 1; i >= 0; i--)
			{
				Entry pending = _pendingDeletes[i];
				if (IsStillOpen(pending.Container))
				{
					continue;
				}

				_pendingDeletes.RemoveAt(i);
				IStorageLocation? current = _fileSystem.Storage.GetLocation(pending.Container);
				if (current is not null)
				{
					_fileSystem.Storage.DeleteContainer(current, FileSystemTypes.File);
				}
			}
		}
		finally
		{
			lock (_lock)
			{
				_sweeping = false;
				_hasWork = _entries.Count > 0 || _pendingDeletes.Count > 0;
			}
		}
	}

	private bool IsStillOpen(IStorageContainer container)
	{
		lock (_lock)
		{
			foreach (KeyValuePair<IntPtr, Entry> item in _entries)
			{
				if (ReferenceEquals(item.Value.Container, container))
				{
					return true;
				}
			}

			return false;
		}
	}

	private bool WasIssued(IntPtr value)
	{
		long candidate = value.ToInt64();
		return candidate >= FirstHandleValue && candidate < _nextHandleValue;
	}

	internal sealed class Entry
	{
		internal Entry(SafeFileHandle handle,
			SafeFileHandleMock mock,
			IStorageAccessHandle accessLock,
			IStorageLocation location,
			IStorageContainer container,
			FileAccess access,
			FileMode mode,
			FileOptions options)
		{
			Handle = handle;
			Mock = mock;
			AccessLock = accessLock;
			Location = location;
			Container = container;
			Access = access;
			Mode = mode;
			Options = options;
		}

		internal FileAccess Access { get; }
		internal IStorageAccessHandle AccessLock { get; }
		internal IStorageContainer Container { get; }
		internal SafeFileHandle Handle { get; }

		internal IStorageLocation Location { get; }

		internal FileMode Mode { get; }
		internal SafeFileHandleMock Mock { get; }
		internal FileOptions Options { get; }
	}
}
#endif
