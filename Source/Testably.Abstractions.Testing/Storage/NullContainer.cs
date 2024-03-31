using System;
using System.IO;
using Testably.Abstractions.Helpers;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.Storage;

internal sealed class NullContainer : IStorageContainer
{
	private readonly MockFileSystem _fileSystem;

	private NullContainer(MockFileSystem fileSystem, ITimeSystem timeSystem)
	{
		_fileSystem = fileSystem;
		TimeSystem = timeSystem;
		Extensibility = new FileSystemExtensibility();
		CreationTime = new CreationNullTime(_fileSystem);
		LastAccessTime = new NullTime(_fileSystem);
		LastWriteTime = new NullTime(_fileSystem);
	}

	#region IStorageContainer Members

	/// <inheritdoc cref="IStorageContainer.Attributes" />
	public FileAttributes Attributes
	{
		get => (FileAttributes)(-1);
		set => throw ExceptionFactory.FileNotFound(string.Empty);
	}

	/// <inheritdoc cref="IStorageContainer.CreationTime" />
	public IStorageContainer.ITimeContainer CreationTime { get; }

	/// <inheritdoc cref="IStorageContainer.Extensibility" />
	public IFileSystemExtensibility Extensibility { get; }

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem => _fileSystem;

	/// <inheritdoc cref="IStorageContainer.LastAccessTime" />
	public IStorageContainer.ITimeContainer LastAccessTime { get; }

	/// <inheritdoc cref="IStorageContainer.LastWriteTime" />
	public IStorageContainer.ITimeContainer LastWriteTime { get; }

	/// <inheritdoc cref="IStorageContainer.LinkTarget" />
	public string? LinkTarget
	{
		get => null;
		set => _ = value;
	}

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem { get; }

	/// <inheritdoc cref="IStorageContainer.Type" />
	public FileSystemTypes Type
		=> FileSystemTypes.DirectoryOrFile;

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	/// <inheritdoc cref="IStorageContainer.UnixFileMode" />
	public UnixFileMode UnixFileMode { get; set; } = (UnixFileMode)(-1);
#endif

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

	/// <inheritdoc cref="IStorageContainer.RequestAccess(FileAccess, FileShare, bool, bool, int?)" />
	public IStorageAccessHandle RequestAccess(FileAccess access, FileShare share,
		bool deleteAccess = false,
		bool ignoreMetadataErrors = true,
		int? hResult = null)
		=> new NullStorageAccessHandle(access, share, deleteAccess);

	/// <inheritdoc cref="IStorageContainer.WriteBytes(byte[])" />
	public void WriteBytes(byte[] bytes)
	{
		// Do nothing in NullContainer
	}

	#endregion

	internal static IStorageContainer New(MockFileSystem fileSystem)
		=> new NullContainer(fileSystem, fileSystem.TimeSystem);

	private sealed class NullStorageAccessHandle : IStorageAccessHandle
	{
		public NullStorageAccessHandle(FileAccess access, FileShare share,
			bool deleteAccess)
		{
			Access = access;
			Share = share;
			DeleteAccess = deleteAccess;
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
			// Nothing to do!
		}

		#endregion
	}

	/// <summary>
	///     The default time returned by the file system if no time has been set.
	///     <seealso href="https://learn.microsoft.com/en-us/windows/win32/sysinfo/file-times" />:
	///     A file time is a 64-bit value that represents the number of 100-nanosecond intervals that have elapsed
	///     since 12:00 A.M. January 1, 1601, Coordinated Universal Time (UTC).
	/// </summary>
	private class NullTime : IStorageContainer.ITimeContainer
	{
		protected readonly MockFileSystem FileSystem;

		private readonly DateTime _time =
			new(1601, 01, 01, 00, 00, 00, DateTimeKind.Utc);

		public NullTime(MockFileSystem fileSystem)
		{
			FileSystem = fileSystem;
		}

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
		public virtual void Set(DateTime time, DateTimeKind kind)
		{
#if NET7_0_OR_GREATER
			throw ExceptionFactory.FileNotFound(string.Empty);
#else
			FileSystem.Execute.OnWindows(()
				=> throw ExceptionFactory.FileNotFound(string.Empty));

			throw ExceptionFactory.DirectoryNotFound(string.Empty);
#endif
		}

		#endregion
	}

	/// <summary>
	///     Overrides the setter of <see cref="NullTime" /> as different exceptions are thrown for MacOS starting with .NET 7.
	/// </summary>
	private sealed class CreationNullTime : NullTime
	{
		public CreationNullTime(MockFileSystem fileSystem) : base(fileSystem) { }

		/// <inheritdoc cref="NullTime.Set(DateTime, DateTimeKind)" />
		public override void Set(DateTime time, DateTimeKind kind)
		{
#if NET7_0_OR_GREATER
			FileSystem.Execute.OnMac(()
				=> throw ExceptionFactory.DirectoryNotFound(string.Empty));

			throw ExceptionFactory.FileNotFound(string.Empty);
#else
			FileSystem.Execute.OnWindows(()
				=> throw ExceptionFactory.FileNotFound(string.Empty));

			throw ExceptionFactory.DirectoryNotFound(string.Empty);
#endif
		}
	}
}
