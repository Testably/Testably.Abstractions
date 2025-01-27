using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Statistics;
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
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.DirectoryInfo.RegisterPathProperty(Location.FullPath,
					nameof(Exists), PropertyAccess.Get);

			return base.Exists && FileSystemType == FileSystemTypes.Directory;
		}
	}

	/// <inheritdoc cref="IDirectoryInfo.Parent" />
	public IDirectoryInfo? Parent
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.DirectoryInfo.RegisterPathProperty(Location.FullPath,
					nameof(Parent), PropertyAccess.Get);

			return New(Location.GetParent(), _fileSystem);
		}
	}

	/// <inheritdoc cref="IDirectoryInfo.Root" />
	public IDirectoryInfo Root
	{
		get
		{
			using IDisposable registration = _fileSystem.StatisticsRegistration
				.DirectoryInfo.RegisterPathProperty(Location.FullPath,
					nameof(Root), PropertyAccess.Get);

			return New(_fileSystem.Storage.GetLocation(string.Empty.PrefixRoot(_fileSystem)),
				_fileSystem);
		}
	}

	/// <inheritdoc cref="IDirectoryInfo.Create()" />
	public void Create()
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(Create));

		FullName.EnsureValidFormat(_fileSystem);

		Container = _fileSystem.Storage.GetOrCreateContainer(Location,
			InMemoryContainer.NewDirectory,
			this);

		if (Container.Type != FileSystemTypes.Directory)
		{
			throw ExceptionFactory.CannotCreateFileAsAlreadyExists(_fileSystem.Execute, FullName);
		}

		ResetCache(!_fileSystem.Execute.IsNetFramework);
	}

	/// <inheritdoc cref="IDirectoryInfo.CreateSubdirectory(string)" />
	public IDirectoryInfo CreateSubdirectory(string path)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(CreateSubdirectory),
				path);

		DirectoryInfoMock directory = New(
			_fileSystem.Storage.GetLocation(
				_fileSystem.Execute.Path.Combine(FullName, path
					.EnsureValidFormat(_fileSystem, nameof(path),
						_fileSystem.Execute is { IsWindows: true, IsNetFramework: false }))),
			_fileSystem);
		directory.Create();
		return directory;
	}

	/// <inheritdoc cref="IDirectoryInfo.Delete(bool)" />
	public void Delete(bool recursive)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(Delete),
				recursive);

		if (!_fileSystem.Storage.DeleteContainer(
			_fileSystem.Storage.GetLocation(FullName), FileSystemTypes.Directory, recursive))
		{
			throw ExceptionFactory.DirectoryNotFound(FullName);
		}

		ResetCache(!_fileSystem.Execute.IsNetFramework);
	}

	/// <inheritdoc cref="IFileSystemInfo.Delete()" />
	public override void Delete()
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(Delete));

		if (!_fileSystem.Storage.DeleteContainer(Location, FileSystemTypes.Directory))
		{
			throw ExceptionFactory.DirectoryNotFound(Location.FullPath);
		}

		ResetCache(!_fileSystem.Execute.IsNetFramework);
	}

	/// <inheritdoc cref="IDirectoryInfo.EnumerateDirectories()" />
	public IEnumerable<IDirectoryInfo> EnumerateDirectories()
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(EnumerateDirectories));

		return EnumerateDirectories(
			EnumerationOptionsHelper.DefaultSearchPattern,
			SearchOption.TopDirectoryOnly);
	}

	/// <inheritdoc cref="IDirectoryInfo.EnumerateDirectories(string)" />
	public IEnumerable<IDirectoryInfo>
		EnumerateDirectories(string searchPattern)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(EnumerateDirectories),
				searchPattern);

		return EnumerateDirectories(searchPattern, SearchOption.TopDirectoryOnly);
	}

	/// <inheritdoc cref="IDirectoryInfo.EnumerateDirectories(string, SearchOption)" />
	public IEnumerable<IDirectoryInfo> EnumerateDirectories(
		string searchPattern, SearchOption searchOption)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(EnumerateDirectories),
				searchPattern, searchOption);

		return EnumerateInternal(FileSystemTypes.Directory,
				FullName,
				searchPattern,
				EnumerationOptionsHelper.FromSearchOption(searchOption))
			.Select(location => New(location, _fileSystem));
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectoryInfo.EnumerateDirectories(string, EnumerationOptions)" />
	public IEnumerable<IDirectoryInfo> EnumerateDirectories(
		string searchPattern,
		EnumerationOptions enumerationOptions)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(EnumerateDirectories),
				searchPattern, enumerationOptions);

		return EnumerateInternal(FileSystemTypes.Directory,
				FullName,
				searchPattern,
				enumerationOptions)
			.Select(location => New(location, _fileSystem));
	}
#endif

	/// <inheritdoc cref="IDirectoryInfo.EnumerateFiles()" />
	public IEnumerable<IFileInfo> EnumerateFiles()
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(EnumerateFiles));

		return EnumerateFiles(
			EnumerationOptionsHelper.DefaultSearchPattern,
			SearchOption.TopDirectoryOnly);
	}

	/// <inheritdoc cref="IDirectoryInfo.EnumerateFiles(string)" />
	public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(EnumerateFiles),
				searchPattern);

		return EnumerateFiles(searchPattern, SearchOption.TopDirectoryOnly);
	}

	/// <inheritdoc cref="IDirectoryInfo.EnumerateFiles(string, SearchOption)" />
	public IEnumerable<IFileInfo> EnumerateFiles(
		string searchPattern, SearchOption searchOption)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(EnumerateFiles),
				searchPattern, searchOption);

		return EnumerateInternal(FileSystemTypes.File,
				FullName,
				searchPattern,
				EnumerationOptionsHelper.FromSearchOption(searchOption))
			.Select(location => FileInfoMock.New(location, _fileSystem));
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectoryInfo.EnumerateFiles(string, EnumerationOptions)" />
	public IEnumerable<IFileInfo> EnumerateFiles(
		string searchPattern, EnumerationOptions enumerationOptions)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(EnumerateFiles),
				searchPattern, enumerationOptions);

		return EnumerateInternal(FileSystemTypes.File,
				FullName,
				searchPattern,
				enumerationOptions)
			.Select(location => FileInfoMock.New(location, _fileSystem));
	}
#endif

	/// <inheritdoc cref="IDirectoryInfo.EnumerateFileSystemInfos()" />
	public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos()
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(EnumerateFileSystemInfos));

		return EnumerateFileSystemInfos(
			EnumerationOptionsHelper.DefaultSearchPattern,
			SearchOption.TopDirectoryOnly);
	}

	/// <inheritdoc cref="IDirectoryInfo.EnumerateFileSystemInfos(string)" />
	public IEnumerable<IFileSystemInfo>
		EnumerateFileSystemInfos(string searchPattern)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(EnumerateFileSystemInfos),
				searchPattern);

		return EnumerateFileSystemInfos(searchPattern, SearchOption.TopDirectoryOnly);
	}

	/// <inheritdoc cref="IDirectoryInfo.EnumerateFileSystemInfos(string, SearchOption)" />
	public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(
		string searchPattern, SearchOption searchOption)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(EnumerateFileSystemInfos),
				searchPattern, searchOption);

		return EnumerateInternal(FileSystemTypes.DirectoryOrFile,
				FullName,
				searchPattern,
				EnumerationOptionsHelper.FromSearchOption(searchOption))
			.Select(location => FileSystemInfoMock.New(location, _fileSystem));
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectoryInfo.EnumerateFileSystemInfos(string, EnumerationOptions)" />
	public IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos(
		string searchPattern,
		EnumerationOptions enumerationOptions)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(EnumerateFileSystemInfos),
				searchPattern, enumerationOptions);

		return EnumerateInternal(FileSystemTypes.DirectoryOrFile,
				FullName,
				searchPattern,
				enumerationOptions)
			.Select(location => FileSystemInfoMock.New(location, _fileSystem));
	}
#endif

	/// <inheritdoc cref="IDirectoryInfo.GetDirectories()" />
	public IDirectoryInfo[] GetDirectories()
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(GetDirectories));

		return EnumerateDirectories().ToArray();
	}

	/// <inheritdoc cref="IDirectoryInfo.GetDirectories(string)" />
	public IDirectoryInfo[] GetDirectories(string searchPattern)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(GetDirectories),
				searchPattern);

		return EnumerateDirectories(searchPattern).ToArray();
	}

	/// <inheritdoc cref="IDirectoryInfo.GetDirectories(string, SearchOption)" />
	public IDirectoryInfo[] GetDirectories(
		string searchPattern, SearchOption searchOption)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(GetDirectories),
				searchPattern, searchOption);

		return EnumerateDirectories(searchPattern, searchOption).ToArray();
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectoryInfo.GetDirectories(string, EnumerationOptions)" />
	public IDirectoryInfo[] GetDirectories(
		string searchPattern,
		EnumerationOptions enumerationOptions)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(GetDirectories),
				searchPattern, enumerationOptions);

		return EnumerateDirectories(searchPattern, enumerationOptions).ToArray();
	}
#endif

	/// <inheritdoc cref="IDirectoryInfo.GetFiles()" />
	public IFileInfo[] GetFiles()
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(GetFiles));

		return EnumerateFiles().ToArray();
	}

	/// <inheritdoc cref="IDirectoryInfo.GetFiles(string)" />
	public IFileInfo[] GetFiles(string searchPattern)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(GetFiles),
				searchPattern);

		return EnumerateFiles(searchPattern).ToArray();
	}

	/// <inheritdoc cref="IDirectoryInfo.GetFiles(string, SearchOption)" />
	public IFileInfo[] GetFiles(string searchPattern,
		SearchOption searchOption)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(GetFiles),
				searchPattern, searchOption);

		return EnumerateFiles(searchPattern, searchOption).ToArray();
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectoryInfo.GetFiles(string, EnumerationOptions)" />
	public IFileInfo[] GetFiles(string searchPattern,
		EnumerationOptions enumerationOptions)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(GetFiles),
				searchPattern, enumerationOptions);

		return EnumerateFiles(searchPattern, enumerationOptions).ToArray();
	}
#endif

	/// <inheritdoc cref="IDirectoryInfo.GetFileSystemInfos()" />
	public IFileSystemInfo[] GetFileSystemInfos()
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(GetFileSystemInfos));

		return EnumerateFileSystemInfos().ToArray();
	}

	/// <inheritdoc cref="IDirectoryInfo.GetFileSystemInfos(string)" />
	public IFileSystemInfo[] GetFileSystemInfos(string searchPattern)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(GetFileSystemInfos),
				searchPattern);

		return EnumerateFileSystemInfos(searchPattern).ToArray();
	}

	/// <inheritdoc cref="IDirectoryInfo.GetFileSystemInfos(string, SearchOption)" />
	public IFileSystemInfo[] GetFileSystemInfos(
		string searchPattern, SearchOption searchOption)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(GetFileSystemInfos),
				searchPattern, searchOption);

		return EnumerateFileSystemInfos(searchPattern, searchOption).ToArray();
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	/// <inheritdoc cref="IDirectoryInfo.GetFileSystemInfos(string, EnumerationOptions)" />
	public IFileSystemInfo[] GetFileSystemInfos(
		string searchPattern,
		EnumerationOptions enumerationOptions)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(GetFileSystemInfos),
				searchPattern, enumerationOptions);

		return EnumerateFileSystemInfos(searchPattern, enumerationOptions).ToArray();
	}
#endif

	/// <inheritdoc cref="IDirectoryInfo.MoveTo(string)" />
	public void MoveTo(string destDirName)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterPathMethod(Location.FullPath, nameof(MoveTo),
				destDirName);

		Location = _fileSystem.Storage.Move(
			           _fileSystem.Storage.GetLocation(FullName),
			           _fileSystem.Storage.GetLocation(destDirName
				           .EnsureValidFormat(_fileSystem, nameof(destDirName))),
			           recursive: true)
		           ?? throw ExceptionFactory.DirectoryNotFound(FullName);
	}

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
			.AdjustLocationFromSearchPattern(_fileSystem,
				path.EnsureValidFormat(_fileSystem),
				searchPattern);
		return _fileSystem.Storage.EnumerateLocations(
			adjustedLocation.Location,
			fileSystemTypes,
			adjustedLocation.SearchPattern,
			enumerationOptions);
	}

	protected override IDisposable RegisterPathMethod(string name)
		=> _fileSystem.StatisticsRegistration.DirectoryInfo.RegisterPathMethod(Location.FullPath,
			name);

	protected override IDisposable RegisterPathMethod<T1>(string name, T1 parameter1)
		=> _fileSystem.StatisticsRegistration.DirectoryInfo.RegisterPathMethod(Location.FullPath,
			name,
			parameter1);

	protected override IDisposable RegisterPathProperty(string name, PropertyAccess access)
		=> _fileSystem.StatisticsRegistration.DirectoryInfo.RegisterPathProperty(Location.FullPath,
			name, access);
}
