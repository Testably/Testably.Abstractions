using System;
using System.IO;

namespace Testably.Abstractions.FileSystem;

/// <summary>
///     Abstractions for <see cref="FileSystemInfo" />.
/// </summary>
public interface IFileSystemInfo
{
	/// <inheritdoc cref="FileSystemInfo.Attributes" />
	FileAttributes Attributes { get; set; }

	/// <inheritdoc cref="FileSystemInfo.CreationTime" />
	DateTime CreationTime { get; set; }

	/// <inheritdoc cref="FileSystemInfo.CreationTimeUtc" />
	DateTime CreationTimeUtc { get; set; }

	/// <inheritdoc cref="FileSystemInfo.Exists" />
	bool Exists { get; }

	/// <inheritdoc cref="FileSystemInfo.Extension" />
	string Extension { get; }

	/// <summary>
	///     A container to support extensions on <see cref="IFileSystemInfo" />.
	/// </summary>
	IFileSystemExtensionContainer ExtensionContainer { get; }

	/// <inheritdoc cref="FileSystemInfo.FullName" />
	string FullName { get; }

	/// <inheritdoc cref="FileSystemInfo.LastAccessTime" />
	DateTime LastAccessTime { get; set; }

	/// <inheritdoc cref="FileSystemInfo.LastAccessTimeUtc" />
	DateTime LastAccessTimeUtc { get; set; }

	/// <inheritdoc cref="FileSystemInfo.LastWriteTime" />
	DateTime LastWriteTime { get; set; }

	/// <inheritdoc cref="FileSystemInfo.LastWriteTimeUtc" />
	DateTime LastWriteTimeUtc { get; set; }

#if FEATURE_FILESYSTEM_LINK
	/// <inheritdoc cref="FileSystemInfo.LinkTarget" />
	string? LinkTarget { get; }
#endif

	/// <inheritdoc cref="FileSystemInfo.Name" />
	string Name { get; }

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	/// <inheritdoc cref="FileSystemInfo.UnixFileMode" />
	UnixFileMode UnixFileMode
	{
		get;
		[UnsupportedOSPlatform("windows")] set;
	}
#endif

#if FEATURE_FILESYSTEM_LINK
	/// <inheritdoc cref="FileSystemInfo.CreateAsSymbolicLink(string)" />
	void CreateAsSymbolicLink(string pathToTarget);
#endif

	/// <inheritdoc cref="FileSystemInfo.Delete()" />
	void Delete();

	/// <inheritdoc cref="FileSystemInfo.Refresh()" />
	void Refresh();

#if FEATURE_FILESYSTEM_LINK
	/// <inheritdoc cref="FileSystemInfo.ResolveLinkTarget(bool)" />
	IFileSystemInfo? ResolveLinkTarget(bool returnFinalTarget);
#endif
}
