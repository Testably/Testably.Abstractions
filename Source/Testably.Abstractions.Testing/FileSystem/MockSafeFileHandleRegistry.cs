#if FEATURE_FILESYSTEM_RANDOMACCESS
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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

	/// <summary>
	///     Shared by all <see cref="MockFileSystem" />s, so that a handle passed to another instance is foreign there
	///     instead of resolving to an unrelated file that happens to have the same value.
	/// </summary>
	private static long _lastHandleValue = FirstHandleValue - 1;

	private const FileOptions ValidFileOptions = FileOptions.WriteThrough |
	                                             FileOptions.Asynchronous |
	                                             FileOptions.RandomAccess |
	                                             FileOptions.DeleteOnClose |
	                                             FileOptions.SequentialScan |
	                                             FileOptions.Encrypted |
	                                             (FileOptions)0x20000000; // NoBuffering

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
		IStorageLocation location = _fileSystem.Storage.GetLocation(
			ValidateArguments(path, mode, access, share, options, preallocationSize));
		IStorageContainer container =
			FileModeHelper.GetFileContainer(_fileSystem, location, mode, access);

		// `deleteAccess` marks a delete *operation* — `FileHandle.GrantAccess` requires every other handle to have
		// been opened with `FileShare.Delete` on Windows — so it is not what `FileOptions.DeleteOnClose` means.
		// Opening for deletion-on-close is an ordinary open; the deletion happens when the handle closes.
		IStorageAccessHandle accessLock = container.RequestAccess(access, share);

		if (mode is FileMode.Create or FileMode.Truncate)
		{
			try
			{
				container.WriteBytes([]);
			}
			catch
			{
				accessLock.Dispose();
				throw;
			}
		}

		lock (_lock)
		{
			IntPtr value = new(Interlocked.Increment(ref _lastHandleValue));
			SafeFileHandle handle = new(value, ownsHandle: false);
			_entries[value] = new Entry(
				new WeakReference<SafeFileHandle>(handle),
				new SafeFileHandleMock(location.FullPath, mode, share),
				accessLock,
				location,
				container,
				access,
				options);
			_hasWork = true;
			return handle;
		}
	}

	/// <summary>
	///     A handle refers to the file that was opened, not to its name, so one this registry created resolves to the
	///     container it was opened on. Only a handle from elsewhere, which carries nothing but a path, is looked up by
	///     name.
	/// </summary>
	internal (IStorageContainer Container, FileAccess Access) GetContainer(SafeFileHandle handle)
	{
		if (Resolve(handle) is { } entry)
		{
			return (entry.Container, entry.Access);
		}

		SafeFileHandleMock mock = MapForeign(handle);
		IStorageContainer container = _fileSystem.Storage
			.GetContainer(_fileSystem.Storage.GetLocation(mock.Path)
				.ThrowExceptionIfNotFound(_fileSystem));
		return (container, FileAccess.ReadWrite);
	}

	internal SafeFileHandleMock Map(SafeFileHandle handle)
		=> Resolve(handle)?.Mock ?? MapForeign(handle);

	/// <summary>
	///     <see cref="SafeFileHandle" /> is sealed, so the mock cannot be notified when one is closed. Instead, the
	///     storage calls this before it answers whether a file exists or may be opened, which is where a closed handle
	///     becomes observable.
	/// </summary>
	/// <remarks>
	///     A closed handle is released here, together with its share lock and any deletion it requested, and nothing
	///     here throws: a real file system also ignores a delete-on-close that fails.
	/// </remarks>
	internal void ReleaseClosedHandles()
	{
		if (!_hasWork)
		{
			return;
		}

		List<Entry> released = [];
		List<IntPtr> closed = [];
		lock (_lock)
		{
			if (_sweeping)
			{
				return;
			}

			foreach (KeyValuePair<IntPtr, Entry> item in _entries)
			{
				if (!item.Value.Handle.TryGetTarget(out SafeFileHandle? handle) ||
				    handle.IsClosed)
				{
					closed.Add(item.Key);
					released.Add(item.Value);
				}
			}

			if (released.Count == 0 && _pendingDeletes.Count == 0)
			{
				return;
			}

			foreach (IntPtr value in closed)
			{
				_entries.Remove(value);
			}

			_sweeping = true;
		}

		try
		{
			foreach (Entry entry in released)
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
					// Unix unlinks the name that was opened as soon as this handle closes, whatever else still holds
					// the file open.
					TryDelete(entry.Location);
				}
			}

			// Windows removes the file once the last handle to it closes.
			_pendingDeletes.RemoveAll(IsDeletedOrGone);
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

	private bool IsDeletedOrGone(Entry pending)
	{
		if (!ReferenceEquals(_fileSystem.Storage.GetContainer(pending.Location),
			pending.Container))
		{
			return true;
		}

		// Exclusive access is only granted when nothing else, handle or stream, holds the file.
		if (!_fileSystem.Storage.TryGetFileAccess(pending.Location, FileAccess.ReadWrite,
			FileShare.None, deleteAccess: false, ignoreFileShare: false,
			out FileHandle? probe))
		{
			return false;
		}

		probe.Dispose();
		TryDelete(pending.Location);
		return true;
	}

	private SafeFileHandleMock MapForeign(SafeFileHandle handle)
	{
		if (handle.IsClosed)
		{
			throw ExceptionFactory.HandleIsClosed();
		}

		return _fileSystem.SafeFileHandleStrategy.MapSafeFileHandle(handle);
	}

	private Entry? Resolve(SafeFileHandle handle)
	{
		if (handle is null)
		{
			throw new ArgumentNullException(nameof(handle));
		}

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

			return null;
		}
	}

	private void TryDelete(IStorageLocation location)
	{
		try
		{
			_fileSystem.Storage.DeleteContainer(location, FileSystemTypes.File);
		}
		catch (IOException)
		{
			// The name is gone, or its directory is.
		}
		catch (UnauthorizedAccessException)
		{
			// A directory now has the name.
		}
	}

	/// <summary>
	///     Validates in the order of the runtime's <c>FileStreamHelpers.ValidateArguments</c>, so that a call with more
	///     than one invalid argument reports the same one.
	/// </summary>
	private string ValidateArguments(string path,
		FileMode mode,
		FileAccess access,
		FileShare share,
		FileOptions options,
		long preallocationSize)
	{
		path = path.EnsureValidFormat(_fileSystem, nameof(path), includeIsEmptyCheck: true);

		if (mode is < FileMode.CreateNew or > FileMode.Append)
		{
			throw ExceptionFactory.EnumValueOutOfRange(nameof(mode));
		}

		if (access is < FileAccess.Read or > FileAccess.ReadWrite)
		{
			throw ExceptionFactory.EnumValueOutOfRange(nameof(access));
		}

		FileShare shareWithoutInheritable = share & ~FileShare.Inheritable;
		if (shareWithoutInheritable is < FileShare.None or > (FileShare.ReadWrite | FileShare.Delete))
		{
			throw ExceptionFactory.EnumValueOutOfRange(nameof(share));
		}

		if ((options & ~ValidFileOptions) != 0)
		{
			throw ExceptionFactory.EnumValueOutOfRange(nameof(options));
		}

		if (preallocationSize < 0)
		{
			throw ExceptionFactory.NonNegativeNumberRequired(nameof(preallocationSize));
		}

		FileModeHelper.ThrowIfInvalidModeAccess(mode, access);

		if (preallocationSize > 0)
		{
			if (!access.HasFlag(FileAccess.Write))
			{
				throw ExceptionFactory.PreallocationRequiresWriteAccess(access);
			}

			if (mode is not (FileMode.Create or FileMode.CreateNew))
			{
				throw ExceptionFactory.PreallocationRequiresNewFile(mode);
			}
		}

		return path;
	}

	internal sealed record Entry(
		WeakReference<SafeFileHandle> Handle,
		SafeFileHandleMock Mock,
		IStorageAccessHandle AccessLock,
		IStorageLocation Location,
		IStorageContainer Container,
		FileAccess Access,
		FileOptions Options);
}
#endif
