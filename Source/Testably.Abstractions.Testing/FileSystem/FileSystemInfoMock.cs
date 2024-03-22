using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Testably.Abstractions.Helpers;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.FileSystem;

internal class FileSystemInfoMock : IFileSystemInfo, IFileSystemExtensibility
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
		get
		{
			using IDisposable registration =
				RegisterProperty(nameof(Attributes), PropertyAccess.Get);

			return Container.Attributes;
		}
		set
		{
			using IDisposable registration =
				RegisterProperty(nameof(Attributes), PropertyAccess.Set);

			Container.Attributes = value;
		}
	}

#if FEATURE_FILESYSTEM_LINK
	/// <inheritdoc cref="IFileSystemInfo.CreateAsSymbolicLink(string)" />
	public void CreateAsSymbolicLink(string pathToTarget)
	{
		using IDisposable registration = RegisterMethod(nameof(CreateAsSymbolicLink),
			pathToTarget);

		if (!_fileSystem.Execute.IsWindows && string.IsNullOrWhiteSpace(FullName))
		{
			return;
		}

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
			throw ExceptionFactory.CannotCreateFileAsAlreadyExists(
				_fileSystem.Execute,
				Location.FriendlyName);
		}
	}
#endif

	/// <inheritdoc cref="IFileSystemInfo.CreationTime" />
	public DateTime CreationTime
	{
		get
		{
			using IDisposable registration =
				RegisterProperty(nameof(CreationTime), PropertyAccess.Get);

			return Container.CreationTime.Get(DateTimeKind.Local);
		}
		set
		{
			using IDisposable registration =
				RegisterProperty(nameof(CreationTime), PropertyAccess.Set);

			Container.CreationTime.Set(value, DateTimeKind.Local);
		}
	}

	/// <inheritdoc cref="IFileSystemInfo.CreationTimeUtc" />
	public DateTime CreationTimeUtc
	{
		get
		{
			using IDisposable registration =
				RegisterProperty(nameof(CreationTimeUtc), PropertyAccess.Get);

			return Container.CreationTime.Get(DateTimeKind.Utc);
		}
		set
		{
			using IDisposable registration =
				RegisterProperty(nameof(CreationTimeUtc), PropertyAccess.Set);

			Container.CreationTime.Set(value, DateTimeKind.Utc);
		}
	}

	/// <inheritdoc cref="IFileSystemInfo.Delete()" />
	public virtual void Delete()
	{
		using IDisposable registration = RegisterMethod(nameof(Delete));

		_fileSystem.Storage.DeleteContainer(Location);
		ResetCache(!_fileSystem.Execute.IsNetFramework);
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
			using IDisposable registration =
				RegisterProperty(nameof(Extension), PropertyAccess.Get);

			if (Location.FullPath.EndsWith('.') &&
			    !_fileSystem.Execute.IsWindows)
			{
				return ".";
			}

			return _fileSystem.Execute.Path.GetExtension(Location.FullPath);
		}
	}

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem
		=> _fileSystem;

	/// <inheritdoc cref="IFileSystemInfo.FullName" />
	public string FullName
	{
		get
		{
			using IDisposable registration = RegisterProperty(nameof(FullName), PropertyAccess.Get);

			return Location.FullPath;
		}
	}

	/// <inheritdoc cref="IFileSystemInfo.LastAccessTime" />
	public DateTime LastAccessTime
	{
		get
		{
			using IDisposable registration =
				RegisterProperty(nameof(LastAccessTime), PropertyAccess.Get);

			return Container.LastAccessTime.Get(DateTimeKind.Local);
		}
		set
		{
			using IDisposable registration =
				RegisterProperty(nameof(LastAccessTime), PropertyAccess.Set);

			Container.LastAccessTime.Set(value, DateTimeKind.Local);
		}
	}

	/// <inheritdoc cref="IFileSystemInfo.LastAccessTimeUtc" />
	public DateTime LastAccessTimeUtc
	{
		get
		{
			using IDisposable registration =
				RegisterProperty(nameof(LastAccessTimeUtc), PropertyAccess.Get);

			return Container.LastAccessTime.Get(DateTimeKind.Utc);
		}
		set
		{
			using IDisposable registration =
				RegisterProperty(nameof(LastAccessTimeUtc), PropertyAccess.Set);

			Container.LastAccessTime.Set(value, DateTimeKind.Utc);
		}
	}

	/// <inheritdoc cref="IFileSystemInfo.LastWriteTime" />
	public DateTime LastWriteTime
	{
		get
		{
			using IDisposable registration =
				RegisterProperty(nameof(LastWriteTime), PropertyAccess.Get);

			return Container.LastWriteTime.Get(DateTimeKind.Local);
		}
		set
		{
			using IDisposable registration =
				RegisterProperty(nameof(LastWriteTime), PropertyAccess.Set);

			Container.LastWriteTime.Set(value, DateTimeKind.Local);
		}
	}

	/// <inheritdoc cref="IFileSystemInfo.LastWriteTimeUtc" />
	public DateTime LastWriteTimeUtc
	{
		get
		{
			using IDisposable registration =
				RegisterProperty(nameof(LastWriteTimeUtc), PropertyAccess.Get);

			return Container.LastWriteTime.Get(DateTimeKind.Utc);
		}
		set
		{
			using IDisposable registration =
				RegisterProperty(nameof(LastWriteTimeUtc), PropertyAccess.Set);

			Container.LastWriteTime.Set(value, DateTimeKind.Utc);
		}
	}

#if FEATURE_FILESYSTEM_LINK
	/// <inheritdoc cref="IFileSystemInfo.LinkTarget" />
	public string? LinkTarget
	{
		get
		{
			using IDisposable registration =
				RegisterProperty(nameof(LinkTarget), PropertyAccess.Get);

			return Container.LinkTarget;
		}
	}
#endif

	/// <inheritdoc cref="IFileSystemInfo.Name" />
	public virtual string Name
	{
		get
		{
			using IDisposable registration = RegisterProperty(nameof(Name), PropertyAccess.Get);

			return string.Equals(
				_fileSystem.Execute.Path.GetPathRoot(Location.FullPath),
				Location.FullPath,
				_fileSystem.Execute.StringComparisonMode)
				? Location.FullPath
				: _fileSystem.Execute.Path.GetFileName(Location.FullPath.TrimEnd(
					_fileSystem.Execute.Path.DirectorySeparatorChar,
					_fileSystem.Execute.Path.AltDirectorySeparatorChar));
		}
	}

#if FEATURE_FILESYSTEM_UNIXFILEMODE
	/// <inheritdoc cref="IFileSystemInfo.UnixFileMode" />
	public UnixFileMode UnixFileMode
	{
		get
		{
			using IDisposable registration =
				RegisterProperty(nameof(UnixFileMode), PropertyAccess.Get);

			return Container.UnixFileMode;
		}
		[UnsupportedOSPlatform("windows")]
		set
		{
			using IDisposable registration =
				RegisterProperty(nameof(UnixFileMode), PropertyAccess.Set);

			_fileSystem.Execute.OnWindows(
				() => throw ExceptionFactory.UnixFileModeNotSupportedOnThisPlatform());

			Container.UnixFileMode = value;
		}
	}
#endif

	/// <inheritdoc cref="IFileSystemInfo.Refresh()" />
	public void Refresh()
	{
		using IDisposable registration = RegisterMethod(nameof(Refresh));

		ResetCache(true);
	}

#if FEATURE_FILESYSTEM_LINK
	/// <inheritdoc cref="IFileSystemInfo.ResolveLinkTarget(bool)" />
	public IFileSystemInfo? ResolveLinkTarget(bool returnFinalTarget)
	{
		using IDisposable registration = RegisterMethod(nameof(ResolveLinkTarget),
			returnFinalTarget);

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
				_fileSystem.Execute.IsWindows ? -2147022975 : -2146232800);
		}
	}
#endif

	#endregion

	/// <inheritdoc cref="IFileSystemExtensibility.TryGetWrappedInstance{T}" />
	public bool TryGetWrappedInstance<T>([NotNullWhen(true)] out T? wrappedInstance)
		=> Container.Extensibility.TryGetWrappedInstance(out wrappedInstance);

	/// <inheritdoc cref="StoreMetadata{T}(string, T)" />
	public void StoreMetadata<T>(string key, T? value)
		=> Container.Extensibility.StoreMetadata(key, value);

	/// <inheritdoc cref="RetrieveMetadata{T}(string)" />
	public T? RetrieveMetadata<T>(string key)
		=> Container.Extensibility.RetrieveMetadata<T>(key);

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

	protected virtual IDisposable RegisterProperty(string name, PropertyAccess access)
		=> new NoOpDisposable();

	protected virtual IDisposable RegisterMethod(string name)
		=> new NoOpDisposable();

	protected virtual IDisposable RegisterMethod<T1>(string name, T1 parameter1)
		=> new NoOpDisposable();
}
