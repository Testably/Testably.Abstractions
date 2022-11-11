using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Testably.Abstractions.FileSystem;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.FileSystem;

internal sealed class DirectoryMock : IDirectory
{
	private readonly MockFileSystem _fileSystem;

	internal DirectoryMock(MockFileSystem fileSystem)
	{
		_fileSystem = fileSystem;
	}

	#region IDirectory Members

	/// <inheritdoc cref="IFileSystemExtensionPoint.FileSystem" />
	public IFileSystem FileSystem
		=> _fileSystem;

	/// <inheritdoc cref="IDirectory.CreateDirectory(string)" />
	public IDirectoryInfo CreateDirectory(string path)
	{
		path.EnsureValidFormat(_fileSystem);

		DirectoryInfoMock directory = DirectoryInfoMock.New(
			_fileSystem.Storage.GetLocation(path),
			_fileSystem);
		directory.Create();

		return directory;
	}

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	/// <inheritdoc cref="IDirectory.CreateDirectory(string, UnixFileMode)" />
	public IDirectoryInfo CreateDirectory(string path, UnixFileMode unixCreateMode)
	{
		IDirectoryInfo directoryInfo = CreateDirectory(path);
#pragma warning disable CA1416
		directoryInfo.UnixFileMode = unixCreateMode;
#pragma warning restore CA1416
		return directoryInfo;
	}
#endif

#if FEATURE_FILESYSTEM_LINK
	/// <inheritdoc cref="IDirectory.CreateSymbolicLink(string, string)" />
	public IFileSystemInfo CreateSymbolicLink(
		string path, string pathToTarget)
	{
		path.EnsureValidFormat(_fileSystem);
		IDirectoryInfo fileSystemInfo =
			_fileSystem.DirectoryInfo.New(path);
		fileSystemInfo.CreateAsSymbolicLink(pathToTarget);
		return fileSystemInfo;
	}
#endif

#if FEATURE_FILESYSTEM_NET7
	/// <inheritdoc cref="IDirectory.CreateTempSubdirectory(string)" />
	public IDirectoryInfo CreateTempSubdirectory(string? prefix = null)
	{
		string basePath;

		do
		{
			string localBasePath = _fileSystem.Path.Combine(
				_fileSystem.Path.GetTempPath(),
				(prefix ?? "") + _fileSystem.Path.GetFileNameWithoutExtension(
					_fileSystem.Path.GetRandomFileName()));
			Execute.OnMac(() => localBasePath = "/private" + localBasePath);
			basePath = localBasePath;
		} while (_fileSystem.Directory.Exists(basePath));

		_fileSystem.Directory.CreateDirectory(basePath);
		return _fileSystem.DirectoryInfo.New(basePath);
	}
#endif

	/// <inheritdoc cref="IDirectory.Delete(string)" />
	public void Delete(string path)
		=> _fileSystem.DirectoryInfo
		   .New(path.EnsureValidFormat(FileSystem))
		   .Delete();

	/// <inheritdoc cref="IDirectory.Delete(string, bool)" />
	public void Delete(string path, bool recursive)
		=> _fileSystem.DirectoryInfo
		   .New(path.EnsureValidFormat(FileSystem))
		   .Delete(recursive);

	/// <inheritdoc cref="IDirectory.EnumerateDirectories(string)" />
	public IEnumerable<string> EnumerateDirectories(string path)
		=> EnumerateDirectories(path,
			EnumerationOptionsHelper.DefaultSearchPattern,
			SearchOption.TopDirectoryOnly);

	/// <inheritdoc cref="IDirectory.EnumerateDirectories(string, string)" />
	public IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
		=> EnumerateDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);

	/// <inheritdoc cref="IDirectory.EnumerateDirectories(string, string, SearchOption)" />
	public IEnumerable<string> EnumerateDirectories(string path,
	                                                string searchPattern,
	                                                SearchOption searchOption)
		=> EnumerateInternal(FileSystemTypes.Directory,
			path,
			searchPattern,
			EnumerationOptionsHelper.FromSearchOption(searchOption));

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectory.EnumerateDirectories(string, string, EnumerationOptions)" />
	public IEnumerable<string> EnumerateDirectories(string path,
	                                                string searchPattern,
	                                                EnumerationOptions
		                                                enumerationOptions)
		=> EnumerateInternal(FileSystemTypes.Directory,
			path,
			searchPattern,
			enumerationOptions);
#endif

	/// <inheritdoc cref="IDirectory.EnumerateFiles(string)" />
	public IEnumerable<string> EnumerateFiles(string path)
		=> EnumerateFiles(path,
			EnumerationOptionsHelper.DefaultSearchPattern,
			SearchOption.TopDirectoryOnly);

	/// <inheritdoc cref="IDirectory.EnumerateFiles(string, string)" />
	public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
		=> EnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly);

	/// <inheritdoc cref="IDirectory.EnumerateFiles(string, string, SearchOption)" />
	public IEnumerable<string> EnumerateFiles(string path,
	                                          string searchPattern,
	                                          SearchOption searchOption)
		=> EnumerateInternal(FileSystemTypes.File,
			path,
			searchPattern,
			EnumerationOptionsHelper.FromSearchOption(searchOption));

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectory.EnumerateFiles(string, string, EnumerationOptions)" />
	public IEnumerable<string> EnumerateFiles(string path,
	                                          string searchPattern,
	                                          EnumerationOptions enumerationOptions)
		=> EnumerateInternal(FileSystemTypes.File,
			path,
			searchPattern,
			enumerationOptions);
#endif

	/// <inheritdoc cref="IDirectory.EnumerateFileSystemEntries(string)" />
	public IEnumerable<string> EnumerateFileSystemEntries(string path)
		=> EnumerateFileSystemEntries(path,
			EnumerationOptionsHelper.DefaultSearchPattern,
			SearchOption.TopDirectoryOnly);

	/// <inheritdoc cref="IDirectory.EnumerateFileSystemEntries(string, string)" />
	public IEnumerable<string> EnumerateFileSystemEntries(
		string path, string searchPattern)
		=> EnumerateFileSystemEntries(path,
			searchPattern,
			SearchOption.TopDirectoryOnly);

	/// <inheritdoc cref="IDirectory.EnumerateFileSystemEntries(string, string, SearchOption)" />
	public IEnumerable<string> EnumerateFileSystemEntries(string path,
	                                                      string searchPattern,
	                                                      SearchOption searchOption)
		=> EnumerateInternal(FileSystemTypes.DirectoryOrFile,
			path,
			searchPattern,
			EnumerationOptionsHelper.FromSearchOption(searchOption));

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectory.EnumerateFileSystemEntries(string, string, EnumerationOptions)" />
	public IEnumerable<string> EnumerateFileSystemEntries(string path,
	                                                      string searchPattern,
	                                                      EnumerationOptions
		                                                      enumerationOptions)
		=> EnumerateInternal(FileSystemTypes.DirectoryOrFile,
			path,
			searchPattern,
			enumerationOptions);
#endif

	/// <inheritdoc cref="IDirectory.Exists(string)" />
	public bool Exists([NotNullWhen(true)] string? path)
		=> DirectoryInfoMock.New(
			_fileSystem.Storage.GetLocation(path),
			_fileSystem)?.Exists ?? false;

	/// <inheritdoc cref="IDirectory.GetCreationTime(string)" />
	public DateTime GetCreationTime(string path)
		=> _fileSystem.Storage.GetContainer(
				_fileSystem.Storage.GetLocation(
					path.EnsureValidFormat(FileSystem)))
		   .CreationTime.Get(DateTimeKind.Local);

	/// <inheritdoc cref="IDirectory.GetCreationTimeUtc(string)" />
	public DateTime GetCreationTimeUtc(string path)
		=> _fileSystem.Storage.GetContainer(
				_fileSystem.Storage.GetLocation(
					path.EnsureValidFormat(FileSystem)))
		   .CreationTime.Get(DateTimeKind.Utc);

	/// <inheritdoc cref="IDirectory.GetCurrentDirectory()" />
	public string GetCurrentDirectory()
		=> _fileSystem.Storage.CurrentDirectory;

	/// <inheritdoc cref="IDirectory.GetDirectories(string)" />
	public string[] GetDirectories(string path)
		=> EnumerateDirectories(path).ToArray();

	/// <inheritdoc cref="IDirectory.GetDirectories(string, string)" />
	public string[] GetDirectories(string path, string searchPattern)
		=> EnumerateDirectories(path, searchPattern).ToArray();

	/// <inheritdoc cref="IDirectory.GetDirectories(string, string, SearchOption)" />
	public string[] GetDirectories(string path,
	                               string searchPattern,
	                               SearchOption searchOption)
		=> EnumerateDirectories(path, searchPattern, searchOption).ToArray();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectory.GetDirectories(string, string, EnumerationOptions)" />
	public string[] GetDirectories(string path,
	                               string searchPattern,
	                               EnumerationOptions enumerationOptions)
		=> EnumerateDirectories(path, searchPattern, enumerationOptions).ToArray();
#endif

	/// <inheritdoc cref="IDirectory.GetDirectoryRoot(string)" />
	public string GetDirectoryRoot(string path)
		=> _fileSystem.Path.GetPathRoot(
			   _fileSystem.Path.GetFullPath(path)) ??
		   throw ExceptionFactory.PathIsEmpty(nameof(path));

	/// <inheritdoc cref="IDirectory.GetFiles(string)" />
	public string[] GetFiles(string path)
		=> EnumerateFiles(path).ToArray();

	/// <inheritdoc cref="IDirectory.GetFiles(string, string)" />
	public string[] GetFiles(string path, string searchPattern)
		=> EnumerateFiles(path, searchPattern).ToArray();

	/// <inheritdoc cref="IDirectory.GetFiles(string, string, SearchOption)" />
	public string[] GetFiles(string path,
	                         string searchPattern,
	                         SearchOption searchOption)
		=> EnumerateFiles(path, searchPattern, searchOption).ToArray();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectory.GetFiles(string, string, EnumerationOptions)" />
	public string[] GetFiles(string path,
	                         string searchPattern,
	                         EnumerationOptions enumerationOptions)
		=> EnumerateFiles(path, searchPattern, enumerationOptions).ToArray();
#endif

	/// <inheritdoc cref="IDirectory.GetFileSystemEntries(string)" />
	public string[] GetFileSystemEntries(string path)
		=> EnumerateFileSystemEntries(path).ToArray();

	/// <inheritdoc cref="IDirectory.GetFileSystemEntries(string, string)" />
	public string[] GetFileSystemEntries(string path, string searchPattern)
		=> EnumerateFileSystemEntries(path, searchPattern).ToArray();

	/// <inheritdoc cref="IDirectory.GetFileSystemEntries(string, string, SearchOption)" />
	public string[] GetFileSystemEntries(string path,
	                                     string searchPattern,
	                                     SearchOption searchOption)
		=> EnumerateFileSystemEntries(path, searchPattern, searchOption).ToArray();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectory.GetFileSystemEntries(string, string, EnumerationOptions)" />
	public string[] GetFileSystemEntries(string path,
	                                     string searchPattern,
	                                     EnumerationOptions enumerationOptions)
		=> EnumerateFileSystemEntries(path, searchPattern, enumerationOptions)
		   .ToArray();
#endif

	/// <inheritdoc cref="IDirectory.GetLastAccessTime(string)" />
	public DateTime GetLastAccessTime(string path)
		=> _fileSystem.Storage.GetContainer(
				_fileSystem.Storage.GetLocation(
					path.EnsureValidFormat(FileSystem)))
		   .LastAccessTime.Get(DateTimeKind.Local);

	/// <inheritdoc cref="IDirectory.GetLastAccessTimeUtc(string)" />
	public DateTime GetLastAccessTimeUtc(string path)
		=> _fileSystem.Storage.GetContainer(
				_fileSystem.Storage.GetLocation(
					path.EnsureValidFormat(FileSystem)))
		   .LastAccessTime.Get(DateTimeKind.Utc);

	/// <inheritdoc cref="IDirectory.GetLastWriteTime(string)" />
	public DateTime GetLastWriteTime(string path)
		=> _fileSystem.Storage.GetContainer(
				_fileSystem.Storage.GetLocation(
					path.EnsureValidFormat(FileSystem)))
		   .LastWriteTime.Get(DateTimeKind.Local);

	/// <inheritdoc cref="IDirectory.GetLastWriteTimeUtc(string)" />
	public DateTime GetLastWriteTimeUtc(string path)
		=> _fileSystem.Storage.GetContainer(
				_fileSystem.Storage.GetLocation(
					path.EnsureValidFormat(FileSystem)))
		   .LastWriteTime.Get(DateTimeKind.Utc);

	/// <inheritdoc cref="IDirectory.GetLogicalDrives()" />
	public string[] GetLogicalDrives()
		=> _fileSystem.DriveInfo.GetDrives().Select(x => x.Name).ToArray();

	/// <inheritdoc cref="IDirectory.GetParent(string)" />
	public IDirectoryInfo? GetParent(string path)
		=> _fileSystem.DirectoryInfo
		   .New(path.EnsureValidArgument(_fileSystem, nameof(path)))
		   .Parent;

	/// <inheritdoc cref="IDirectory.Move(string, string)" />
	public void Move(string sourceDirName, string destDirName)
		=> _fileSystem.DirectoryInfo.New(sourceDirName
			   .EnsureValidFormat(_fileSystem, nameof(sourceDirName)))
		   .MoveTo(destDirName
			   .EnsureValidFormat(_fileSystem, nameof(destDirName)));

#if FEATURE_FILESYSTEM_LINK
	/// <inheritdoc cref="IDirectory.ResolveLinkTarget(string, bool)" />
	public IFileSystemInfo? ResolveLinkTarget(
		string linkPath, bool returnFinalTarget)
	{
		try
		{
			return _fileSystem.DirectoryInfo.New(linkPath
				   .EnsureValidFormat(_fileSystem, nameof(linkPath)))
			   .ResolveLinkTarget(returnFinalTarget);
		}
		catch (IOException ex)
			when (ex.HResult != -2147024773)
		{
			throw ExceptionFactory.FileNameCannotBeResolved(linkPath);
		}
	}
#endif

	/// <inheritdoc cref="IDirectory.SetCreationTime(string, DateTime)" />
	public void SetCreationTime(string path, DateTime creationTime)
		=> LoadDirectoryInfoOrThrowNotFoundException(path)
		   .CreationTime = creationTime;

	/// <inheritdoc cref="IDirectory.SetCreationTimeUtc(string, DateTime)" />
	public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
		=> LoadDirectoryInfoOrThrowNotFoundException(path)
		   .CreationTimeUtc = creationTimeUtc;

	/// <inheritdoc cref="IDirectory.SetCurrentDirectory(string)" />
	public void SetCurrentDirectory(string path)
	{
		IDirectoryInfo directoryInfo =
			_fileSystem.DirectoryInfo.New(path);
		if (!directoryInfo.Exists)
		{
			throw ExceptionFactory.DirectoryNotFound(
				FileSystem.Path.GetFullPath(path));
		}

		_fileSystem.Storage.CurrentDirectory = directoryInfo.FullName;
	}

	/// <inheritdoc cref="IDirectory.SetLastAccessTime(string, DateTime)" />
	public void SetLastAccessTime(string path, DateTime lastAccessTime)
		=> LoadDirectoryInfoOrThrowNotFoundException(path)
		   .LastAccessTime = lastAccessTime;

	/// <inheritdoc cref="IDirectory.SetLastAccessTimeUtc(string, DateTime)" />
	public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
		=> LoadDirectoryInfoOrThrowNotFoundException(path)
		   .LastAccessTimeUtc = lastAccessTimeUtc;

	/// <inheritdoc cref="IDirectory.SetLastWriteTime(string, DateTime)" />
	public void SetLastWriteTime(string path, DateTime lastWriteTime)
		=> LoadDirectoryInfoOrThrowNotFoundException(path)
		   .LastWriteTime = lastWriteTime;

	/// <inheritdoc cref="IDirectory.SetLastWriteTimeUtc(string, DateTime)" />
	public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
		=> LoadDirectoryInfoOrThrowNotFoundException(path)
		   .LastWriteTimeUtc = lastWriteTimeUtc;

	#endregion

	private IDirectoryInfo LoadDirectoryInfoOrThrowNotFoundException(
		string path)
	{
		IDirectoryInfo directoryInfo =
			_fileSystem.DirectoryInfo.New(path.EnsureValidFormat(FileSystem));
		if (!directoryInfo.Exists)
		{
#if NET7_0_OR_GREATER
			Execute.OnMac(
				() =>
					throw ExceptionFactory.DirectoryNotFound(
						FileSystem.Path.GetFullPath(path)),
				() =>
					throw ExceptionFactory.FileNotFound(
						FileSystem.Path.GetFullPath(path)));
#else
			Execute.OnWindows(
				() =>
					throw ExceptionFactory.FileNotFound(
						FileSystem.Path.GetFullPath(path)),
				() =>
					throw ExceptionFactory.DirectoryNotFound(
						FileSystem.Path.GetFullPath(path)));
#endif
		}

		return directoryInfo;
	}

	private IEnumerable<string> EnumerateInternal(FileSystemTypes fileSystemTypes,
	                                              string path,
	                                              string searchPattern,
	                                              EnumerationOptions enumerationOptions)
	{
		StorageExtensions.AdjustedLocation adjustedLocation = _fileSystem.Storage
		   .AdjustLocationFromSearchPattern(
				path.EnsureValidFormat(FileSystem),
				searchPattern);
		return _fileSystem.Storage.EnumerateLocations(
				adjustedLocation.Location,
				fileSystemTypes,
				adjustedLocation.SearchPattern,
				enumerationOptions)
		   .Select(x => _fileSystem
			   .GetSubdirectoryPath(x.FullPath, adjustedLocation.GivenPath));
	}
}