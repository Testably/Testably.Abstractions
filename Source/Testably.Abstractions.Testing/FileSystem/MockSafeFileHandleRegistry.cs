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
	///     Files whose last <see cref="FileOptions.DeleteOnClose" /> handle has been closed while other handles were
	///     still open on them. They are deleted once the last of those closes.
	/// </summary>
	private readonly List<Entry> _pendingDeletes = [];

	private volatile bool _hasWork;
	private long _nextHandleValue = FirstHandleValue;
	private bool _sweeping;

	internal MockSafeFileHandleRegistry(MockFileSystem fileSystem)
	{
		_fileSystem = fileSystem;
	}

	/// <summary>
	///     Opens <paramref name="path" /> and returns a <see cref="SafeFileHandle" /> that this registry can resolve
	///     back to the file.
	/// </summary>
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

		// Release the share locks of handles that were closed since the last access, before requesting a new one.
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

	/// <summary>
	///     Resolves a <paramref name="handle" /> that this registry created, or returns <see langword="null" /> if the
	///     handle originated elsewhere and has to be mapped by the <see cref="ISafeFileHandleStrategy" /> instead.
	/// </summary>
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

	/// <summary>
	///     Indicates whether <paramref name="handle" /> was created by this registry, irrespective of whether it is
	///     still open.
	/// </summary>
	internal bool IsKnown(SafeFileHandle handle)
	{
		lock (_lock)
		{
			IntPtr value = handle.DangerousGetHandle();
			return _entries.ContainsKey(value) || WasIssued(value);
		}
	}

	/// <summary>
	///     Maps <paramref name="handle" /> to the file it refers to, either because this registry created it or,
	///     failing that, by asking the registered <see cref="ISafeFileHandleStrategy" />.
	/// </summary>
	internal SafeFileHandleMock Map(SafeFileHandle handle)
		=> Resolve(handle)?.Mock
		   ?? _fileSystem.SafeFileHandleStrategy.MapSafeFileHandle(handle);

	/// <summary>
	///     Returns the file that <paramref name="handle" /> refers to, together with the <see cref="FileAccess" /> and
	///     <see cref="FileMode" /> the handle was opened with.
	/// </summary>
	/// <remarks>
	///     A handle refers to the file that was opened, not to its name: it keeps working when the file is renamed, and
	///     does not start referring to whatever is later created under the original path. A handle created by this
	///     registry is therefore resolved to the container it was opened on, and only a handle that originated
	///     elsewhere — which carries nothing but a path, and no access information — is looked up by name.
	/// </remarks>
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

		// A handle from elsewhere is resolved by the strategy, but a disposed one is unusable whatever its origin.
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
	///     Releases the file share locks held by handles that the caller has since disposed, and applies
	///     <see cref="FileOptions.DeleteOnClose" /> for them.
	///     <para />
	///     <see cref="SafeFileHandle" /> is sealed, so the mock cannot be notified when one is closed and instead
	///     notices on the next registry access.
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
				if (entry.Options.HasFlag(FileOptions.DeleteOnClose))
				{
					_pendingDeletes.Add(entry);
				}
			}

			// The file is removed when the last handle to it is closed, not the first, so a deletion stays pending
			// until no handle refers to the file any more.
			for (int i = _pendingDeletes.Count - 1; i >= 0; i--)
			{
				Entry pending = _pendingDeletes[i];
				if (IsStillOpen(pending.Container))
				{
					continue;
				}

				_pendingDeletes.RemoveAt(i);

				// The file may have been renamed since the handle was opened, and a container survives a rename, so
				// the deletion has to follow the container rather than the path it was opened at.
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

	/// <summary>
	///     Indicates whether <paramref name="value" /> lies in the range of handle values this registry has issued.
	/// </summary>
	private bool WasIssued(IntPtr value)
	{
		long candidate = value.ToInt64();
		return candidate >= FirstHandleValue && candidate < _nextHandleValue;
	}

	/// <summary>
	///     The file behind a <see cref="SafeFileHandle" /> created by the <see cref="MockFileSystem" />.
	/// </summary>
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

		/// <summary>
		///     The location the handle was opened on, which a rename can invalidate.
		/// </summary>
		internal IStorageLocation Location { get; }

		internal FileMode Mode { get; }
		internal SafeFileHandleMock Mock { get; }
		internal FileOptions Options { get; }
	}
}
#endif
