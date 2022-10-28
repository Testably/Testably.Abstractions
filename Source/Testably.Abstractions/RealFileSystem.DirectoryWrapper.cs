using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions;

public sealed partial class RealFileSystem
{
	private sealed class DirectoryWrapper : IDirectory
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
				System.IO.Directory.CreateDirectory(path), FileSystem);

#if FEATURE_FILESYSTEM_LINK
		/// <inheritdoc cref="IDirectory.CreateSymbolicLink(string, string)" />
		public IFileSystemInfo CreateSymbolicLink(
			string path, string pathToTarget)
			=> FileSystemInfoWrapper.FromFileSystemInfo(
				System.IO.Directory.CreateSymbolicLink(path, pathToTarget), FileSystem);
#endif

		/// <inheritdoc cref="IDirectory.Delete(string)" />
		public void Delete(string path)
			=> System.IO.Directory.Delete(path);

		/// <inheritdoc cref="IDirectory.Delete(string, bool)" />
		public void Delete(string path, bool recursive)
			=> System.IO.Directory.Delete(path, recursive);

		/// <inheritdoc cref="IDirectory.EnumerateDirectories(string)" />
		public IEnumerable<string> EnumerateDirectories(string path)
			=> System.IO.Directory.EnumerateDirectories(path);

		/// <inheritdoc cref="IDirectory.EnumerateDirectories(string, string)" />
		public IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
			=> System.IO.Directory.EnumerateDirectories(path, searchPattern);

		/// <inheritdoc cref="IDirectory.EnumerateDirectories(string, string, System.IO.SearchOption)" />
		public IEnumerable<string> EnumerateDirectories(string path,
		                                                string searchPattern,
		                                                SearchOption searchOption)
			=> System.IO.Directory.EnumerateDirectories(
				path,
				searchPattern,
				searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="IDirectory.EnumerateDirectories(string, string, System.IO.EnumerationOptions)" />
		public IEnumerable<string> EnumerateDirectories(string path,
		                                                string searchPattern,
		                                                EnumerationOptions
			                                                enumerationOptions)
			=> System.IO.Directory.EnumerateDirectories(path,
				searchPattern,
				enumerationOptions);
#endif

		/// <inheritdoc cref="IDirectory.EnumerateFiles(string)" />
		public IEnumerable<string> EnumerateFiles(string path)
			=> System.IO.Directory.EnumerateFiles(path);

		/// <inheritdoc cref="IDirectory.EnumerateFiles(string, string)" />
		public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
			=> System.IO.Directory.EnumerateFiles(path, searchPattern);

		/// <inheritdoc cref="IDirectory.EnumerateFiles(string, string, System.IO.SearchOption)" />
		public IEnumerable<string> EnumerateFiles(string path,
		                                          string searchPattern,
		                                          SearchOption searchOption)
			=> System.IO.Directory.EnumerateFiles(path, searchPattern, searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="IDirectory.EnumerateFiles(string, string, System.IO.EnumerationOptions)" />
		public IEnumerable<string> EnumerateFiles(string path,
		                                          string searchPattern,
		                                          EnumerationOptions enumerationOptions)
			=> System.IO.Directory.EnumerateFiles(path, searchPattern,
				enumerationOptions);
#endif

		/// <inheritdoc cref="IDirectory.EnumerateFileSystemEntries(string)" />
		public IEnumerable<string> EnumerateFileSystemEntries(string path)
			=> System.IO.Directory.EnumerateFileSystemEntries(path);

		/// <inheritdoc cref="IDirectory.EnumerateFileSystemEntries(string, string)" />
		public IEnumerable<string> EnumerateFileSystemEntries(
			string path, string searchPattern)
			=> System.IO.Directory.EnumerateFileSystemEntries(path, searchPattern);

		/// <inheritdoc cref="IDirectory.EnumerateFileSystemEntries(string, string, System.IO.SearchOption)" />
		public IEnumerable<string> EnumerateFileSystemEntries(string path,
			string searchPattern,
			SearchOption searchOption)
			=> System.IO.Directory.EnumerateFileSystemEntries(path,
				searchPattern,
				searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="IDirectory.EnumerateFileSystemEntries(string, string, System.IO.EnumerationOptions)" />
		public IEnumerable<string> EnumerateFileSystemEntries(string path,
			string searchPattern,
			EnumerationOptions
				enumerationOptions)
			=> System.IO.Directory.EnumerateFileSystemEntries(path, searchPattern,
				enumerationOptions);
#endif

		/// <inheritdoc cref="IDirectory.Exists(string)" />
		public bool Exists([NotNullWhen(true)] string? path)
			=> System.IO.Directory.Exists(path);

		/// <inheritdoc cref="IDirectory.GetCreationTime(string)" />
		public DateTime GetCreationTime(string path)
			=> System.IO.Directory.GetCreationTime(path);

		/// <inheritdoc cref="IDirectory.GetCreationTimeUtc(string)" />
		public DateTime GetCreationTimeUtc(string path)
			=> System.IO.Directory.GetCreationTimeUtc(path);

		/// <inheritdoc cref="IDirectory.GetCurrentDirectory()" />
		public string GetCurrentDirectory()
			=> System.IO.Directory.GetCurrentDirectory();

		/// <inheritdoc cref="IDirectory.GetDirectories(string)" />
		public string[] GetDirectories(string path)
			=> System.IO.Directory.GetDirectories(path);

		/// <inheritdoc cref="IDirectory.GetDirectories(string, string)" />
		public string[] GetDirectories(string path, string searchPattern)
			=> System.IO.Directory.GetDirectories(path, searchPattern);

		/// <inheritdoc cref="IDirectory.GetDirectories(string, string, System.IO.SearchOption)" />
		public string[] GetDirectories(string path,
		                               string searchPattern,
		                               SearchOption searchOption)
			=> System.IO.Directory.GetDirectories(path, searchPattern, searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="IDirectory.GetDirectories(string, string, System.IO.EnumerationOptions)" />
		public string[] GetDirectories(string path,
		                               string searchPattern,
		                               EnumerationOptions enumerationOptions)
			=> System.IO.Directory.GetDirectories(path, searchPattern,
				enumerationOptions);
#endif

		/// <inheritdoc cref="IDirectory.GetDirectoryRoot(string)" />
		public string GetDirectoryRoot(string path)
			=> System.IO.Directory.GetDirectoryRoot(path);

		/// <inheritdoc cref="IDirectory.GetFiles(string)" />
		public string[] GetFiles(string path)
			=> System.IO.Directory.GetFiles(path);

		/// <inheritdoc cref="IDirectory.GetFiles(string, string)" />
		public string[] GetFiles(string path, string searchPattern)
			=> System.IO.Directory.GetFiles(path, searchPattern);

		/// <inheritdoc cref="IDirectory.GetFiles(string, string, System.IO.SearchOption)" />
		public string[] GetFiles(string path,
		                         string searchPattern,
		                         SearchOption searchOption)
			=> System.IO.Directory.GetFiles(path, searchPattern, searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="IDirectory.GetFiles(string, string, System.IO.EnumerationOptions)" />
		public string[] GetFiles(string path,
		                         string searchPattern,
		                         EnumerationOptions enumerationOptions)
			=> System.IO.Directory.GetFiles(path, searchPattern, enumerationOptions);
#endif

		/// <inheritdoc cref="IDirectory.GetFileSystemEntries(string)" />
		public string[] GetFileSystemEntries(string path)
			=> System.IO.Directory.GetFileSystemEntries(path);

		/// <inheritdoc cref="IDirectory.GetFileSystemEntries(string, string)" />
		public string[] GetFileSystemEntries(string path, string searchPattern)
			=> System.IO.Directory.GetFileSystemEntries(path, searchPattern);

		/// <inheritdoc cref="IDirectory.GetFileSystemEntries(string, string, System.IO.SearchOption)" />
		public string[] GetFileSystemEntries(string path,
		                                     string searchPattern,
		                                     SearchOption searchOption)
			=> System.IO.Directory.GetFileSystemEntries(path,
				searchPattern,
				searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="IDirectory.GetFileSystemEntries(string, string, System.IO.EnumerationOptions)" />
		public string[] GetFileSystemEntries(string path,
		                                     string searchPattern,
		                                     EnumerationOptions enumerationOptions)
			=> System.IO.Directory.GetFileSystemEntries(path,
				searchPattern,
				enumerationOptions);
#endif

		/// <inheritdoc cref="IDirectory.GetLastAccessTime(string)" />
		public DateTime GetLastAccessTime(string path)
			=> System.IO.Directory.GetLastAccessTime(path);

		/// <inheritdoc cref="IDirectory.GetLastAccessTimeUtc(string)" />
		public DateTime GetLastAccessTimeUtc(string path)
			=> System.IO.Directory.GetLastAccessTimeUtc(path);

		/// <inheritdoc cref="IDirectory.GetLastWriteTime(string)" />
		public DateTime GetLastWriteTime(string path)
			=> System.IO.Directory.GetLastWriteTime(path);

		/// <inheritdoc cref="IDirectory.GetLastWriteTimeUtc(string)" />
		public DateTime GetLastWriteTimeUtc(string path)
			=> System.IO.Directory.GetLastWriteTimeUtc(path);

		/// <inheritdoc cref="IDirectory.GetLogicalDrives()" />
		public string[] GetLogicalDrives()
			=> System.IO.Directory.GetLogicalDrives();

		/// <inheritdoc cref="IDirectory.GetParent(string)" />
		public IDirectoryInfo? GetParent(string path)
			=> DirectoryInfoWrapper.FromDirectoryInfo(
				System.IO.Directory.GetParent(path),
				FileSystem);

		/// <inheritdoc cref="IDirectory.Move(string, string)" />
		public void Move(string sourceDirName, string destDirName)
			=> System.IO.Directory.Move(sourceDirName, destDirName);

#if FEATURE_FILESYSTEM_LINK
		/// <inheritdoc cref="IDirectory.ResolveLinkTarget(string, bool)" />
		public IFileSystemInfo? ResolveLinkTarget(
			string linkPath, bool returnFinalTarget)
			=> FileSystemInfoWrapper.FromFileSystemInfo(
				System.IO.Directory.ResolveLinkTarget(linkPath, returnFinalTarget),
				FileSystem);
#endif

		/// <inheritdoc cref="IDirectory.SetCreationTime(string, DateTime)" />
		public void SetCreationTime(string path, DateTime creationTime)
			=> System.IO.Directory.SetCreationTime(path, creationTime);

		/// <inheritdoc cref="IDirectory.SetCreationTimeUtc(string, DateTime)" />
		public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
			=> System.IO.Directory.SetCreationTimeUtc(path, creationTimeUtc);

		/// <inheritdoc cref="IDirectory.SetCurrentDirectory(string)" />
		public void SetCurrentDirectory(string path)
			=> System.IO.Directory.SetCurrentDirectory(path);

		/// <inheritdoc cref="IDirectory.SetLastAccessTime(string, DateTime)" />
		public void SetLastAccessTime(string path, DateTime lastAccessTime)
			=> System.IO.Directory.SetLastAccessTime(path, lastAccessTime);

		/// <inheritdoc cref="IDirectory.SetLastAccessTimeUtc(string, DateTime)" />
		public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
			=> System.IO.Directory.SetLastAccessTimeUtc(path, lastAccessTimeUtc);

		/// <inheritdoc cref="IDirectory.SetLastWriteTime(string, DateTime)" />
		public void SetLastWriteTime(string path, DateTime lastWriteTime)
			=> System.IO.Directory.SetLastWriteTime(path, lastWriteTime);

		/// <inheritdoc cref="IDirectory.SetLastWriteTimeUtc(string, DateTime)" />
		public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
			=> System.IO.Directory.SetLastWriteTimeUtc(path, lastWriteTimeUtc);

		#endregion
	}
}