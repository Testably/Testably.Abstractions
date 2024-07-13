using System;
using System.IO;
using Testably.Abstractions.Helpers;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.Storage;

/// <summary>
///     A container for a stored file or directory in the <see cref="IStorage" />.
/// </summary>
internal interface IStorageContainer : IFileSystemEntity, ITimeSystemEntity
{
	/// <inheritdoc cref="System.IO.FileSystemInfo.Attributes" />
	FileAttributes Attributes { get; set; }

	/// <inheritdoc cref="System.IO.FileSystemInfo.CreationTime" />
	ITimeContainer CreationTime { get; }

	/// <summary>
	///     A container to support extensions on <see cref="IStorageContainer" />.
	/// </summary>
	IFileSystemExtensibility Extensibility { get; }

	/// <inheritdoc cref="System.IO.FileSystemInfo.LastAccessTime" />
	ITimeContainer LastAccessTime { get; }

	/// <inheritdoc cref="System.IO.FileSystemInfo.LastWriteTime" />
	ITimeContainer LastWriteTime { get; }

	/// <summary>
	///     If this instance represents a link, returns the link target's path, otherwise returns <see langword="null" />.
	/// </summary>
	string? LinkTarget { get; set; }

	/// <summary>
	///     The type of the container indicates if it is a <see cref="FileSystemTypes.File" /> or
	///     <see cref="FileSystemTypes.Directory" />.
	/// </summary>
	FileSystemTypes Type { get; }

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	/// <summary>
	///     Gets or sets the Unix file mode for the current file or directory.<br />
	///     See also: <seealso cref="FileSystemInfo.UnixFileMode" />
	/// </summary>
	UnixFileMode UnixFileMode { get; set; }
#endif

	/// <summary>
	///     An event triggered, when the underlying buffer is changed.
	/// </summary>
	event EventHandler? BytesChanged;

	/// <summary>
	///     Appends the <paramref name="bytes" /> to the <see cref="IFileInfo" />.
	/// </summary>
	void AppendBytes(byte[] bytes);

	/// <summary>
	///     Clears the content of the <see cref="IFileInfo" />.
	///     <para />
	///     This is used to delete the file.
	/// </summary>
	void ClearBytes();

	/// <summary>
	///     Decrypts the file content and removes the <see cref="FileAttributes.Encrypted" /> attribute.
	///     <para />
	///     Does nothing if the file is not encrypted.
	/// </summary>
	void Decrypt();

	/// <summary>
	///     Encrypts the file content and adds the <see cref="FileAttributes.Encrypted" /> attribute.
	///     <para />
	///     Does nothing if the file is already encrypted.
	/// </summary>
	void Encrypt();

	/// <summary>
	///     Gets the bytes in the <see cref="IFileInfo" />.
	/// </summary>
	byte[] GetBytes();

	/// <summary>
	///     Requests access to this file with the given <paramref name="share" />.
	/// </summary>
	/// <returns>An <see cref="IStorageAccessHandle" /> that is used to release the access lock on dispose.</returns>
	IStorageAccessHandle RequestAccess(FileAccess access, FileShare share,
		bool deleteAccess = false,
		bool ignoreFileShare = false,
		bool ignoreMetadataErrors = true,
		int? hResult = null);

	/// <summary>
	///     Writes the <paramref name="bytes" /> to the <see cref="IFileInfo" />.
	/// </summary>
	void WriteBytes(byte[] bytes);

	/// <summary>
	///     A container to allow reading/writing <see cref="DateTime" />s with consistent <see cref="DateTimeKind" />.
	/// </summary>
	public interface ITimeContainer
	{
		/// <summary>
		///     Get the <see cref="DateTime" /> in the given <paramref name="kind" />.
		/// </summary>
		DateTime Get(DateTimeKind kind);

		/// <summary>
		///     Set the <see cref="DateTime" /> to the <paramref name="time" /> in the given <paramref name="kind" />.
		/// </summary>
		void Set(DateTime time, DateTimeKind kind);
	}
}
