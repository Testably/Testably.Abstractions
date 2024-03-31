using System.IO;
using System.Linq;
using System.Security.AccessControl;
using Testably.Abstractions.Helpers;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Storage;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.Tests.TestHelpers;

/// <summary>
///     A <see cref="IStorageContainer" /> for testing purposes.
///     <para />
///     Set <see cref="IsLocked" /> to <see langword="true" /> to simulate a locked file
///     (<see cref="RequestAccess(FileAccess, FileShare, bool, bool, int?)" /> throws an <see cref="IOException" />).
/// </summary>
internal sealed class LockableContainer(
	MockFileSystem fileSystem,
	FileSystemTypes containerType = FileSystemTypes.DirectoryOrFile)
	: IStorageContainer
{
	/// <inheritdoc cref="FileSystemSecurity" />
	public FileSystemSecurity? AccessControl { get; set; }

	/// <summary>
	///     Simulate a locked file, if set to <see langword="true" />.<br />
	///     In this case <see cref="RequestAccess(FileAccess, FileShare, bool, bool, int?)" /> throws
	///     an <see cref="IOException" />, otherwise it will succeed.
	/// </summary>
	public bool IsLocked { get; set; }

	private byte[] _bytes = Array.Empty<byte>();

	#region IStorageContainer Members

	/// <inheritdoc cref="IStorageContainer.Attributes" />
	public FileAttributes Attributes { get; set; }

	/// <inheritdoc cref="IStorageContainer.CreationTime" />
	public IStorageContainer.ITimeContainer CreationTime { get; }
		= new InMemoryContainer.TimeContainer();

	/// <inheritdoc cref="IStorageContainer.Extensibility" />
	public IFileSystemExtensibility Extensibility { get; }
		= new FileSystemExtensibility();

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem { get; } = fileSystem;

	/// <inheritdoc cref="IStorageContainer.LastAccessTime" />
	public IStorageContainer.ITimeContainer LastAccessTime { get; }
		= new InMemoryContainer.TimeContainer();

	/// <inheritdoc cref="IStorageContainer.LastWriteTime" />
	public IStorageContainer.ITimeContainer LastWriteTime { get; }
		= new InMemoryContainer.TimeContainer();

	/// <inheritdoc cref="IStorageContainer.LinkTarget" />
	public string? LinkTarget { get; set; }

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem { get; } = fileSystem.TimeSystem;

	/// <inheritdoc cref="IStorageContainer.Type" />
	public FileSystemTypes Type { get; } = containerType;

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	/// <inheritdoc cref="IStorageContainer.UnixFileMode" />
	public UnixFileMode UnixFileMode { get; set; }
#endif

	/// <inheritdoc cref="IStorageContainer.AppendBytes(byte[])" />
	public void AppendBytes(byte[] bytes)
		=> WriteBytes(_bytes.Concat(bytes).ToArray());

	/// <inheritdoc cref="IStorageContainer.ClearBytes()" />
	public void ClearBytes()
		=> _bytes = Array.Empty<byte>();

	/// <inheritdoc cref="IStorageContainer.Decrypt()" />
	public void Decrypt()
		=> throw new NotSupportedException();

	/// <inheritdoc cref="IStorageContainer.Encrypt()" />
	public void Encrypt()
		=> throw new NotSupportedException();

	/// <inheritdoc cref="IStorageContainer.GetBytes()" />
	public byte[] GetBytes()
		=> _bytes;

	/// <inheritdoc cref="IStorageContainer.RequestAccess(FileAccess, FileShare, bool, bool, int?)" />
	public IStorageAccessHandle RequestAccess(FileAccess access, FileShare share,
		bool deleteAccess = false,
		bool ignoreMetadataErrors = true,
		int? hResult = null)
	{
		if (IsLocked)
		{
			throw ExceptionFactory.ProcessCannotAccessTheFile("", hResult ?? -1);
		}

		return new AccessHandle(access, share, deleteAccess);
	}

	/// <inheritdoc cref="IStorageContainer.WriteBytes(byte[])" />
	public void WriteBytes(byte[] bytes)
		=> _bytes = bytes;

	#endregion

	private sealed class AccessHandle(FileAccess access, FileShare share, bool deleteAccess)
		: IStorageAccessHandle
	{
		#region IStorageAccessHandle Members

		/// <inheritdoc cref="IStorageAccessHandle.Access" />
		public FileAccess Access { get; } = access;

		/// <inheritdoc cref="IStorageAccessHandle.DeleteAccess" />
		public bool DeleteAccess { get; } = deleteAccess;

		/// <inheritdoc cref="IStorageAccessHandle.Access" />
		public FileShare Share { get; } = share;

		/// <inheritdoc cref="IDisposable.Dispose()" />
		public void Dispose()
		{
			// Nothing to do
		}

		#endregion
	}
}
