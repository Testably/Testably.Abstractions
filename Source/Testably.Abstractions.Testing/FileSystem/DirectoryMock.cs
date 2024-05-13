using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Statistics;
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

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem
		=> _fileSystem;

	/// <inheritdoc cref="IDirectory.CreateDirectory(string)" />
	public IDirectoryInfo CreateDirectory(string path)
	{
		using IDisposable registration = RegisterMethod(nameof(CreateDirectory),
			path);

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
		using IDisposable registration = RegisterMethod(nameof(CreateDirectory),
			path, unixCreateMode);

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
		using IDisposable registration = RegisterMethod(nameof(CreateSymbolicLink),
			path, pathToTarget);

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
		using IDisposable registration = RegisterMethod(nameof(CreateTempSubdirectory),
			prefix);

		string basePath;

		do
		{
			string localBasePath = _fileSystem.Execute.Path.Combine(
				_fileSystem.Execute.Path.GetTempPath(),
				(prefix ?? "") + _fileSystem.Execute.Path.GetFileNameWithoutExtension(
					_fileSystem.Execute.Path.GetRandomFileName()));
			if (_fileSystem.Execute.IsMac)
			{
				localBasePath = "/private" + localBasePath;
			}
			basePath = localBasePath;
		} while (_fileSystem.Directory.Exists(basePath));

		_fileSystem.Directory.CreateDirectory(basePath);
		return _fileSystem.DirectoryInfo.New(basePath);
	}
#endif

	/// <inheritdoc cref="IDirectory.Delete(string)" />
	public void Delete(string path)
	{
		using IDisposable registration = RegisterMethod(nameof(Delete),
			path);

		_fileSystem.DirectoryInfo
			.New(path.EnsureValidFormat(_fileSystem))
			.Delete();
	}

	/// <inheritdoc cref="IDirectory.Delete(string, bool)" />
	public void Delete(string path, bool recursive)
	{
		using IDisposable registration = RegisterMethod(nameof(Delete),
			path, recursive);

		_fileSystem.DirectoryInfo
			.New(path.EnsureValidFormat(_fileSystem))
			.Delete(recursive);
	}

	/// <inheritdoc cref="IDirectory.EnumerateDirectories(string)" />
	public IEnumerable<string> EnumerateDirectories(string path)
	{
		using IDisposable registration = RegisterMethod(nameof(EnumerateDirectories),
			path);

		return EnumerateDirectories(path,
			EnumerationOptionsHelper.DefaultSearchPattern,
			SearchOption.TopDirectoryOnly);
	}

	/// <inheritdoc cref="IDirectory.EnumerateDirectories(string, string)" />
	public IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
	{
		using IDisposable registration = RegisterMethod(nameof(EnumerateDirectories),
			path, searchPattern);

		return EnumerateDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);
	}

	/// <inheritdoc cref="IDirectory.EnumerateDirectories(string, string, SearchOption)" />
	public IEnumerable<string> EnumerateDirectories(string path,
		string searchPattern,
		SearchOption searchOption)
	{
		using IDisposable registration = RegisterMethod(nameof(EnumerateDirectories),
			path, searchPattern, searchOption);

		return EnumerateInternal(FileSystemTypes.Directory,
			path,
			searchPattern,
			EnumerationOptionsHelper.FromSearchOption(searchOption));
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectory.EnumerateDirectories(string, string, EnumerationOptions)" />
	public IEnumerable<string> EnumerateDirectories(string path,
		string searchPattern,
		EnumerationOptions enumerationOptions)
	{
		using IDisposable registration = RegisterMethod(nameof(EnumerateDirectories),
			path, searchPattern, enumerationOptions);

		return EnumerateInternal(FileSystemTypes.Directory,
			path,
			searchPattern,
			enumerationOptions);
	}
#endif

	/// <inheritdoc cref="IDirectory.EnumerateFiles(string)" />
	public IEnumerable<string> EnumerateFiles(string path)
	{
		using IDisposable registration = RegisterMethod(nameof(EnumerateFiles),
			path);

		return EnumerateFiles(path,
			EnumerationOptionsHelper.DefaultSearchPattern,
			SearchOption.TopDirectoryOnly);
	}

	/// <inheritdoc cref="IDirectory.EnumerateFiles(string, string)" />
	public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
	{
		using IDisposable registration = RegisterMethod(nameof(EnumerateFiles),
			path, searchPattern);

		return EnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
	}

	/// <inheritdoc cref="IDirectory.EnumerateFiles(string, string, SearchOption)" />
	public IEnumerable<string> EnumerateFiles(string path,
		string searchPattern,
		SearchOption searchOption)
	{
		using IDisposable registration = RegisterMethod(nameof(EnumerateFiles),
			path, searchPattern, searchOption);

		return EnumerateInternal(FileSystemTypes.File,
			path,
			searchPattern,
			EnumerationOptionsHelper.FromSearchOption(searchOption));
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectory.EnumerateFiles(string, string, EnumerationOptions)" />
	public IEnumerable<string> EnumerateFiles(string path,
		string searchPattern,
		EnumerationOptions enumerationOptions)
	{
		using IDisposable registration = RegisterMethod(nameof(EnumerateFiles),
			path, searchPattern, enumerationOptions);

		return EnumerateInternal(FileSystemTypes.File,
			path,
			searchPattern,
			enumerationOptions);
	}
#endif

	/// <inheritdoc cref="IDirectory.EnumerateFileSystemEntries(string)" />
	public IEnumerable<string> EnumerateFileSystemEntries(string path)
	{
		using IDisposable registration = RegisterMethod(nameof(EnumerateFileSystemEntries),
			path);

		return EnumerateFileSystemEntries(path,
			EnumerationOptionsHelper.DefaultSearchPattern,
			SearchOption.TopDirectoryOnly);
	}

	/// <inheritdoc cref="IDirectory.EnumerateFileSystemEntries(string, string)" />
	public IEnumerable<string> EnumerateFileSystemEntries(
		string path, string searchPattern)
	{
		using IDisposable registration = RegisterMethod(nameof(EnumerateFileSystemEntries),
			path, searchPattern);

		return EnumerateFileSystemEntries(path,
			searchPattern,
			SearchOption.TopDirectoryOnly);
	}

	/// <inheritdoc cref="IDirectory.EnumerateFileSystemEntries(string, string, SearchOption)" />
	public IEnumerable<string> EnumerateFileSystemEntries(string path,
		string searchPattern,
		SearchOption searchOption)
	{
		using IDisposable registration = RegisterMethod(nameof(EnumerateFileSystemEntries),
			path, searchPattern, searchOption);

		return EnumerateInternal(FileSystemTypes.DirectoryOrFile,
			path,
			searchPattern,
			EnumerationOptionsHelper.FromSearchOption(searchOption));
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectory.EnumerateFileSystemEntries(string, string, EnumerationOptions)" />
	public IEnumerable<string> EnumerateFileSystemEntries(string path,
		string searchPattern,
		EnumerationOptions enumerationOptions)
	{
		using IDisposable registration = RegisterMethod(nameof(EnumerateFileSystemEntries),
			path, searchPattern, enumerationOptions);

		return EnumerateInternal(FileSystemTypes.DirectoryOrFile,
			path,
			searchPattern,
			enumerationOptions);
	}
#endif

	/// <inheritdoc cref="IDirectory.Exists(string)" />
	public bool Exists([NotNullWhen(true)] string? path)
	{
		using IDisposable registration = RegisterMethod(nameof(Exists),
			path);

		return DirectoryInfoMock.New(
			_fileSystem.Storage.GetLocation(path),
			_fileSystem)?.Exists ?? false;
	}

	/// <inheritdoc cref="IDirectory.GetCreationTime(string)" />
	public DateTime GetCreationTime(string path)
	{
		using IDisposable registration = RegisterMethod(nameof(GetCreationTime),
			path);

		return _fileSystem.Storage.GetContainer(
				_fileSystem.Storage.GetLocation(
					path.EnsureValidFormat(_fileSystem)))
			.CreationTime.Get(DateTimeKind.Local);
	}

	/// <inheritdoc cref="IDirectory.GetCreationTimeUtc(string)" />
	public DateTime GetCreationTimeUtc(string path)
	{
		using IDisposable registration = RegisterMethod(nameof(GetCreationTimeUtc),
			path);

		return _fileSystem.Storage.GetContainer(
				_fileSystem.Storage.GetLocation(
					path.EnsureValidFormat(_fileSystem)))
			.CreationTime.Get(DateTimeKind.Utc);
	}

	/// <inheritdoc cref="IDirectory.GetCurrentDirectory()" />
	public string GetCurrentDirectory()
	{
		using IDisposable registration = RegisterMethod(nameof(GetCurrentDirectory));

		return _fileSystem.Storage.CurrentDirectory;
	}

	/// <inheritdoc cref="IDirectory.GetDirectories(string)" />
	public string[] GetDirectories(string path)
	{
		using IDisposable registration = RegisterMethod(nameof(GetDirectories),
			path);

		return EnumerateDirectories(path).ToArray();
	}

	/// <inheritdoc cref="IDirectory.GetDirectories(string, string)" />
	public string[] GetDirectories(string path, string searchPattern)
	{
		using IDisposable registration = RegisterMethod(nameof(GetDirectories),
			path, searchPattern);

		return EnumerateDirectories(path, searchPattern).ToArray();
	}

	/// <inheritdoc cref="IDirectory.GetDirectories(string, string, SearchOption)" />
	public string[] GetDirectories(string path,
		string searchPattern,
		SearchOption searchOption)
	{
		using IDisposable registration = RegisterMethod(nameof(GetDirectories),
			path, searchPattern, searchOption);

		return EnumerateDirectories(path, searchPattern, searchOption).ToArray();
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectory.GetDirectories(string, string, EnumerationOptions)" />
	public string[] GetDirectories(string path,
		string searchPattern,
		EnumerationOptions enumerationOptions)
	{
		using IDisposable registration = RegisterMethod(nameof(GetDirectories),
			path, searchPattern, enumerationOptions);

		return EnumerateDirectories(path, searchPattern, enumerationOptions).ToArray();
	}
#endif

	/// <inheritdoc cref="IDirectory.GetDirectoryRoot(string)" />
	public string GetDirectoryRoot(string path)
	{
		using IDisposable registration = RegisterMethod(nameof(GetDirectoryRoot),
			path);

		return _fileSystem.Execute.Path.GetPathRoot(
			       _fileSystem.Execute.Path.GetFullPath(path)) ??
		       throw ExceptionFactory.PathIsEmpty(nameof(path));
	}

	/// <inheritdoc cref="IDirectory.GetFiles(string)" />
	public string[] GetFiles(string path)
	{
		using IDisposable registration = RegisterMethod(nameof(GetFiles),
			path);

		return EnumerateFiles(path).ToArray();
	}

	/// <inheritdoc cref="IDirectory.GetFiles(string, string)" />
	public string[] GetFiles(string path, string searchPattern)
	{
		using IDisposable registration = RegisterMethod(nameof(GetFiles),
			path, searchPattern);

		return EnumerateFiles(path, searchPattern).ToArray();
	}

	/// <inheritdoc cref="IDirectory.GetFiles(string, string, SearchOption)" />
	public string[] GetFiles(string path,
		string searchPattern,
		SearchOption searchOption)
	{
		using IDisposable registration = RegisterMethod(nameof(GetFiles),
			path, searchPattern, searchOption);

		return EnumerateFiles(path, searchPattern, searchOption).ToArray();
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectory.GetFiles(string, string, EnumerationOptions)" />
	public string[] GetFiles(string path,
		string searchPattern,
		EnumerationOptions enumerationOptions)
	{
		using IDisposable registration = RegisterMethod(nameof(GetFiles),
			path, searchPattern, enumerationOptions);

		return EnumerateFiles(path, searchPattern, enumerationOptions).ToArray();
	}
#endif

	/// <inheritdoc cref="IDirectory.GetFileSystemEntries(string)" />
	public string[] GetFileSystemEntries(string path)
	{
		using IDisposable registration = RegisterMethod(nameof(GetFileSystemEntries),
			path);

		return EnumerateFileSystemEntries(path).ToArray();
	}

	/// <inheritdoc cref="IDirectory.GetFileSystemEntries(string, string)" />
	public string[] GetFileSystemEntries(string path, string searchPattern)
	{
		using IDisposable registration = RegisterMethod(nameof(GetFileSystemEntries),
			path, searchPattern);

		return EnumerateFileSystemEntries(path, searchPattern).ToArray();
	}

	/// <inheritdoc cref="IDirectory.GetFileSystemEntries(string, string, SearchOption)" />
	public string[] GetFileSystemEntries(string path,
		string searchPattern,
		SearchOption searchOption)
	{
		using IDisposable registration = RegisterMethod(nameof(GetFileSystemEntries),
			path, searchPattern, searchOption);

		return EnumerateFileSystemEntries(path, searchPattern, searchOption).ToArray();
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectory.GetFileSystemEntries(string, string, EnumerationOptions)" />
	public string[] GetFileSystemEntries(string path,
		string searchPattern,
		EnumerationOptions enumerationOptions)
	{
		using IDisposable registration = RegisterMethod(nameof(GetFileSystemEntries),
			path, searchPattern, enumerationOptions);

		return EnumerateFileSystemEntries(path, searchPattern, enumerationOptions)
			.ToArray();
	}
#endif

	/// <inheritdoc cref="IDirectory.GetLastAccessTime(string)" />
	public DateTime GetLastAccessTime(string path)
	{
		using IDisposable registration = RegisterMethod(nameof(GetLastAccessTime),
			path);

		return _fileSystem.Storage.GetContainer(
				_fileSystem.Storage.GetLocation(
					path.EnsureValidFormat(_fileSystem)))
			.LastAccessTime.Get(DateTimeKind.Local);
	}

	/// <inheritdoc cref="IDirectory.GetLastAccessTimeUtc(string)" />
	public DateTime GetLastAccessTimeUtc(string path)
	{
		using IDisposable registration = RegisterMethod(nameof(GetLastAccessTimeUtc),
			path);

		return _fileSystem.Storage.GetContainer(
				_fileSystem.Storage.GetLocation(
					path.EnsureValidFormat(_fileSystem)))
			.LastAccessTime.Get(DateTimeKind.Utc);
	}

	/// <inheritdoc cref="IDirectory.GetLastWriteTime(string)" />
	public DateTime GetLastWriteTime(string path)
	{
		using IDisposable registration = RegisterMethod(nameof(GetLastWriteTime),
			path);

		return _fileSystem.Storage.GetContainer(
				_fileSystem.Storage.GetLocation(
					path.EnsureValidFormat(_fileSystem)))
			.LastWriteTime.Get(DateTimeKind.Local);
	}

	/// <inheritdoc cref="IDirectory.GetLastWriteTimeUtc(string)" />
	public DateTime GetLastWriteTimeUtc(string path)
	{
		using IDisposable registration = RegisterMethod(nameof(GetLastWriteTimeUtc),
			path);

		return _fileSystem.Storage.GetContainer(
				_fileSystem.Storage.GetLocation(
					path.EnsureValidFormat(_fileSystem)))
			.LastWriteTime.Get(DateTimeKind.Utc);
	}

	/// <inheritdoc cref="IDirectory.GetLogicalDrives()" />
	public string[] GetLogicalDrives()
	{
		using IDisposable registration = RegisterMethod(nameof(GetLogicalDrives));

		return _fileSystem.DriveInfo.GetDrives().Select(x => x.Name).ToArray();
	}

	/// <inheritdoc cref="IDirectory.GetParent(string)" />
	public IDirectoryInfo? GetParent(string path)
	{
		using IDisposable registration = RegisterMethod(nameof(GetParent),
			path);

		return _fileSystem.DirectoryInfo
			.New(path.EnsureValidArgument(_fileSystem, nameof(path)))
			.Parent;
	}

	/// <inheritdoc cref="IDirectory.Move(string, string)" />
	public void Move(string sourceDirName, string destDirName)
	{
		using IDisposable registration = RegisterMethod(nameof(Move),
			sourceDirName, destDirName);

		_fileSystem.DirectoryInfo.New(sourceDirName
				.EnsureValidFormat(_fileSystem, nameof(sourceDirName)))
			.MoveTo(destDirName
				.EnsureValidFormat(_fileSystem, nameof(destDirName)));
	}

#if FEATURE_FILESYSTEM_LINK
	/// <inheritdoc cref="IDirectory.ResolveLinkTarget(string, bool)" />
	public IFileSystemInfo? ResolveLinkTarget(
		string linkPath, bool returnFinalTarget)
	{
		using IDisposable registration = RegisterMethod(nameof(ResolveLinkTarget),
			linkPath, returnFinalTarget);

		try
		{
			return _fileSystem.DirectoryInfo.New(linkPath
					.EnsureValidFormat(_fileSystem, nameof(linkPath)))
				.ResolveLinkTarget(returnFinalTarget);
		}
		catch (IOException ex)
			when (ex.HResult != -2147024773)
		{
			throw ExceptionFactory.FileNameCannotBeResolved(linkPath,
				_fileSystem.Execute.IsWindows ? -2147022975 : -2146232800);
		}
	}
#endif

	/// <inheritdoc cref="IDirectory.SetCreationTime(string, DateTime)" />
	public void SetCreationTime(string path, DateTime creationTime)
	{
		using IDisposable registration = RegisterMethod(nameof(SetCreationTime),
			path, creationTime);

		LoadDirectoryInfoOrThrowNotFoundException(path, ThrowMissingFileCreatedTimeException)
			.CreationTime = creationTime;
	}

	/// <inheritdoc cref="IDirectory.SetCreationTimeUtc(string, DateTime)" />
	public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
	{
		using IDisposable registration = RegisterMethod(nameof(SetCreationTimeUtc),
			path, creationTimeUtc);

		LoadDirectoryInfoOrThrowNotFoundException(path, ThrowMissingFileCreatedTimeException)
			.CreationTimeUtc = creationTimeUtc;
	}

	/// <inheritdoc cref="IDirectory.SetCurrentDirectory(string)" />
	public void SetCurrentDirectory(string path)
	{
		using IDisposable registration = RegisterMethod(nameof(SetCurrentDirectory),
			path);

		IDirectoryInfo directoryInfo =
			_fileSystem.DirectoryInfo.New(path);
		if (!directoryInfo.Exists)
		{
			throw ExceptionFactory.DirectoryNotFound(
				_fileSystem.Execute.Path.GetFullPath(path));
		}

		_fileSystem.Storage.CurrentDirectory = directoryInfo.FullName;
	}

	/// <inheritdoc cref="IDirectory.SetLastAccessTime(string, DateTime)" />
	public void SetLastAccessTime(string path, DateTime lastAccessTime)
	{
		using IDisposable registration = RegisterMethod(nameof(SetLastAccessTime),
			path, lastAccessTime);

		LoadDirectoryInfoOrThrowNotFoundException(path,
				ThrowMissingFileLastAccessOrLastWriteTimeException)
			.LastAccessTime = lastAccessTime;
	}

	/// <inheritdoc cref="IDirectory.SetLastAccessTimeUtc(string, DateTime)" />
	public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
	{
		using IDisposable registration = RegisterMethod(nameof(SetLastAccessTimeUtc),
			path, lastAccessTimeUtc);

		LoadDirectoryInfoOrThrowNotFoundException(path,
				ThrowMissingFileLastAccessOrLastWriteTimeException)
			.LastAccessTimeUtc = lastAccessTimeUtc;
	}

	/// <inheritdoc cref="IDirectory.SetLastWriteTime(string, DateTime)" />
	public void SetLastWriteTime(string path, DateTime lastWriteTime)
	{
		using IDisposable registration = RegisterMethod(nameof(SetLastWriteTime),
			path, lastWriteTime);

		LoadDirectoryInfoOrThrowNotFoundException(path,
				ThrowMissingFileLastAccessOrLastWriteTimeException)
			.LastWriteTime = lastWriteTime;
	}

	/// <inheritdoc cref="IDirectory.SetLastWriteTimeUtc(string, DateTime)" />
	public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
	{
		using IDisposable registration = RegisterMethod(nameof(SetLastWriteTimeUtc),
			path, lastWriteTimeUtc);

		LoadDirectoryInfoOrThrowNotFoundException(path,
				ThrowMissingFileLastAccessOrLastWriteTimeException)
			.LastWriteTimeUtc = lastWriteTimeUtc;
	}

	#endregion

	private IEnumerable<string> EnumerateInternal(FileSystemTypes fileSystemTypes,
		string path,
		string searchPattern,
		EnumerationOptions enumerationOptions)
	{
		StorageExtensions.AdjustedLocation adjustedLocation = _fileSystem.Storage
			.AdjustLocationFromSearchPattern(_fileSystem,
				path.EnsureValidFormat(_fileSystem),
				searchPattern);
		return _fileSystem.Storage.EnumerateLocations(
				adjustedLocation.Location,
				fileSystemTypes,
				adjustedLocation.SearchPattern,
				enumerationOptions)
			.Select(x => _fileSystem
				.GetSubdirectoryPath(x.FullPath, x.FriendlyName, adjustedLocation.GivenPath));
	}

	private IDirectoryInfo LoadDirectoryInfoOrThrowNotFoundException(
		string path, Action<MockFileSystem, string> onMissingCallback)
	{
		IDirectoryInfo directoryInfo =
			_fileSystem.DirectoryInfo.New(path.EnsureValidFormat(_fileSystem));
		if (!directoryInfo.Exists)
		{
			onMissingCallback.Invoke(_fileSystem, path);
		}

		return directoryInfo;
	}

	private IDisposable RegisterMethod(string name)
		=> _fileSystem.StatisticsRegistration.Directory.RegisterMethod(name);

	private IDisposable RegisterMethod<T1>(string name, T1 parameter1)
		=> _fileSystem.StatisticsRegistration.Directory.RegisterMethod(name,
			ParameterDescription.FromParameter(parameter1));

	private IDisposable RegisterMethod<T1, T2>(string name, T1 parameter1, T2 parameter2)
		=> _fileSystem.StatisticsRegistration.Directory.RegisterMethod(name,
			ParameterDescription.FromParameter(parameter1),
			ParameterDescription.FromParameter(parameter2));

	private IDisposable RegisterMethod<T1, T2, T3>(string name, T1 parameter1, T2 parameter2,
		T3 parameter3)
		=> _fileSystem.StatisticsRegistration.Directory.RegisterMethod(name,
			ParameterDescription.FromParameter(parameter1),
			ParameterDescription.FromParameter(parameter2),
			ParameterDescription.FromParameter(parameter3));

	private static void ThrowMissingFileCreatedTimeException(MockFileSystem fileSystem, string path)
	{
#if NET7_0_OR_GREATER
		if (!fileSystem.Execute.IsMac)
#else
		if (fileSystem.Execute.IsWindows)
#endif
		{
			throw ExceptionFactory.FileNotFound(
				fileSystem.Execute.Path.GetFullPath(path));
		}

		throw ExceptionFactory.DirectoryNotFound(
			fileSystem.Execute.Path.GetFullPath(path));
	}

	private static void ThrowMissingFileLastAccessOrLastWriteTimeException(
		MockFileSystem fileSystem,
		string path)
	{
#if !NET7_0_OR_GREATER
		if (!fileSystem.Execute.IsWindows)
		{
			throw ExceptionFactory.DirectoryNotFound(
				fileSystem.Execute.Path.GetFullPath(path));
		}
#endif
		throw ExceptionFactory.FileNotFound(
			fileSystem.Execute.Path.GetFullPath(path));
	}
}
