using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions.FileSystem;

internal sealed class DirectoryWrapper : IDirectory
{
	internal DirectoryWrapper(RealFileSystem fileSystem)
	{
		FileSystem = fileSystem;
	}

	#region IDirectory Members

	/// <inheritdoc cref="IFileSystemExtensionPoint.FileSystem" />
	public IFileSystem FileSystem { get; }

	/// <inheritdoc cref="IDirectory.CreateDirectory(string)" />
	public IDirectoryInfo CreateDirectory(string path)
		=> DirectoryInfoWrapper.FromDirectoryInfo(
			Directory.CreateDirectory(path), FileSystem);

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	/// <inheritdoc cref="IDirectory.CreateDirectory(string, UnixFileMode)" />
	public IDirectoryInfo CreateDirectory(string path, UnixFileMode unixCreateMode)
		=> DirectoryInfoWrapper.FromDirectoryInfo(
			#pragma warning disable CA1416
			Directory.CreateDirectory(path, unixCreateMode), FileSystem);
	#pragma warning restore CA1416
#endif

#if FEATURE_FILESYSTEM_LINK
	/// <inheritdoc cref="IDirectory.CreateSymbolicLink(string, string)" />
	public IFileSystemInfo CreateSymbolicLink(
		string path, string pathToTarget)
		=> FileSystemInfoWrapper.FromFileSystemInfo(
			Directory.CreateSymbolicLink(path, pathToTarget), FileSystem);
#endif

#if FEATURE_FILESYSTEM_NET7
	/// <inheritdoc cref="IDirectory.CreateTempSubdirectory(string)" />
	public IDirectoryInfo CreateTempSubdirectory(string? prefix = null)
		=> DirectoryInfoWrapper.FromDirectoryInfo(
			Directory.CreateTempSubdirectory(prefix), FileSystem);
#endif

	/// <inheritdoc cref="IDirectory.Delete(string)" />
	public void Delete(string path)
		=> Directory.Delete(path);

	/// <inheritdoc cref="IDirectory.Delete(string, bool)" />
	public void Delete(string path, bool recursive)
		=> Directory.Delete(path, recursive);

	/// <inheritdoc cref="IDirectory.EnumerateDirectories(string)" />
	public IEnumerable<string> EnumerateDirectories(string path)
		=> Directory.EnumerateDirectories(path);

	/// <inheritdoc cref="IDirectory.EnumerateDirectories(string, string)" />
	public IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
		=> Directory.EnumerateDirectories(path, searchPattern);

	/// <inheritdoc cref="IDirectory.EnumerateDirectories(string, string, SearchOption)" />
	public IEnumerable<string> EnumerateDirectories(string path,
		string searchPattern,
		SearchOption searchOption)
		=> Directory.EnumerateDirectories(
			path,
			searchPattern,
			searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectory.EnumerateDirectories(string, string, EnumerationOptions)" />
	public IEnumerable<string> EnumerateDirectories(string path,
		string searchPattern,
		EnumerationOptions
			enumerationOptions)
		=> Directory.EnumerateDirectories(path,
			searchPattern,
			enumerationOptions);
#endif

	/// <inheritdoc cref="IDirectory.EnumerateFiles(string)" />
	public IEnumerable<string> EnumerateFiles(string path)
		=> Directory.EnumerateFiles(path);

	/// <inheritdoc cref="IDirectory.EnumerateFiles(string, string)" />
	public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
		=> Directory.EnumerateFiles(path, searchPattern);

	/// <inheritdoc cref="IDirectory.EnumerateFiles(string, string, SearchOption)" />
	public IEnumerable<string> EnumerateFiles(string path,
		string searchPattern,
		SearchOption searchOption)
		=> Directory.EnumerateFiles(path, searchPattern, searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectory.EnumerateFiles(string, string, EnumerationOptions)" />
	public IEnumerable<string> EnumerateFiles(string path,
		string searchPattern,
		EnumerationOptions enumerationOptions)
		=> Directory.EnumerateFiles(path, searchPattern,
			enumerationOptions);
#endif

	/// <inheritdoc cref="IDirectory.EnumerateFileSystemEntries(string)" />
	public IEnumerable<string> EnumerateFileSystemEntries(string path)
		=> Directory.EnumerateFileSystemEntries(path);

	/// <inheritdoc cref="IDirectory.EnumerateFileSystemEntries(string, string)" />
	public IEnumerable<string> EnumerateFileSystemEntries(
		string path, string searchPattern)
		=> Directory.EnumerateFileSystemEntries(path, searchPattern);

	/// <inheritdoc cref="IDirectory.EnumerateFileSystemEntries(string, string, SearchOption)" />
	public IEnumerable<string> EnumerateFileSystemEntries(string path,
		string searchPattern,
		SearchOption searchOption)
		=> Directory.EnumerateFileSystemEntries(path,
			searchPattern,
			searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectory.EnumerateFileSystemEntries(string, string, EnumerationOptions)" />
	public IEnumerable<string> EnumerateFileSystemEntries(string path,
		string searchPattern,
		EnumerationOptions
			enumerationOptions)
		=> Directory.EnumerateFileSystemEntries(path, searchPattern,
			enumerationOptions);
#endif

	/// <inheritdoc cref="IDirectory.Exists(string)" />
	public bool Exists([NotNullWhen(true)] string? path)
		=> Directory.Exists(path);

	/// <inheritdoc cref="IDirectory.GetCreationTime(string)" />
	public DateTime GetCreationTime(string path)
		=> Directory.GetCreationTime(path);

	/// <inheritdoc cref="IDirectory.GetCreationTimeUtc(string)" />
	public DateTime GetCreationTimeUtc(string path)
		=> Directory.GetCreationTimeUtc(path);

	/// <inheritdoc cref="IDirectory.GetCurrentDirectory()" />
	public string GetCurrentDirectory()
		=> Directory.GetCurrentDirectory();

	/// <inheritdoc cref="IDirectory.GetDirectories(string)" />
	public string[] GetDirectories(string path)
		=> Directory.GetDirectories(path);

	/// <inheritdoc cref="IDirectory.GetDirectories(string, string)" />
	public string[] GetDirectories(string path, string searchPattern)
		=> Directory.GetDirectories(path, searchPattern);

	/// <inheritdoc cref="IDirectory.GetDirectories(string, string, SearchOption)" />
	public string[] GetDirectories(string path,
		string searchPattern,
		SearchOption searchOption)
		=> Directory.GetDirectories(path, searchPattern, searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectory.GetDirectories(string, string, EnumerationOptions)" />
	public string[] GetDirectories(string path,
		string searchPattern,
		EnumerationOptions enumerationOptions)
		=> Directory.GetDirectories(path, searchPattern,
			enumerationOptions);
#endif

	/// <inheritdoc cref="IDirectory.GetDirectoryRoot(string)" />
	public string GetDirectoryRoot(string path)
		=> Directory.GetDirectoryRoot(path);

	/// <inheritdoc cref="IDirectory.GetFiles(string)" />
	public string[] GetFiles(string path)
		=> Directory.GetFiles(path);

	/// <inheritdoc cref="IDirectory.GetFiles(string, string)" />
	public string[] GetFiles(string path, string searchPattern)
		=> Directory.GetFiles(path, searchPattern);

	/// <inheritdoc cref="IDirectory.GetFiles(string, string, SearchOption)" />
	public string[] GetFiles(string path,
		string searchPattern,
		SearchOption searchOption)
		=> Directory.GetFiles(path, searchPattern, searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectory.GetFiles(string, string, EnumerationOptions)" />
	public string[] GetFiles(string path,
		string searchPattern,
		EnumerationOptions enumerationOptions)
		=> Directory.GetFiles(path, searchPattern, enumerationOptions);
#endif

	/// <inheritdoc cref="IDirectory.GetFileSystemEntries(string)" />
	public string[] GetFileSystemEntries(string path)
		=> Directory.GetFileSystemEntries(path);

	/// <inheritdoc cref="IDirectory.GetFileSystemEntries(string, string)" />
	public string[] GetFileSystemEntries(string path, string searchPattern)
		=> Directory.GetFileSystemEntries(path, searchPattern);

	/// <inheritdoc cref="IDirectory.GetFileSystemEntries(string, string, SearchOption)" />
	public string[] GetFileSystemEntries(string path,
		string searchPattern,
		SearchOption searchOption)
		=> Directory.GetFileSystemEntries(path,
			searchPattern,
			searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectory.GetFileSystemEntries(string, string, EnumerationOptions)" />
	public string[] GetFileSystemEntries(string path,
		string searchPattern,
		EnumerationOptions enumerationOptions)
		=> Directory.GetFileSystemEntries(path,
			searchPattern,
			enumerationOptions);
#endif

	/// <inheritdoc cref="IDirectory.GetLastAccessTime(string)" />
	public DateTime GetLastAccessTime(string path)
		=> Directory.GetLastAccessTime(path);

	/// <inheritdoc cref="IDirectory.GetLastAccessTimeUtc(string)" />
	public DateTime GetLastAccessTimeUtc(string path)
		=> Directory.GetLastAccessTimeUtc(path);

	/// <inheritdoc cref="IDirectory.GetLastWriteTime(string)" />
	public DateTime GetLastWriteTime(string path)
		=> Directory.GetLastWriteTime(path);

	/// <inheritdoc cref="IDirectory.GetLastWriteTimeUtc(string)" />
	public DateTime GetLastWriteTimeUtc(string path)
		=> Directory.GetLastWriteTimeUtc(path);

	/// <inheritdoc cref="IDirectory.GetLogicalDrives()" />
	public string[] GetLogicalDrives()
		=> Directory.GetLogicalDrives();

	/// <inheritdoc cref="IDirectory.GetParent(string)" />
	public IDirectoryInfo? GetParent(string path)
		=> DirectoryInfoWrapper.FromDirectoryInfo(
			Directory.GetParent(path),
			FileSystem);

	/// <inheritdoc cref="IDirectory.Move(string, string)" />
	public void Move(string sourceDirName, string destDirName)
		=> Directory.Move(sourceDirName, destDirName);

#if FEATURE_FILESYSTEM_LINK
	/// <inheritdoc cref="IDirectory.ResolveLinkTarget(string, bool)" />
	public IFileSystemInfo? ResolveLinkTarget(
		string linkPath, bool returnFinalTarget)
		=> FileSystemInfoWrapper.FromFileSystemInfo(
			Directory.ResolveLinkTarget(linkPath, returnFinalTarget),
			FileSystem);
#endif

	/// <inheritdoc cref="IDirectory.SetCreationTime(string, DateTime)" />
	public void SetCreationTime(string path, DateTime creationTime)
		=> Directory.SetCreationTime(path, creationTime);

	/// <inheritdoc cref="IDirectory.SetCreationTimeUtc(string, DateTime)" />
	public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
		=> Directory.SetCreationTimeUtc(path, creationTimeUtc);

	/// <inheritdoc cref="IDirectory.SetCurrentDirectory(string)" />
	public void SetCurrentDirectory(string path)
		=> Directory.SetCurrentDirectory(path);

	/// <inheritdoc cref="IDirectory.SetLastAccessTime(string, DateTime)" />
	public void SetLastAccessTime(string path, DateTime lastAccessTime)
		=> Directory.SetLastAccessTime(path, lastAccessTime);

	/// <inheritdoc cref="IDirectory.SetLastAccessTimeUtc(string, DateTime)" />
	public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
		=> Directory.SetLastAccessTimeUtc(path, lastAccessTimeUtc);

	/// <inheritdoc cref="IDirectory.SetLastWriteTime(string, DateTime)" />
	public void SetLastWriteTime(string path, DateTime lastWriteTime)
		=> Directory.SetLastWriteTime(path, lastWriteTime);

	/// <inheritdoc cref="IDirectory.SetLastWriteTimeUtc(string, DateTime)" />
	public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
		=> Directory.SetLastWriteTimeUtc(path, lastWriteTimeUtc);

	#endregion
}
