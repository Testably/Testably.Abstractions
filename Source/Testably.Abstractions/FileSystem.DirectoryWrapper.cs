using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions;

public sealed partial class FileSystem
{
	private sealed class DirectoryWrapper : IFileSystem.IDirectory
	{
		internal DirectoryWrapper(FileSystem fileSystem)
		{
			FileSystem = fileSystem;
		}

		#region IDirectory Members

		/// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
		public IFileSystem FileSystem { get; }

		/// <inheritdoc cref="IFileSystem.IDirectory.CreateDirectory(string)" />
		public IFileSystem.IDirectoryInfo CreateDirectory(string path)
			=> DirectoryInfoWrapper.FromDirectoryInfo(
				System.IO.Directory.CreateDirectory(path), FileSystem);

#if FEATURE_FILESYSTEM_LINK
		/// <inheritdoc cref="IFileSystem.IDirectory.CreateSymbolicLink(string, string)" />
		public IFileSystem.IFileSystemInfo CreateSymbolicLink(
			string path, string pathToTarget)
			=> FileSystemInfoWrapper.FromFileSystemInfo(
				System.IO.Directory.CreateSymbolicLink(path, pathToTarget), FileSystem);
#endif

		/// <inheritdoc cref="IFileSystem.IDirectory.Delete(string)" />
		public void Delete(string path)
			=> System.IO.Directory.Delete(path);

		/// <inheritdoc cref="IFileSystem.IDirectory.Delete(string, bool)" />
		public void Delete(string path, bool recursive)
			=> System.IO.Directory.Delete(path, recursive);

		/// <inheritdoc cref="IFileSystem.IDirectory.EnumerateDirectories(string)" />
		public IEnumerable<string> EnumerateDirectories(string path)
			=> System.IO.Directory.EnumerateDirectories(path);

		/// <inheritdoc cref="IFileSystem.IDirectory.EnumerateDirectories(string, string)" />
		public IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
			=> System.IO.Directory.EnumerateDirectories(path, searchPattern);

		/// <inheritdoc cref="IFileSystem.IDirectory.EnumerateDirectories(string, string, System.IO.SearchOption)" />
		public IEnumerable<string> EnumerateDirectories(string path,
		                                                string searchPattern,
		                                                SearchOption searchOption)
			=> System.IO.Directory.EnumerateDirectories(
				path,
				searchPattern,
				searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="IFileSystem.IDirectory.EnumerateDirectories(string, string, System.IO.EnumerationOptions)" />
		public IEnumerable<string> EnumerateDirectories(string path,
		                                                string searchPattern,
		                                                EnumerationOptions
			                                                enumerationOptions)
			=> System.IO.Directory.EnumerateDirectories(path,
				searchPattern,
				enumerationOptions);
#endif

		/// <inheritdoc cref="IFileSystem.IDirectory.EnumerateFiles(string)" />
		public IEnumerable<string> EnumerateFiles(string path)
			=> System.IO.Directory.EnumerateFiles(path);

		/// <inheritdoc cref="IFileSystem.IDirectory.EnumerateFiles(string, string)" />
		public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
			=> System.IO.Directory.EnumerateFiles(path, searchPattern);

		/// <inheritdoc cref="IFileSystem.IDirectory.EnumerateFiles(string, string, System.IO.SearchOption)" />
		public IEnumerable<string> EnumerateFiles(string path,
		                                          string searchPattern,
		                                          SearchOption searchOption)
			=> System.IO.Directory.EnumerateFiles(path, searchPattern, searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="IFileSystem.IDirectory.EnumerateFiles(string, string, System.IO.EnumerationOptions)" />
		public IEnumerable<string> EnumerateFiles(string path,
		                                          string searchPattern,
		                                          EnumerationOptions enumerationOptions)
			=> System.IO.Directory.EnumerateFiles(path, searchPattern,
				enumerationOptions);
#endif

		/// <inheritdoc cref="IFileSystem.IDirectory.EnumerateFileSystemEntries(string)" />
		public IEnumerable<string> EnumerateFileSystemEntries(string path)
			=> System.IO.Directory.EnumerateFileSystemEntries(path);

		/// <inheritdoc cref="IFileSystem.IDirectory.EnumerateFileSystemEntries(string, string)" />
		public IEnumerable<string> EnumerateFileSystemEntries(
			string path, string searchPattern)
			=> System.IO.Directory.EnumerateFileSystemEntries(path, searchPattern);

		/// <inheritdoc cref="IFileSystem.IDirectory.EnumerateFileSystemEntries(string, string, System.IO.SearchOption)" />
		public IEnumerable<string> EnumerateFileSystemEntries(string path,
			string searchPattern,
			SearchOption searchOption)
			=> System.IO.Directory.EnumerateFileSystemEntries(path,
				searchPattern,
				searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="IFileSystem.IDirectory.EnumerateFileSystemEntries(string, string, System.IO.EnumerationOptions)" />
		public IEnumerable<string> EnumerateFileSystemEntries(string path,
			string searchPattern,
			EnumerationOptions
				enumerationOptions)
			=> System.IO.Directory.EnumerateFileSystemEntries(path, searchPattern,
				enumerationOptions);
#endif

		/// <inheritdoc cref="IFileSystem.IDirectory.Exists(string)" />
		public bool Exists([NotNullWhen(true)] string? path)
			=> System.IO.Directory.Exists(path);

		/// <inheritdoc cref="IFileSystem.IDirectory.GetCreationTime(string)" />
		public DateTime GetCreationTime(string path)
			=> System.IO.Directory.GetCreationTime(path);

		/// <inheritdoc cref="IFileSystem.IDirectory.GetCreationTimeUtc(string)" />
		public DateTime GetCreationTimeUtc(string path)
			=> System.IO.Directory.GetCreationTimeUtc(path);

		/// <inheritdoc cref="IFileSystem.IDirectory.GetCurrentDirectory()" />
		public string GetCurrentDirectory()
			=> System.IO.Directory.GetCurrentDirectory();

		/// <inheritdoc cref="IFileSystem.IDirectory.GetDirectories(string)" />
		public string[] GetDirectories(string path)
			=> System.IO.Directory.GetDirectories(path);

		/// <inheritdoc cref="IFileSystem.IDirectory.GetDirectories(string, string)" />
		public string[] GetDirectories(string path, string searchPattern)
			=> System.IO.Directory.GetDirectories(path, searchPattern);

		/// <inheritdoc cref="IFileSystem.IDirectory.GetDirectories(string, string, System.IO.SearchOption)" />
		public string[] GetDirectories(string path,
		                               string searchPattern,
		                               SearchOption searchOption)
			=> System.IO.Directory.GetDirectories(path, searchPattern, searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="IFileSystem.IDirectory.GetDirectories(string, string, System.IO.EnumerationOptions)" />
		public string[] GetDirectories(string path,
		                               string searchPattern,
		                               EnumerationOptions enumerationOptions)
			=> System.IO.Directory.GetDirectories(path, searchPattern,
				enumerationOptions);
#endif

		/// <inheritdoc cref="IFileSystem.IDirectory.GetDirectoryRoot(string)" />
		public string GetDirectoryRoot(string path)
			=> System.IO.Directory.GetDirectoryRoot(path);

		/// <inheritdoc cref="IFileSystem.IDirectory.GetFiles(string)" />
		public string[] GetFiles(string path)
			=> System.IO.Directory.GetFiles(path);

		/// <inheritdoc cref="IFileSystem.IDirectory.GetFiles(string, string)" />
		public string[] GetFiles(string path, string searchPattern)
			=> System.IO.Directory.GetFiles(path, searchPattern);

		/// <inheritdoc cref="IFileSystem.IDirectory.GetFiles(string, string, System.IO.SearchOption)" />
		public string[] GetFiles(string path,
		                         string searchPattern,
		                         SearchOption searchOption)
			=> System.IO.Directory.GetFiles(path, searchPattern, searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="IFileSystem.IDirectory.GetFiles(string, string, System.IO.EnumerationOptions)" />
		public string[] GetFiles(string path,
		                         string searchPattern,
		                         EnumerationOptions enumerationOptions)
			=> System.IO.Directory.GetFiles(path, searchPattern, enumerationOptions);
#endif

		/// <inheritdoc cref="IFileSystem.IDirectory.GetFileSystemEntries(string)" />
		public string[] GetFileSystemEntries(string path)
			=> System.IO.Directory.GetFileSystemEntries(path);

		/// <inheritdoc cref="IFileSystem.IDirectory.GetFileSystemEntries(string, string)" />
		public string[] GetFileSystemEntries(string path, string searchPattern)
			=> System.IO.Directory.GetFileSystemEntries(path, searchPattern);

		/// <inheritdoc cref="IFileSystem.IDirectory.GetFileSystemEntries(string, string, System.IO.SearchOption)" />
		public string[] GetFileSystemEntries(string path,
		                                     string searchPattern,
		                                     SearchOption searchOption)
			=> System.IO.Directory.GetFileSystemEntries(path,
				searchPattern,
				searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="IFileSystem.IDirectory.GetFileSystemEntries(string, string, System.IO.EnumerationOptions)" />
		public string[] GetFileSystemEntries(string path,
		                                     string searchPattern,
		                                     EnumerationOptions enumerationOptions)
			=> System.IO.Directory.GetFileSystemEntries(path,
				searchPattern,
				enumerationOptions);
#endif

		/// <inheritdoc cref="IFileSystem.IDirectory.GetLastAccessTime(string)" />
		public DateTime GetLastAccessTime(string path)
			=> System.IO.Directory.GetLastAccessTime(path);

		/// <inheritdoc cref="IFileSystem.IDirectory.GetLastAccessTimeUtc(string)" />
		public DateTime GetLastAccessTimeUtc(string path)
			=> System.IO.Directory.GetLastAccessTimeUtc(path);

		/// <inheritdoc cref="IFileSystem.IDirectory.GetLastWriteTime(string)" />
		public DateTime GetLastWriteTime(string path)
			=> System.IO.Directory.GetLastWriteTime(path);

		/// <inheritdoc cref="IFileSystem.IDirectory.GetLastWriteTimeUtc(string)" />
		public DateTime GetLastWriteTimeUtc(string path)
			=> System.IO.Directory.GetLastWriteTimeUtc(path);

		/// <inheritdoc cref="IFileSystem.IDirectory.GetLogicalDrives()" />
		public string[] GetLogicalDrives()
			=> System.IO.Directory.GetLogicalDrives();

		/// <inheritdoc cref="IFileSystem.IDirectory.GetParent(string)" />
		public IFileSystem.IDirectoryInfo? GetParent(string path)
			=> DirectoryInfoWrapper.FromDirectoryInfo(
				System.IO.Directory.GetParent(path),
				FileSystem);

		/// <inheritdoc cref="IFileSystem.IDirectory.Move(string, string)" />
		public void Move(string sourceDirName, string destDirName)
			=> System.IO.Directory.Move(sourceDirName, destDirName);

#if FEATURE_FILESYSTEM_LINK
		/// <inheritdoc cref="IFileSystem.IDirectory.ResolveLinkTarget(string, bool)" />
		public IFileSystem.IFileSystemInfo? ResolveLinkTarget(
			string linkPath, bool returnFinalTarget)
			=> FileSystemInfoWrapper.FromFileSystemInfo(
				System.IO.Directory.ResolveLinkTarget(linkPath, returnFinalTarget),
				FileSystem);
#endif

		/// <inheritdoc cref="IFileSystem.IDirectory.SetCreationTime(string, DateTime)" />
		public void SetCreationTime(string path, DateTime creationTime)
			=> System.IO.Directory.SetCreationTime(path, creationTime);

		/// <inheritdoc cref="IFileSystem.IDirectory.SetCreationTimeUtc(string, DateTime)" />
		public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
			=> System.IO.Directory.SetCreationTimeUtc(path, creationTimeUtc);

		/// <inheritdoc cref="IFileSystem.IDirectory.SetCurrentDirectory(string)" />
		public void SetCurrentDirectory(string path)
			=> System.IO.Directory.SetCurrentDirectory(path);

		/// <inheritdoc cref="IFileSystem.IDirectory.SetLastAccessTime(string, DateTime)" />
		public void SetLastAccessTime(string path, DateTime lastAccessTime)
			=> System.IO.Directory.SetLastAccessTime(path, lastAccessTime);

		/// <inheritdoc cref="IFileSystem.IDirectory.SetLastAccessTimeUtc(string, DateTime)" />
		public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
			=> System.IO.Directory.SetLastAccessTimeUtc(path, lastAccessTimeUtc);

		/// <inheritdoc cref="IFileSystem.IDirectory.SetLastWriteTime(string, DateTime)" />
		public void SetLastWriteTime(string path, DateTime lastWriteTime)
			=> System.IO.Directory.SetLastWriteTime(path, lastWriteTime);

		/// <inheritdoc cref="IFileSystem.IDirectory.SetLastWriteTimeUtc(string, DateTime)" />
		public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
			=> System.IO.Directory.SetLastWriteTimeUtc(path, lastWriteTimeUtc);

		#endregion
	}
}