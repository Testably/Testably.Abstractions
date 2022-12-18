using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     A mocked directory in the <see cref="InMemoryStorage" />.
/// </summary>
internal sealed class DirectoryInfoMock
	: FileSystemInfoMock, IDirectoryInfo
{
	private readonly MockFileSystem _fileSystem;

	private DirectoryInfoMock(IStorageLocation location,
		MockFileSystem fileSystem)
		: base(fileSystem, location, FileSystemTypes.Directory)
	{
		_fileSystem = fileSystem;
	}

	#region IDirectoryInfo Members

	/// <inheritdoc cref="IFileSystemInfo.Exists" />
	public override bool Exists
		=> base.Exists && FileSystemType == FileSystemTypes.Directory;

	/// <inheritdoc cref="IDirectoryInfo.Parent" />
	public IDirectoryInfo? Parent
		=> New(Location.GetParent(), _fileSystem);

	/// <inheritdoc cref="IDirectoryInfo.Root" />
	public IDirectoryInfo Root
		=> New(_fileSystem.Storage.GetLocation(string.Empty.PrefixRoot()),
			_fileSystem);

	/// <inheritdoc cref="IDirectoryInfo.Create()" />
	public void Create()
	{
		FullName.EnsureValidFormat(_fileSystem);

		Container = _fileSystem.Storage.GetOrCreateContainer(Location,
			InMemoryContainer.NewDirectory,
			this);

		ResetCache(!Execute.IsNetFramework);
	}

	/// <inheritdoc cref="IDirectoryInfo.CreateSubdirectory(string)" />
	public IDirectoryInfo CreateSubdirectory(string path)
	{
		DirectoryInfoMock directory = New(
			_fileSystem.Storage.GetLocation(
				_fileSystem.Path.Combine(FullName, path
					.EnsureValidFormat(_fileSystem, nameof(path),
						Execute.IsWindows && !Execute.IsNetFramework))),
			_fileSystem);
		directory.Create();
		return directory;
	}

	/// <inheritdoc />
	public override void Delete()
	{
		if (!_fileSystem.Storage.DeleteContainer(Location))
		{
			throw ExceptionFactory.DirectoryNotFound(Location.FullPath);
		}

		ResetCache(!Execute.IsNetFramework);
	}

	/// <inheritdoc cref="IDirectoryInfo.Delete(bool)" />
	public void Delete(bool recursive)
	{
		if (!_fileSystem.Storage.DeleteContainer(
			_fileSystem.Storage.GetLocation(FullName), recursive))
		{
			throw ExceptionFactory.DirectoryNotFound(FullName);
		}

		ResetCache(!Execute.IsNetFramework);
	}

	/// <inheritdoc cref="IDirectoryInfo.EnumerateDirectories()" />
	public IEnumerable<IDirectoryInfo> EnumerateDirectories()
		=> EnumerateDirectories(
			EnumerationOptionsHelper.DefaultSearchPattern,
			SearchOption.TopDirectoryOnly);

	/// <inheritdoc cref="IDirectoryInfo.EnumerateDirectories(string)" />
	public IEnumerable<IDirectoryInfo>
		EnumerateDirectories(string searchPattern)
		=> EnumerateDirectories(searchPattern, SearchOption.TopDirectoryOnly);

	/// <inheritdoc cref="IDirectoryInfo.EnumerateDirectories(string, SearchOption)" />
	public IEnumerable<IDirectoryInfo> EnumerateDirectories(
		string searchPattern, SearchOption searchOption)
		=> EnumerateInternal(FileSystemTypes.Directory,
				FullName,
				searchPattern,
				EnumerationOptionsHelper.FromSearchOption(searchOption))
			.Select(location => New(location, _fileSystem));

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectoryInfo.EnumerateDirectories(string, EnumerationOptions)" />
	public IEnumerable<IDirectoryInfo> EnumerateDirectories(
		string searchPattern,
		EnumerationOptions enumerationOptions)
		=> EnumerateInternal(FileSystemTypes.Directory,
				FullName,
				searchPattern,
				enumerationOptions)
			.Select(location => New(location, _fileSystem));
#endif

	/// <inheritdoc cref="IDirectoryInfo.EnumerateFiles()" />
	public IEnumerable<IFileInfo> EnumerateFiles()
		=> EnumerateFiles(
			EnumerationOptionsHelper.DefaultSearchPattern,
			SearchOption.TopDirectoryOnly);

	/// <inheritdoc cref="IDirectoryInfo.EnumerateFiles(string)" />
	public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern)
		=> EnumerateFiles(searchPattern, SearchOption.TopDirectoryOnly);

	/// <inheritdoc cref="IDirectoryInfo.EnumerateFiles(string, SearchOption)" />
	public IEnumerable<IFileInfo> EnumerateFiles(
		string searchPattern, SearchOption searchOption)
		=> EnumerateInternal(FileSystemTypes.File,
				FullName,
				searchPattern,
				EnumerationOptionsHelper.FromSearchOption(searchOption))
			.Select(location => FileInfoMock.New(location, _fileSystem));

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectoryInfo.EnumerateFiles(string, EnumerationOptions)" />
	public IEnumerable<IFileInfo> EnumerateFiles(
		string searchPattern, EnumerationOptions enumerationOptions)
		=> EnumerateInternal(FileSystemTypes.File,
				FullName,
				searchPattern,
				enumerationOptions)
			.Select(location => FileInfoMock.New(location, _fileSystem));
#endif

	/// <inheritdoc cref="IDirectoryInfo.EnumerateFileSystemInfos()" />
	public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos()
		=> EnumerateFileSystemInfos(
			EnumerationOptionsHelper.DefaultSearchPattern,
			SearchOption.TopDirectoryOnly);

	/// <inheritdoc cref="IDirectoryInfo.EnumerateFileSystemInfos(string)" />
	public IEnumerable<IFileSystemInfo>
		EnumerateFileSystemInfos(string searchPattern)
		=> EnumerateFileSystemInfos(searchPattern, SearchOption.TopDirectoryOnly);

	/// <inheritdoc cref="IDirectoryInfo.EnumerateFileSystemInfos(string, SearchOption)" />
	public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(
		string searchPattern, SearchOption searchOption)
		=> EnumerateInternal(FileSystemTypes.DirectoryOrFile,
				FullName,
				searchPattern,
				EnumerationOptionsHelper.FromSearchOption(searchOption))
			.Select(location => FileSystemInfoMock.New(location, _fileSystem));

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectoryInfo.EnumerateFileSystemInfos(string, EnumerationOptions)" />
	public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(
		string searchPattern,
		EnumerationOptions enumerationOptions)
		=> EnumerateInternal(FileSystemTypes.DirectoryOrFile,
				FullName,
				searchPattern,
				enumerationOptions)
			.Select(location => FileSystemInfoMock.New(location, _fileSystem));
#endif

	/// <inheritdoc cref="IDirectoryInfo.GetDirectories()" />
	public IDirectoryInfo[] GetDirectories()
		=> EnumerateDirectories().ToArray();

	/// <inheritdoc cref="IDirectoryInfo.GetDirectories(string)" />
	public IDirectoryInfo[] GetDirectories(string searchPattern)
		=> EnumerateDirectories(searchPattern).ToArray();

	/// <inheritdoc cref="IDirectoryInfo.GetDirectories(string, SearchOption)" />
	public IDirectoryInfo[] GetDirectories(
		string searchPattern, SearchOption searchOption)
		=> EnumerateDirectories(searchPattern, searchOption).ToArray();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectoryInfo.GetDirectories(string, EnumerationOptions)" />
	public IDirectoryInfo[] GetDirectories(
		string searchPattern,
		EnumerationOptions enumerationOptions)
		=> EnumerateDirectories(searchPattern, enumerationOptions).ToArray();
#endif

	/// <inheritdoc cref="IDirectoryInfo.GetFiles()" />
	public IFileInfo[] GetFiles()
		=> EnumerateFiles().ToArray();

	/// <inheritdoc cref="IDirectoryInfo.GetFiles(string)" />
	public IFileInfo[] GetFiles(string searchPattern)
		=> EnumerateFiles(searchPattern).ToArray();

	/// <inheritdoc cref="IDirectoryInfo.GetFiles(string, SearchOption)" />
	public IFileInfo[] GetFiles(string searchPattern,
		SearchOption searchOption)
		=> EnumerateFiles(searchPattern, searchOption).ToArray();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectoryInfo.GetFiles(string, EnumerationOptions)" />
	public IFileInfo[] GetFiles(string searchPattern,
		EnumerationOptions enumerationOptions)
		=> EnumerateFiles(searchPattern, enumerationOptions).ToArray();
#endif

	/// <inheritdoc cref="IDirectoryInfo.GetFileSystemInfos()" />
	public IFileSystemInfo[] GetFileSystemInfos()
		=> EnumerateFileSystemInfos().ToArray();

	/// <inheritdoc cref="IDirectoryInfo.GetFileSystemInfos(string)" />
	public IFileSystemInfo[] GetFileSystemInfos(string searchPattern)
		=> EnumerateFileSystemInfos(searchPattern).ToArray();

	/// <inheritdoc cref="IDirectoryInfo.GetFileSystemInfos(string, SearchOption)" />
	public IFileSystemInfo[] GetFileSystemInfos(
		string searchPattern, SearchOption searchOption)
		=> EnumerateFileSystemInfos(searchPattern, searchOption).ToArray();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectoryInfo.GetFileSystemInfos(string, EnumerationOptions)" />
	public IFileSystemInfo[] GetFileSystemInfos(
		string searchPattern,
		EnumerationOptions enumerationOptions)
		=> EnumerateFileSystemInfos(searchPattern, enumerationOptions).ToArray();
#endif

	/// <inheritdoc cref="IDirectoryInfo.MoveTo(string)" />
	public void MoveTo(string destDirName)
		=> Location = _fileSystem.Storage.Move(
			              _fileSystem.Storage.GetLocation(FullName),
			              _fileSystem.Storage.GetLocation(destDirName
				              .EnsureValidFormat(_fileSystem, nameof(destDirName))),
			              recursive: true)
		              ?? throw ExceptionFactory.DirectoryNotFound(FullName);

	#endregion

	[return: NotNullIfNotNull("location")]
	internal static new DirectoryInfoMock? New(IStorageLocation? location,
		MockFileSystem fileSystem)
	{
		if (location == null)
		{
			return null;
		}

		return new DirectoryInfoMock(location, fileSystem);
	}

	private IEnumerable<IStorageLocation> EnumerateInternal(
		FileSystemTypes fileSystemTypes,
		string path,
		string searchPattern,
		EnumerationOptions enumerationOptions)
	{
		StorageExtensions.AdjustedLocation adjustedLocation = _fileSystem.Storage
			.AdjustLocationFromSearchPattern(
				path.EnsureValidFormat(_fileSystem),
				searchPattern);
		return _fileSystem.Storage.EnumerateLocations(
			adjustedLocation.Location,
			fileSystemTypes,
			adjustedLocation.SearchPattern,
			enumerationOptions);
	}
}
