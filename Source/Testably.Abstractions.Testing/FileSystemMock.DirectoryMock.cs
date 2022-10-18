using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
	private sealed class DirectoryMock : IFileSystem.IDirectory
	{
		private readonly FileSystemMock _fileSystem;

		internal DirectoryMock(FileSystemMock fileSystem)
		{
			_fileSystem = fileSystem;
		}

		#region IDirectory Members

		/// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
		public IFileSystem FileSystem
			=> _fileSystem;

		/// <inheritdoc cref="IFileSystem.IDirectory.CreateDirectory(string)" />
		public IFileSystem.IDirectoryInfo CreateDirectory(string path)
		{
			path.ThrowCommonExceptionsIfPathIsInvalid(_fileSystem);

			DirectoryInfoMock directory = DirectoryInfoMock.New(
				_fileSystem.Storage.GetLocation(path),
				_fileSystem);
			directory.Create();

			return directory;
		}

#if FEATURE_FILESYSTEM_LINK
		/// <inheritdoc cref="IFileSystem.IDirectory.CreateSymbolicLink(string, string)" />
		public IFileSystem.IFileSystemInfo CreateSymbolicLink(
			string path, string pathToTarget)
		{
			IFileSystem.IDirectoryInfo fileSystemInfo =
				_fileSystem.DirectoryInfo.New(path);
			fileSystemInfo.CreateAsSymbolicLink(pathToTarget);
			return fileSystemInfo;
		}
#endif

		/// <inheritdoc cref="IFileSystem.IDirectory.Delete(string)" />
		public void Delete(string path)
			=> _fileSystem.DirectoryInfo.New(path).Delete();

		/// <inheritdoc cref="IFileSystem.IDirectory.Delete(string, bool)" />
		public void Delete(string path, bool recursive)
			=> _fileSystem.DirectoryInfo.New(path).Delete(recursive);

		/// <inheritdoc cref="IFileSystem.IDirectory.EnumerateDirectories(string)" />
		public IEnumerable<string> EnumerateDirectories(string path)
			=> EnumerateDirectories(path,
				EnumerationOptionsHelper.DefaultSearchPattern,
				SearchOption.TopDirectoryOnly);

		/// <inheritdoc cref="IFileSystem.IDirectory.EnumerateDirectories(string, string)" />
		public IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
			=> EnumerateDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);

		/// <inheritdoc cref="IFileSystem.IDirectory.EnumerateDirectories(string, string, SearchOption)" />
		public IEnumerable<string> EnumerateDirectories(string path,
		                                                string searchPattern,
		                                                SearchOption searchOption)
			=> _fileSystem.Storage.EnumerateLocations(
					_fileSystem.Storage.GetLocation(path),
					FileSystemTypes.Directory,
					searchPattern,
					EnumerationOptionsHelper.FromSearchOption(searchOption))
			   .Select(x => _fileSystem
				   .GetSubdirectoryPath(x.FullPath, path,
						searchOption == SearchOption.AllDirectories));

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="IFileSystem.IDirectory.EnumerateDirectories(string, string, EnumerationOptions)" />
		public IEnumerable<string> EnumerateDirectories(string path,
		                                                string searchPattern,
		                                                EnumerationOptions
			                                                enumerationOptions)
			=> _fileSystem.Storage.EnumerateLocations(
					_fileSystem.Storage.GetLocation(path),
					FileSystemTypes.Directory,
					searchPattern,
					enumerationOptions)
			   .Select(x => _fileSystem
				   .GetSubdirectoryPath(x.FullPath, path,
						enumerationOptions.RecurseSubdirectories));
#endif

		/// <inheritdoc cref="IFileSystem.IDirectory.EnumerateFiles(string)" />
		public IEnumerable<string> EnumerateFiles(string path)
			=> EnumerateFiles(path,
				EnumerationOptionsHelper.DefaultSearchPattern,
				SearchOption.TopDirectoryOnly);

		/// <inheritdoc cref="IFileSystem.IDirectory.EnumerateFiles(string, string)" />
		public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
			=> EnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly);

		/// <inheritdoc cref="IFileSystem.IDirectory.EnumerateFiles(string, string, SearchOption)" />
		public IEnumerable<string> EnumerateFiles(string path,
		                                          string searchPattern,
		                                          SearchOption searchOption)
			=> _fileSystem.Storage.EnumerateLocations(
					_fileSystem.Storage.GetLocation(path),
					FileSystemTypes.File,
					searchPattern,
					EnumerationOptionsHelper.FromSearchOption(searchOption))
			   .Select(x => _fileSystem
				   .GetSubdirectoryPath(x.FullPath, path,
						searchOption == SearchOption.AllDirectories));

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="IFileSystem.IDirectory.EnumerateFiles(string, string, EnumerationOptions)" />
		public IEnumerable<string> EnumerateFiles(string path,
		                                          string searchPattern,
		                                          EnumerationOptions enumerationOptions)
			=> _fileSystem.Storage.EnumerateLocations(
					_fileSystem.Storage.GetLocation(path),
					FileSystemTypes.File,
					searchPattern,
					enumerationOptions)
			   .Select(x => _fileSystem
				   .GetSubdirectoryPath(x.FullPath, path,
						enumerationOptions.RecurseSubdirectories));
#endif

		/// <inheritdoc cref="IFileSystem.IDirectory.EnumerateFileSystemEntries(string)" />
		public IEnumerable<string> EnumerateFileSystemEntries(string path)
			=> EnumerateFileSystemEntries(path,
				EnumerationOptionsHelper.DefaultSearchPattern,
				SearchOption.TopDirectoryOnly);

		/// <inheritdoc cref="IFileSystem.IDirectory.EnumerateFileSystemEntries(string, string)" />
		public IEnumerable<string> EnumerateFileSystemEntries(
			string path, string searchPattern)
			=> EnumerateFileSystemEntries(path,
				searchPattern,
				SearchOption.TopDirectoryOnly);

		/// <inheritdoc cref="IFileSystem.IDirectory.EnumerateFileSystemEntries(string, string, SearchOption)" />
		public IEnumerable<string> EnumerateFileSystemEntries(string path,
			string searchPattern,
			SearchOption searchOption)
			=> _fileSystem.Storage.EnumerateLocations(
					_fileSystem.Storage.GetLocation(path),
					FileSystemTypes.DirectoryOrFile,
					searchPattern,
					EnumerationOptionsHelper.FromSearchOption(searchOption))
			   .Select(x => _fileSystem
				   .GetSubdirectoryPath(x.FullPath, path,
						searchOption == SearchOption.AllDirectories));

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="IFileSystem.IDirectory.EnumerateFileSystemEntries(string, string, EnumerationOptions)" />
		public IEnumerable<string> EnumerateFileSystemEntries(string path,
			string searchPattern,
			EnumerationOptions enumerationOptions)
			=> _fileSystem.Storage.EnumerateLocations(
					_fileSystem.Storage.GetLocation(path),
					FileSystemTypes.DirectoryOrFile,
					searchPattern,
					enumerationOptions)
			   .Select(x => _fileSystem
				   .GetSubdirectoryPath(x.FullPath, path,
						enumerationOptions.RecurseSubdirectories));
#endif

		/// <inheritdoc cref="IFileSystem.IDirectory.Exists(string)" />
		public bool Exists([NotNullWhen(true)] string? path)
			=> DirectoryInfoMock.New(
				_fileSystem.Storage.GetLocation(path),
				_fileSystem)?.Exists ?? false;

		/// <inheritdoc cref="IFileSystem.IDirectory.GetCreationTime(string)" />
		public DateTime GetCreationTime(string path)
			=> _fileSystem.Storage.GetContainer(
					_fileSystem.Storage.GetLocation(path))
			   .CreationTime.Get(DateTimeKind.Local);

		/// <inheritdoc cref="IFileSystem.IDirectory.GetCreationTimeUtc(string)" />
		public DateTime GetCreationTimeUtc(string path)
			=> _fileSystem.Storage.GetContainer(
					_fileSystem.Storage.GetLocation(path))
			   .CreationTime.Get(DateTimeKind.Utc);

		/// <inheritdoc cref="IFileSystem.IDirectory.GetCurrentDirectory()" />
		public string GetCurrentDirectory()
			=> _fileSystem.Storage.CurrentDirectory;

		/// <inheritdoc cref="IFileSystem.IDirectory.GetDirectories(string)" />
		public string[] GetDirectories(string path)
			=> EnumerateDirectories(path).ToArray();

		/// <inheritdoc cref="IFileSystem.IDirectory.GetDirectories(string, string)" />
		public string[] GetDirectories(string path, string searchPattern)
			=> EnumerateDirectories(path, searchPattern).ToArray();

		/// <inheritdoc cref="IFileSystem.IDirectory.GetDirectories(string, string, SearchOption)" />
		public string[] GetDirectories(string path,
		                               string searchPattern,
		                               SearchOption searchOption)
			=> EnumerateDirectories(path, searchPattern, searchOption).ToArray();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="IFileSystem.IDirectory.GetDirectories(string, string, EnumerationOptions)" />
		public string[] GetDirectories(string path,
		                               string searchPattern,
		                               EnumerationOptions enumerationOptions)
			=> EnumerateDirectories(path, searchPattern, enumerationOptions).ToArray();
#endif

		/// <inheritdoc cref="IFileSystem.IDirectory.GetDirectoryRoot(string)" />
		public string GetDirectoryRoot(string path)
			=> _fileSystem.Path.GetPathRoot(
				   _fileSystem.Path.GetFullPath(path)) ??
			   throw ExceptionFactory.PathIsEmpty(nameof(path));

		/// <inheritdoc cref="IFileSystem.IDirectory.GetFiles(string)" />
		public string[] GetFiles(string path)
			=> EnumerateFiles(path).ToArray();

		/// <inheritdoc cref="IFileSystem.IDirectory.GetFiles(string, string)" />
		public string[] GetFiles(string path, string searchPattern)
			=> EnumerateFiles(path, searchPattern).ToArray();

		/// <inheritdoc cref="IFileSystem.IDirectory.GetFiles(string, string, SearchOption)" />
		public string[] GetFiles(string path,
		                         string searchPattern,
		                         SearchOption searchOption)
			=> EnumerateFiles(path, searchPattern, searchOption).ToArray();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="IFileSystem.IDirectory.GetFiles(string, string, EnumerationOptions)" />
		public string[] GetFiles(string path,
		                         string searchPattern,
		                         EnumerationOptions enumerationOptions)
			=> EnumerateFiles(path, searchPattern, enumerationOptions).ToArray();
#endif

		/// <inheritdoc cref="IFileSystem.IDirectory.GetFileSystemEntries(string)" />
		public string[] GetFileSystemEntries(string path)
			=> EnumerateFileSystemEntries(path).ToArray();

		/// <inheritdoc cref="IFileSystem.IDirectory.GetFileSystemEntries(string, string)" />
		public string[] GetFileSystemEntries(string path, string searchPattern)
			=> EnumerateFileSystemEntries(path, searchPattern).ToArray();

		/// <inheritdoc cref="IFileSystem.IDirectory.GetFileSystemEntries(string, string, SearchOption)" />
		public string[] GetFileSystemEntries(string path,
		                                     string searchPattern,
		                                     SearchOption searchOption)
			=> EnumerateFileSystemEntries(path, searchPattern, searchOption).ToArray();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
		/// <inheritdoc cref="IFileSystem.IDirectory.GetFileSystemEntries(string, string, EnumerationOptions)" />
		public string[] GetFileSystemEntries(string path,
		                                     string searchPattern,
		                                     EnumerationOptions enumerationOptions)
			=> EnumerateFileSystemEntries(path, searchPattern, enumerationOptions)
			   .ToArray();
#endif

		/// <inheritdoc cref="IFileSystem.IDirectory.GetLastAccessTime(string)" />
		public DateTime GetLastAccessTime(string path)
			=> _fileSystem.Storage.GetContainer(
					_fileSystem.Storage.GetLocation(path))
			   .LastAccessTime.Get(DateTimeKind.Local);

		/// <inheritdoc cref="IFileSystem.IDirectory.GetLastAccessTimeUtc(string)" />
		public DateTime GetLastAccessTimeUtc(string path)
			=> _fileSystem.Storage.GetContainer(
					_fileSystem.Storage.GetLocation(path))
			   .LastAccessTime.Get(DateTimeKind.Utc);

		/// <inheritdoc cref="IFileSystem.IDirectory.GetLastWriteTime(string)" />
		public DateTime GetLastWriteTime(string path)
			=> _fileSystem.Storage.GetContainer(
					_fileSystem.Storage.GetLocation(path))
			   .LastWriteTime.Get(DateTimeKind.Local);

		/// <inheritdoc cref="IFileSystem.IDirectory.GetLastWriteTimeUtc(string)" />
		public DateTime GetLastWriteTimeUtc(string path)
			=> _fileSystem.Storage.GetContainer(
					_fileSystem.Storage.GetLocation(path))
			   .LastWriteTime.Get(DateTimeKind.Utc);

		/// <inheritdoc cref="IFileSystem.IDirectory.GetLogicalDrives()" />
		public string[] GetLogicalDrives()
			=> _fileSystem.DriveInfo.GetDrives().Select(x => x.Name).ToArray();

		/// <inheritdoc cref="IFileSystem.IDirectory.GetParent(string)" />
		public IFileSystem.IDirectoryInfo? GetParent(string path)
			=> _fileSystem.DirectoryInfo.New(path).Parent;

		/// <inheritdoc cref="IFileSystem.IDirectory.Move(string, string)" />
		public void Move(string sourceDirName, string destDirName)
			=> _fileSystem.DirectoryInfo.New(sourceDirName)
			   .MoveTo(destDirName);

#if FEATURE_FILESYSTEM_LINK
		/// <inheritdoc cref="IFileSystem.IDirectory.ResolveLinkTarget(string, bool)" />
		public IFileSystem.IFileSystemInfo? ResolveLinkTarget(
			string linkPath, bool returnFinalTarget)
		{
			try
			{
				return _fileSystem.DirectoryInfo.New(linkPath)
				   .ResolveLinkTarget(returnFinalTarget);
			}
			catch (IOException)
			{
				throw ExceptionFactory.FileNameCannotBeResolved(linkPath);
			}
		}
#endif

		/// <inheritdoc cref="IFileSystem.IDirectory.SetCreationTime(string, DateTime)" />
		public void SetCreationTime(string path, DateTime creationTime)
			=> LoadDirectoryInfoOrThrowNotFoundException(path)
			   .CreationTime = creationTime;

		/// <inheritdoc cref="IFileSystem.IDirectory.SetCreationTimeUtc(string, DateTime)" />
		public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
			=> LoadDirectoryInfoOrThrowNotFoundException(path)
			   .CreationTimeUtc = creationTimeUtc;

		/// <inheritdoc cref="IFileSystem.IDirectory.SetCurrentDirectory(string)" />
		public void SetCurrentDirectory(string path)
		{

			IFileSystem.IDirectoryInfo directoryInfo =
				_fileSystem.DirectoryInfo.New(path);
			if (!directoryInfo.Exists)
			{
				throw ExceptionFactory.DirectoryNotFound(
					FileSystem.Path.GetFullPath(path));
			}
			_fileSystem.Storage.CurrentDirectory = directoryInfo.FullName;
		}

		/// <inheritdoc cref="IFileSystem.IDirectory.SetLastAccessTime(string, DateTime)" />
		public void SetLastAccessTime(string path, DateTime lastAccessTime)
			=> LoadDirectoryInfoOrThrowNotFoundException(path)
			   .LastAccessTime = lastAccessTime;

		/// <inheritdoc cref="IFileSystem.IDirectory.SetLastAccessTimeUtc(string, DateTime)" />
		public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
			=> LoadDirectoryInfoOrThrowNotFoundException(path)
			   .LastAccessTimeUtc = lastAccessTimeUtc;

		/// <inheritdoc cref="IFileSystem.IDirectory.SetLastWriteTime(string, DateTime)" />
		public void SetLastWriteTime(string path, DateTime lastWriteTime)
			=> LoadDirectoryInfoOrThrowNotFoundException(path)
			   .LastWriteTime = lastWriteTime;

		/// <inheritdoc cref="IFileSystem.IDirectory.SetLastWriteTimeUtc(string, DateTime)" />
		public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
			=> LoadDirectoryInfoOrThrowNotFoundException(path)
			   .LastWriteTimeUtc = lastWriteTimeUtc;

		#endregion

		private IFileSystem.IDirectoryInfo LoadDirectoryInfoOrThrowNotFoundException(
			string path)
		{
			IFileSystem.IDirectoryInfo directoryInfo =
				_fileSystem.DirectoryInfo.New(path);
			if (!directoryInfo.Exists)
			{
				Execute.OnWindows(
					() =>
						throw ExceptionFactory.FileNotFound(
							FileSystem.Path.GetFullPath(path)),
					() =>
						throw ExceptionFactory.DirectoryNotFound(
							FileSystem.Path.GetFullPath(path)));
			}

			return directoryInfo;
		}
	}
}