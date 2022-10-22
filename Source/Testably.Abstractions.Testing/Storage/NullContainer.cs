using System;
using System.IO;
using System.Security.AccessControl;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing.Storage;

internal sealed class NullContainer : IStorageContainer
{
	private NullContainer(IFileSystem fileSystem, ITimeSystem timeSystem)
	{
		FileSystem = fileSystem;
		TimeSystem = timeSystem;
	}

	#region IStorageContainer Members

	/// <inheritdoc cref="IStorageContainer.Attributes" />
	public FileAttributes Attributes
	{
		get => (FileAttributes)(-1);
		set => throw ExceptionFactory.FileNotFound(string.Empty);
	}

	/// <inheritdoc cref="IStorageContainer.CreationTime" />
	public IStorageContainer.ITimeContainer CreationTime { get; } = new NullTime();

	/// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
	public IFileSystem FileSystem { get; }

	/// <inheritdoc cref="IStorageContainer.LastAccessTime" />
	public IStorageContainer.ITimeContainer LastAccessTime { get; } = new NullTime();

	/// <inheritdoc cref="IStorageContainer.LastWriteTime" />
	public IStorageContainer.ITimeContainer LastWriteTime { get; } = new NullTime();

	/// <inheritdoc cref="IStorageContainer.LinkTarget" />
	public string? LinkTarget
	{
		get => null;
		set => _ = value;
	}

	/// <inheritdoc cref="ITimeSystem.ITimeSystemExtensionPoint.TimeSystem" />
	public ITimeSystem TimeSystem { get; }

	/// <inheritdoc cref="IStorageContainer.Type" />
	public FileSystemTypes Type
		=> FileSystemTypes.DirectoryOrFile;

	/// <inheritdoc cref="IStorageContainer.AppendBytes(byte[])" />
	public void AppendBytes(byte[] bytes)
	{
		// Do nothing in NullContainer
	}

	/// <inheritdoc cref="IStorageContainer.ClearBytes()" />
	public void ClearBytes()
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

	/// <inheritdoc cref="IStorageContainer.RequestAccess(FileAccess, FileShare, bool)" />
	public IStorageAccessHandle RequestAccess(FileAccess access, FileShare share,
	                                          bool ignoreMetadataError = true)
		=> new NullStorageAccessHandle(access, share);

	/// <inheritdoc cref="IStorageContainer.WriteBytes(byte[])" />
	public void WriteBytes(byte[] bytes)
	{
		// Do nothing in NullContainer
	}

	#endregion

	internal static IStorageContainer New(FileSystemMock fileSystem)
		=> new NullContainer(fileSystem, fileSystem.TimeSystem);

	private sealed class NullStorageAccessHandle : IStorageAccessHandle
	{
		public NullStorageAccessHandle(FileAccess access, FileShare share)
		{
			Access = access;
			Share = share;
		}

		#region IStorageAccessHandle Members

		/// <inheritdoc cref="IStorageAccessHandle.Access" />
		public FileAccess Access { get; }

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
	///     since 12:00 A.M. January 1, 1601 Coordinated Universal Time (UTC).
	/// </summary>
	private sealed class NullTime : IStorageContainer.ITimeContainer
	{
		private readonly DateTime _time =
			new(1601, 01, 01, 00, 00, 00, DateTimeKind.Utc);

		#region ITimeContainer Members

		/// <inheritdoc cref="IStorageContainer.ITimeContainer.Get(DateTimeKind)" />
		public DateTime Get(DateTimeKind kind)
			=> kind switch
			{
				DateTimeKind.Utc => _time.ToUniversalTime(),
				DateTimeKind.Local => _time.ToLocalTime(),
				_ => _time
			};

		/// <inheritdoc cref="IStorageContainer.ITimeContainer.Set(DateTime, DateTimeKind)" />
		public void Set(DateTime time, DateTimeKind kind)
		{
			Execute.OnWindows(()
				=> throw ExceptionFactory.FileNotFound(string.Empty));

			throw ExceptionFactory.DirectoryNotFound(string.Empty);
		}

		#endregion
	}
}