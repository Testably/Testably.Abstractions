using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions;

public partial interface IFileSystem
{
	/// <summary>
	///     Abstractions for <see cref="System.IO.Directory" />.
	/// </summary>
	interface IDirectory : IFileSystemExtensionPoint
	{
		/// <inheritdoc cref="System.IO.Directory.CreateDirectory(string)" />
		IDirectoryInfo CreateDirectory(string path);

#if FEATURE_FILESYSTEM_LINK
		/// <inheritdoc cref="System.IO.Directory.CreateSymbolicLink(string, string)" />
		IFileSystemInfo CreateSymbolicLink(string path, string pathToTarget);
#endif

		/// <inheritdoc cref="System.IO.Directory.Delete(string)" />
		void Delete(string path);

		/// <inheritdoc cref="System.IO.Directory.Delete(string, bool)" />
		void Delete(string path, bool recursive);

		/// <inheritdoc cref="System.IO.Directory.EnumerateDirectories(string)" />
		IEnumerable<string> EnumerateDirectories(string path);

		/// <inheritdoc cref="System.IO.Directory.EnumerateDirectories(string, string)" />
		IEnumerable<string> EnumerateDirectories(string path, string searchPattern);

		/// <inheritdoc cref="System.IO.Directory.EnumerateDirectories(string, string, System.IO.SearchOption)" />
		IEnumerable<string> EnumerateDirectories(string path,
		                                         string searchPattern,
		                                         SearchOption searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="System.IO.Directory.EnumerateDirectories(string, string, System.IO.EnumerationOptions)" />
		IEnumerable<string> EnumerateDirectories(string path,
		                                         string searchPattern,
		                                         EnumerationOptions enumerationOptions);
#endif

		/// <inheritdoc cref="System.IO.Directory.EnumerateFiles(string)" />
		IEnumerable<string> EnumerateFiles(string path);

		/// <inheritdoc cref="System.IO.Directory.EnumerateFiles(string, string)" />
		IEnumerable<string> EnumerateFiles(string path, string searchPattern);

		/// <inheritdoc cref="System.IO.Directory.EnumerateFiles(string, string, System.IO.SearchOption)" />
		IEnumerable<string> EnumerateFiles(string path,
		                                   string searchPattern,
		                                   SearchOption searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="System.IO.Directory.EnumerateFiles(string, string, System.IO.EnumerationOptions)" />
		IEnumerable<string> EnumerateFiles(string path,
		                                   string searchPattern,
		                                   EnumerationOptions enumerationOptions);
#endif

		/// <inheritdoc cref="System.IO.Directory.EnumerateFileSystemEntries(string)" />
		IEnumerable<string> EnumerateFileSystemEntries(string path);

		/// <inheritdoc cref="System.IO.Directory.EnumerateFileSystemEntries(string, string)" />
		IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern);

		/// <inheritdoc cref="System.IO.Directory.EnumerateFileSystemEntries(string, string, System.IO.SearchOption)" />
		IEnumerable<string> EnumerateFileSystemEntries(string path,
		                                               string searchPattern,
		                                               SearchOption searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="System.IO.Directory.EnumerateFileSystemEntries(string, string, System.IO.EnumerationOptions)" />
		IEnumerable<string> EnumerateFileSystemEntries(string path,
		                                               string searchPattern,
		                                               EnumerationOptions
			                                               enumerationOptions);
#endif

		/// <inheritdoc cref="System.IO.Directory.Exists(string)" />
		bool Exists([NotNullWhen(true)] string? path);

		/// <inheritdoc cref="System.IO.Directory.GetCreationTime(string)" />
		DateTime GetCreationTime(string path);

		/// <inheritdoc cref="System.IO.Directory.GetCreationTimeUtc(string)" />
		DateTime GetCreationTimeUtc(string path);

		/// <inheritdoc cref="System.IO.Directory.GetCurrentDirectory()" />
		string GetCurrentDirectory();

		/// <inheritdoc cref="System.IO.Directory.GetDirectories(string)" />
		string[] GetDirectories(string path);

		/// <inheritdoc cref="System.IO.Directory.GetDirectories(string, string)" />
		string[] GetDirectories(string path, string searchPattern);

		/// <inheritdoc cref="System.IO.Directory.GetDirectories(string, string, System.IO.SearchOption)" />
		string[] GetDirectories(string path,
		                        string searchPattern,
		                        SearchOption searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="System.IO.Directory.GetDirectories(string, string, System.IO.EnumerationOptions)" />
		string[] GetDirectories(string path,
		                        string searchPattern,
		                        EnumerationOptions enumerationOptions);
#endif

		/// <inheritdoc cref="System.IO.Directory.GetDirectoryRoot(string)" />
		string GetDirectoryRoot(string path);

		/// <inheritdoc cref="System.IO.Directory.GetFiles(string)" />
		string[] GetFiles(string path);

		/// <inheritdoc cref="System.IO.Directory.GetFiles(string, string)" />
		string[] GetFiles(string path, string searchPattern);

		/// <inheritdoc cref="System.IO.Directory.GetFiles(string, string, System.IO.SearchOption)" />
		string[] GetFiles(string path,
		                  string searchPattern,
		                  SearchOption searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="System.IO.Directory.GetFiles(string, string, System.IO.EnumerationOptions)" />
		string[] GetFiles(string path,
		                  string searchPattern,
		                  EnumerationOptions enumerationOptions);
#endif

		/// <inheritdoc cref="System.IO.Directory.GetFileSystemEntries(string)" />
		string[] GetFileSystemEntries(string path);

		/// <inheritdoc cref="System.IO.Directory.GetFileSystemEntries(string, string)" />
		string[] GetFileSystemEntries(string path, string searchPattern);

		/// <inheritdoc cref="System.IO.Directory.GetFileSystemEntries(string, string, System.IO.SearchOption)" />
		string[] GetFileSystemEntries(string path,
		                              string searchPattern,
		                              SearchOption searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="System.IO.Directory.GetFileSystemEntries(string, string, System.IO.EnumerationOptions)" />
		string[] GetFileSystemEntries(string path,
		                              string searchPattern,
		                              EnumerationOptions enumerationOptions);
#endif

		/// <inheritdoc cref="System.IO.Directory.GetLastAccessTime(string)" />
		DateTime GetLastAccessTime(string path);

		/// <inheritdoc cref="System.IO.Directory.GetLastAccessTimeUtc(string)" />
		DateTime GetLastAccessTimeUtc(string path);

		/// <inheritdoc cref="System.IO.Directory.GetLastWriteTime(string)" />
		DateTime GetLastWriteTime(string path);

		/// <inheritdoc cref="System.IO.Directory.GetLastWriteTimeUtc(string)" />
		DateTime GetLastWriteTimeUtc(string path);

		/// <inheritdoc cref="System.IO.Directory.GetLogicalDrives()" />
		string[] GetLogicalDrives();

		/// <inheritdoc cref="System.IO.Directory.GetParent(string)" />
		IDirectoryInfo? GetParent(string path);

		/// <inheritdoc cref="System.IO.Directory.Move(string, string)" />
		void Move(string sourceDirName, string destDirName);

#if FEATURE_FILESYSTEM_LINK
		/// <inheritdoc cref="System.IO.Directory.ResolveLinkTarget(string, bool)" />
		IFileSystemInfo? ResolveLinkTarget(string linkPath, bool returnFinalTarget);
#endif

		/// <inheritdoc cref="System.IO.Directory.SetCreationTime(string, DateTime)" />
		void SetCreationTime(string path, DateTime creationTime);

		/// <inheritdoc cref="System.IO.Directory.SetCreationTimeUtc(string, DateTime)" />
		void SetCreationTimeUtc(string path, DateTime creationTimeUtc);

		/// <inheritdoc cref="System.IO.Directory.SetCurrentDirectory(string)" />
		void SetCurrentDirectory(string path);

		/// <inheritdoc cref="System.IO.Directory.SetLastAccessTime(string, DateTime)" />
		void SetLastAccessTime(string path, DateTime lastAccessTime);

		/// <inheritdoc cref="System.IO.Directory.SetLastAccessTimeUtc(string, DateTime)" />
		void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc);

		/// <inheritdoc cref="System.IO.Directory.SetLastWriteTime(string, DateTime)" />
		void SetLastWriteTime(string path, DateTime lastWriteTime);

		/// <inheritdoc cref="System.IO.Directory.SetLastWriteTimeUtc(string, DateTime)" />
		void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc);
	}
}