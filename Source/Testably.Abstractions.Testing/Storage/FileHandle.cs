using System;
using System.IO;

namespace Testably.Abstractions.Testing.Storage;

internal sealed class FileHandle : IStorageAccessHandle
{
	public static IStorageAccessHandle Ignore { get; } = new IgnoreFileHandle();
	private readonly MockFileSystem _fileSystem;
	private readonly Guid _key;
	private readonly Action<Guid> _releaseCallback;

	public FileHandle(MockFileSystem fileSystem, Guid key, Action<Guid> releaseCallback,
		FileAccess access,
		FileShare share, bool deleteAccess)
	{
		_fileSystem = fileSystem;
		_releaseCallback = releaseCallback;
		Access = access;
		DeleteAccess = deleteAccess;
		if (_fileSystem.Execute.IsWindows)
		{
			Share = share;
		}
		else
		{
			Share = share == FileShare.None
				? FileShare.None
				: FileShare.ReadWrite;
		}

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

	public bool GrantAccess(
		FileAccess access,
		FileShare share,
		bool deleteAccess,
		bool ignoreFileShare)
	{
		FileShare usedShare = share;
		FileShare currentShare = Share;
		if (!_fileSystem.Execute.IsWindows)
		{
			usedShare = FileShare.ReadWrite;
			if (ignoreFileShare)
			{
				currentShare = FileShare.ReadWrite;
			}
		}

		if (deleteAccess)
		{
			return !_fileSystem.Execute.IsWindows || Share == FileShare.Delete;
		}

		return CheckAccessWithShare(access, currentShare) &&
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

	private sealed class IgnoreFileHandle : IStorageAccessHandle
	{
		#region IStorageAccessHandle Members

		/// <inheritdoc cref="IStorageAccessHandle.Access" />
		public FileAccess Access
			=> FileAccess.ReadWrite;

		/// <inheritdoc cref="IStorageAccessHandle.DeleteAccess" />
		public bool DeleteAccess
			=> false;

		/// <inheritdoc cref="IStorageAccessHandle.Share" />
		public FileShare Share
			=> FileShare.ReadWrite;

		/// <inheritdoc cref="IDisposable.Dispose()" />
		public void Dispose()
		{
			// Do nothing
		}

		#endregion
	}
}
