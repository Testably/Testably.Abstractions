using System;
using System.IO;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.FileSystem;

internal class FileSystemInfoMock : IFileSystemInfo
{
	protected FileSystemTypes FileSystemType { get; }
	protected IStorageLocation Location;
	private readonly MockFileSystem _fileSystem;

	protected IStorageContainer Container
	{
		get
		{
			if (_container is NullContainer)
			{
				RefreshInternal();
			}

			return _container;
		}
		set => _container = value;
	}

	private bool? _exists;
	private bool _isInitialized;
	private IStorageContainer _container;

	protected FileSystemInfoMock(MockFileSystem fileSystem, IStorageLocation location,
		FileSystemTypes fileSystemType)
	{
		_fileSystem = fileSystem;
		Location = location;
		_container = fileSystem.Storage.GetContainer(location);
		FileSystemType = _container is not NullContainer
			? _container.Type
			: fileSystemType;
	}

	#region IFileSystemInfo Members

	/// <inheritdoc cref="IFileSystemInfo.Attributes" />
	public FileAttributes Attributes
	{
		get => Container.Attributes;
		set => Container.Attributes = value;
	}

#if FEATURE_FILESYSTEM_LINK
	/// <inheritdoc cref="IFileSystemInfo.CreateAsSymbolicLink(string)" />
	public void CreateAsSymbolicLink(string pathToTarget)
	{
		FullName.EnsureValidFormat(_fileSystem);
		pathToTarget.ThrowCommonExceptionsIfPathToTargetIsInvalid(_fileSystem);
		if (_fileSystem.Storage.TryAddContainer(Location, InMemoryContainer.NewFile,
			out IStorageContainer? container))
		{
			Container = container;
			container.LinkTarget = pathToTarget;
		}
		else
		{
			throw ExceptionFactory.CannotCreateFileAsAlreadyExists(Location
				.FriendlyName);
		}
	}
#endif

	/// <inheritdoc cref="IFileSystemInfo.CreationTime" />
	public DateTime CreationTime
	{
		get => Container.CreationTime.Get(DateTimeKind.Local);
		set => Container.CreationTime.Set(value, DateTimeKind.Local);
	}

	/// <inheritdoc cref="IFileSystemInfo.CreationTimeUtc" />
	public DateTime CreationTimeUtc
	{
		get => Container.CreationTime.Get(DateTimeKind.Utc);
		set => Container.CreationTime.Set(value, DateTimeKind.Utc);
	}

	/// <inheritdoc cref="IFileSystemInfo.Delete()" />
	public virtual void Delete()
	{
		_fileSystem.Storage.DeleteContainer(Location);
		ResetCache(!Execute.IsNetFramework);
	}

	/// <inheritdoc cref="IFileSystemInfo.Exists" />
	public virtual bool Exists
	{
		get
		{
			RefreshInternal();
			_exists ??= !string.IsNullOrWhiteSpace(Location.FriendlyName) &&
			            Container is not NullContainer;
			return _exists.Value;
		}
	}

	/// <inheritdoc cref="IFileSystemInfo.Extension" />
	public string Extension
	{
		get
		{
			if (Location.FullPath.EndsWith(".") &&
			    !Execute.IsWindows)
			{
				return ".";
			}

			return _fileSystem.Path.GetExtension(Location.FullPath);
		}
	}

	/// <inheritdoc cref="IFileSystemInfo.Extensibility" />
	public IFileSystemExtensibility Extensibility
		=> Container.Extensibility;

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem
		=> _fileSystem;

	/// <inheritdoc cref="IFileSystemInfo.FullName" />
	public string FullName => Location.FullPath;

	/// <inheritdoc cref="IFileSystemInfo.LastAccessTime" />
	public DateTime LastAccessTime
	{
		get => Container.LastAccessTime.Get(DateTimeKind.Local);
		set => Container.LastAccessTime.Set(value, DateTimeKind.Local);
	}

	/// <inheritdoc cref="IFileSystemInfo.LastAccessTimeUtc" />
	public DateTime LastAccessTimeUtc
	{
		get => Container.LastAccessTime.Get(DateTimeKind.Utc);
		set => Container.LastAccessTime.Set(value, DateTimeKind.Utc);
	}

	/// <inheritdoc cref="IFileSystemInfo.LastWriteTime" />
	public DateTime LastWriteTime
	{
		get => Container.LastWriteTime.Get(DateTimeKind.Local);
		set => Container.LastWriteTime.Set(value, DateTimeKind.Local);
	}

	/// <inheritdoc cref="IFileSystemInfo.LastWriteTimeUtc" />
	public DateTime LastWriteTimeUtc
	{
		get => Container.LastWriteTime.Get(DateTimeKind.Utc);
		set => Container.LastWriteTime.Set(value, DateTimeKind.Utc);
	}

#if FEATURE_FILESYSTEM_LINK
	/// <inheritdoc cref="IFileSystemInfo.LinkTarget" />
	public string? LinkTarget
		=> Container.LinkTarget;
#endif

	/// <inheritdoc cref="IFileSystemInfo.Name" />
	public string Name
		=> _fileSystem.Path.GetPathRoot(Location.FullPath) == Location.FullPath
			? Location.FullPath
			: _fileSystem.Path.GetFileName(Location.FullPath.TrimEnd(
				_fileSystem.Path.DirectorySeparatorChar,
				_fileSystem.Path.AltDirectorySeparatorChar));

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	/// <inheritdoc cref="IFileSystemInfo.UnixFileMode" />
	public UnixFileMode UnixFileMode
	{
		get => Container.UnixFileMode;
		[UnsupportedOSPlatform("windows")]
		set
		{
			Execute.OnWindows(
				() => throw ExceptionFactory.UnixFileModeNotSupportedOnThisPlatform());

			Container.UnixFileMode = value;
		}
	}
#endif

	/// <inheritdoc cref="IFileSystemInfo.Refresh()" />
	public void Refresh()
	{
		ResetCache(true);
	}

#if FEATURE_FILESYSTEM_LINK
	/// <inheritdoc cref="IFileSystemInfo.ResolveLinkTarget(bool)" />
	public IFileSystemInfo? ResolveLinkTarget(bool returnFinalTarget)
	{
		try
		{
			IStorageLocation? targetLocation =
				_fileSystem.Storage.ResolveLinkTarget(
					Location,
					returnFinalTarget);
			if (targetLocation != null)
			{
				return New(targetLocation, _fileSystem);
			}

			return null;
		}
		catch (IOException ex) when (ex.HResult != -2147024773)
		{
			throw ExceptionFactory.FileNameCannotBeResolved(Location.FullPath,
				Execute.IsWindows ? -2147022975 : -2146232800);
		}
	}
#endif

	#endregion

#if NETSTANDARD2_0
	/// <inheritdoc cref="object.ToString()" />
#else
	/// <inheritdoc cref="FileSystemInfo.ToString()" />
#endif
	public override string ToString()
		=> Location.FriendlyName;

	internal static FileSystemInfoMock New(IStorageLocation location,
		MockFileSystem fileSystem)
	{
		IStorageContainer container = fileSystem.Storage.GetContainer(location);
		if (container.Type == FileSystemTypes.File)
		{
			return FileInfoMock.New(location, fileSystem);
		}

		if (container.Type == FileSystemTypes.Directory)
		{
			return DirectoryInfoMock.New(location, fileSystem);
		}

		return new FileSystemInfoMock(fileSystem, location,
			FileSystemTypes.DirectoryOrFile);
	}

	protected void ResetCache(bool resetExistsCache)
	{
		if (resetExistsCache)
		{
			_exists = null;
		}

		_isInitialized = false;
	}

	private void RefreshInternal()
	{
		if (_isInitialized)
		{
			return;
		}

		Container = _fileSystem.Storage.GetContainer(Location);
		_isInitialized = true;
	}
}
