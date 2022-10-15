using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Internal;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.Tests.TestHelpers;

/// <summary>
///     A <see cref="IStorageContainer" /> for testing purposes.
///     <para />
///     Set <see cref="IsLocked" /> to <see langword="true" /> to simulate a locked file
///     (<see cref="RequestAccess(FileAccess, FileShare, bool)" /> throws an <see cref="IOException" />).
/// </summary>
internal class LockableContainer : IStorageContainer
{
	/// <summary>
	///     Simulate a locked file, if set to <see langword="true" />.<br />
	///     In this case <see cref="RequestAccess(FileAccess, FileShare, bool)" /> throws an <see cref="IOException" />,
	///     otherwise it will succeed.
	/// </summary>
	public bool IsLocked { get; set; }

	private byte[] _bytes = Array.Empty<byte>();

	public LockableContainer(Testing.FileSystemMock fileSystem,
	                         FileSystemTypes containerType =
		                         FileSystemTypes.DirectoryOrFile)
	{
		FileSystem = fileSystem;
		TimeSystem = fileSystem.TimeSystem;
		Type = containerType;
	}

	#region IStorageContainer Members

	/// <inheritdoc cref="IStorageContainer.Attributes" />
	public FileAttributes Attributes { get; set; }

	/// <inheritdoc cref="IStorageContainer.CreationTime" />
	public IStorageContainer.ITimeContainer CreationTime { get; }
		= new InMemoryContainer.TimeContainer();

	/// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
	public IFileSystem FileSystem { get; }

	/// <inheritdoc cref="IStorageContainer.LastAccessTime" />
	public IStorageContainer.ITimeContainer LastAccessTime { get; }
		= new InMemoryContainer.TimeContainer();

	/// <inheritdoc cref="IStorageContainer.LastWriteTime" />
	public IStorageContainer.ITimeContainer LastWriteTime { get; }
		= new InMemoryContainer.TimeContainer();

	/// <inheritdoc cref="IStorageContainer.LinkTarget" />
	public string? LinkTarget { get; set; }

	/// <inheritdoc cref="ITimeSystem.ITimeSystemExtensionPoint.TimeSystem" />
	public ITimeSystem TimeSystem { get; }

	/// <inheritdoc cref="IStorageContainer.Type" />
	public FileSystemTypes Type { get; }

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

	/// <inheritdoc cref="IStorageContainer.RequestAccess(FileAccess, FileShare, bool)" />
	public IStorageAccessHandle RequestAccess(FileAccess access, FileShare share,
	                                          bool ignoreMetadataError = true)
	{
		if (IsLocked)
		{
			throw ExceptionFactory.ProcessCannotAccessTheFile("");
		}

		return new AccessHandle(access, share);
	}

	/// <inheritdoc cref="IStorageContainer.WriteBytes(byte[])" />
	public void WriteBytes(byte[] bytes)
		=> _bytes = bytes;

	#endregion

	private class AccessHandle : IStorageAccessHandle
	{
		public AccessHandle(FileAccess access, FileShare share)
		{
			Access = access;
			Share = share;
		}

		#region IStorageAccessHandle Members

		/// <inheritdoc cref="IStorageAccessHandle.Access" />
		public FileAccess Access { get; }

		/// <inheritdoc cref="IStorageAccessHandle.Access" />
		public FileShare Share { get; }

		/// <inheritdoc cref="IDisposable.Dispose()" />
		public void Dispose()
		{
		}

		#endregion
	}
}