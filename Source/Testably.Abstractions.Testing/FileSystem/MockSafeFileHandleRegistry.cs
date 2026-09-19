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

	private readonly HashSet<IntPtr> _closed = [];
	private readonly Dictionary<IntPtr, Entry> _entries = new();
	private readonly MockFileSystem _fileSystem;
	private bool _sweeping;
	private volatile bool _hasEntries;
#if NET9_0_OR_GREATER
	private readonly System.Threading.Lock _lock = new();
#else
	private readonly object _lock = new();
#endif
	private long _nextHandleValue = FirstHandleValue;

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
				access,
				options);
			_hasEntries = true;
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
			if (_closed.Contains(value) || (handle.IsClosed && _entries.ContainsKey(value)))
			{
				throw ExceptionFactory.HandleIsClosed();
			}

			return _entries.TryGetValue(value, out Entry? entry) ? entry : null;
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
			return _entries.ContainsKey(handle.DangerousGetHandle());
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
	///     Returns the container for the file that <paramref name="handle" /> refers to, together with the
	///     <see cref="FileAccess" /> the handle was opened with.
	/// </summary>
	/// <remarks>
	///     A handle that originated outside of the <see cref="MockFileSystem" /> carries no access information, so it
	///     is treated as <see cref="FileAccess.ReadWrite" />.
	/// </remarks>
	internal (IStorageContainer Container, FileAccess Access) GetContainer(SafeFileHandle handle)
	{
		Entry? entry = Resolve(handle);
		SafeFileHandleMock mock = entry?.Mock
		                          ?? _fileSystem.SafeFileHandleStrategy.MapSafeFileHandle(handle);

		IStorageContainer container = _fileSystem.Storage
			.GetContainer(_fileSystem.Storage.GetLocation(mock.Path)
				.ThrowExceptionIfNotFound(_fileSystem));
		if (container is NullContainer)
		{
			throw ExceptionFactory.FileNotFound("");
		}

		return (container, entry?.Access ?? FileAccess.ReadWrite);
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
		if (!_hasEntries)
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
				_closed.Add(key);
			}

			_hasEntries = _entries.Count > 0;
			_sweeping = true;
		}

		try
		{
			foreach (Entry entry in released!)
			{
				entry.AccessLock.Dispose();
				if (entry.Options.HasFlag(FileOptions.DeleteOnClose))
				{
					_fileSystem.Storage.DeleteContainer(entry.Location, FileSystemTypes.File);
				}
			}
		}
		finally
		{
			lock (_lock)
			{
				_sweeping = false;
			}
		}
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
			FileAccess access,
			FileOptions options)
		{
			Handle = handle;
			Mock = mock;
			AccessLock = accessLock;
			Location = location;
			Access = access;
			Options = options;
		}

		internal FileAccess Access { get; }
		internal IStorageAccessHandle AccessLock { get; }
		internal SafeFileHandle Handle { get; }
		internal IStorageLocation Location { get; }
		internal SafeFileHandleMock Mock { get; }
		internal FileOptions Options { get; }
	}
}
#endif
