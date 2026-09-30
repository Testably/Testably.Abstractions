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

	private IStorageContainer _container;
	private bool _isInitialized;
#if FEATURE_FILESYSTEM_LINK
	private bool _isLinkTargetCached;
	private string? _linkTarget;
#endif
	private CachedState? _state;

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
				RegisterPathProperty(nameof(Attributes), PropertyAccess.Get);

			return State.Attributes;
		}
		set
		{
			using IDisposable registration =
				RegisterPathProperty(nameof(Attributes), PropertyAccess.Set);

			_fileSystem.Storage.GetContainer(Location).Attributes = value;
			ResetCache(true);
		}
	}

#if FEATURE_FILESYSTEM_LINK
	/// <inheritdoc cref="IFileSystemInfo.CreateAsSymbolicLink(string)" />
	public void CreateAsSymbolicLink(string pathToTarget)
	{
		using IDisposable registration = RegisterPathMethod(nameof(CreateAsSymbolicLink),
			pathToTarget);

		if (!_fileSystem.Execute.IsWindows && string.IsNullOrWhiteSpace(FullName))
		{
			return;
		}

		FullName.EnsureValidFormat(_fileSystem);
		pathToTarget.ThrowCommonExceptionsIfPathToTargetIsInvalid(_fileSystem);
		if (_fileSystem.Storage.TryAddContainer(Location,
			FileSystemType == FileSystemTypes.Directory
				? InMemoryContainer.NewDirectory
				: InMemoryContainer.NewFile,
			out IStorageContainer? container))
		{
			Container = container;
			container.LinkTarget = pathToTarget;
			ResetCache(true);
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
				RegisterPathProperty(nameof(CreationTime), PropertyAccess.Get);

			return State.CreationTimeLocal;
		}
		set
		{
			using IDisposable registration =
				RegisterPathProperty(nameof(CreationTime), PropertyAccess.Set);

			_fileSystem.Storage.GetContainer(Location).CreationTime.Set(value, DateTimeKind.Local);
			ResetCache(true);
		}
	}

	/// <inheritdoc cref="IFileSystemInfo.CreationTimeUtc" />
	public DateTime CreationTimeUtc
	{
		get
		{
			using IDisposable registration =
				RegisterPathProperty(nameof(CreationTimeUtc), PropertyAccess.Get);

			return State.CreationTimeUtc;
		}
		set
		{
			using IDisposable registration =
				RegisterPathProperty(nameof(CreationTimeUtc), PropertyAccess.Set);

			_fileSystem.Storage.GetContainer(Location).CreationTime.Set(value, DateTimeKind.Utc);
			ResetCache(true);
		}
	}

	/// <inheritdoc cref="IFileSystemInfo.Delete()" />
	public virtual void Delete()
	{
		using IDisposable registration = RegisterPathMethod(nameof(Delete));

		_fileSystem.Storage.DeleteContainer(Location, FileSystemType);
		ResetCache(!_fileSystem.Execute.IsNetFramework);
	}

	/// <inheritdoc cref="IFileSystemInfo.Exists" />
	public virtual bool Exists
		=> State.Exists;

	/// <inheritdoc cref="IFileSystemInfo.Extension" />
	public string Extension
	{
		get
		{
			using IDisposable registration =
				RegisterPathProperty(nameof(Extension), PropertyAccess.Get);

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
			using IDisposable registration =
				RegisterPathProperty(nameof(FullName), PropertyAccess.Get);

			return Location.FullPath;
		}
	}

	/// <inheritdoc cref="IFileSystemInfo.LastAccessTime" />
	public DateTime LastAccessTime
	{
		get
		{
			using IDisposable registration =
				RegisterPathProperty(nameof(LastAccessTime), PropertyAccess.Get);

			return State.LastAccessTimeLocal;
		}
		set
		{
			using IDisposable registration =
				RegisterPathProperty(nameof(LastAccessTime), PropertyAccess.Set);

			_fileSystem.Storage.GetContainer(Location).LastAccessTime.Set(value, DateTimeKind.Local);
			ResetCache(true);
		}
	}

	/// <inheritdoc cref="IFileSystemInfo.LastAccessTimeUtc" />
	public DateTime LastAccessTimeUtc
	{
		get
		{
			using IDisposable registration =
				RegisterPathProperty(nameof(LastAccessTimeUtc), PropertyAccess.Get);

			return State.LastAccessTimeUtc;
		}
		set
		{
			using IDisposable registration =
				RegisterPathProperty(nameof(LastAccessTimeUtc), PropertyAccess.Set);

			_fileSystem.Storage.GetContainer(Location).LastAccessTime.Set(value, DateTimeKind.Utc);
			ResetCache(true);
		}
	}

	/// <inheritdoc cref="IFileSystemInfo.LastWriteTime" />
	public DateTime LastWriteTime
	{
		get
		{
			using IDisposable registration =
				RegisterPathProperty(nameof(LastWriteTime), PropertyAccess.Get);

			return State.LastWriteTimeLocal;
		}
		set
		{
			using IDisposable registration =
				RegisterPathProperty(nameof(LastWriteTime), PropertyAccess.Set);

			_fileSystem.Storage.GetContainer(Location).LastWriteTime.Set(value, DateTimeKind.Local);
			ResetCache(true);
		}
	}

	/// <inheritdoc cref="IFileSystemInfo.LastWriteTimeUtc" />
	public DateTime LastWriteTimeUtc
	{
		get
		{
			using IDisposable registration =
				RegisterPathProperty(nameof(LastWriteTimeUtc), PropertyAccess.Get);

			return State.LastWriteTimeUtc;
		}
		set
		{
			using IDisposable registration =
				RegisterPathProperty(nameof(LastWriteTimeUtc), PropertyAccess.Set);

			_fileSystem.Storage.GetContainer(Location).LastWriteTime.Set(value, DateTimeKind.Utc);
			ResetCache(true);
		}
	}

#if FEATURE_FILESYSTEM_LINK
	/// <inheritdoc cref="IFileSystemInfo.LinkTarget" />
	public string? LinkTarget
	{
		get
		{
			using IDisposable registration =
				RegisterPathProperty(nameof(LinkTarget), PropertyAccess.Get);

			if (!_isLinkTargetCached)
			{
				_linkTarget = _fileSystem.Storage.GetContainer(Location).LinkTarget;
				_isLinkTargetCached = true;
			}

			return _linkTarget;
		}
	}
#endif

	/// <inheritdoc cref="IFileSystemInfo.Name" />
	public virtual string Name
	{
		get
		{
			using IDisposable registration = RegisterPathProperty(nameof(Name), PropertyAccess.Get);

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
				RegisterPathProperty(nameof(UnixFileMode), PropertyAccess.Get);

			return State.UnixFileMode;
		}
		[UnsupportedOSPlatform("windows")]
		set
		{
			using IDisposable registration =
				RegisterPathProperty(nameof(UnixFileMode), PropertyAccess.Set);

			if (_fileSystem.Execute.IsWindows)
			{
				throw ExceptionFactory.UnixFileModeNotSupportedOnThisPlatform();
			}

			_fileSystem.Storage.GetContainer(Location).UnixFileMode = value;
			ResetCache(true);
		}
	}
#endif

	/// <inheritdoc cref="IFileSystemInfo.Refresh()" />
	public void Refresh()
	{
		using IDisposable registration = RegisterPathMethod(nameof(Refresh));

		ResetCache(true);
		_state = CaptureState();
	}

#if FEATURE_FILESYSTEM_LINK
	/// <inheritdoc cref="IFileSystemInfo.ResolveLinkTarget(bool)" />
	public IFileSystemInfo? ResolveLinkTarget(bool returnFinalTarget)
	{
		using IDisposable registration = RegisterPathMethod(
			nameof(ResolveLinkTarget), returnFinalTarget
		);

		IStorageLocation? targetLocation
			= _fileSystem.Storage.ResolveLinkTarget(Location, returnFinalTarget);

		return targetLocation != null ? New(targetLocation, _fileSystem) : null;
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

#pragma warning disable MA0202 // Branches differ only in XML doc comments
#if NETSTANDARD2_0
	/// <inheritdoc cref="object.ToString()" />
#else
	/// <inheritdoc cref="FileSystemInfo.ToString()" />
#endif
#pragma warning restore MA0202
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

	/// <summary>
	///     The state of the file or directory, captured on first access and kept until <see cref="Refresh()" />
	///     or an operation that invalidates it, like <see cref="FileSystemInfo" /> does.
	/// </summary>
	protected CachedState State
		=> _state ??= CaptureState();

	/// <summary>
	///     Captures the state now, as <see cref="FileSystemInfo" /> instances returned from an enumeration
	///     already carry the state found during the enumeration.
	/// </summary>
	internal void InitializeState()
		=> _state = CaptureState();

	protected void ResetCache(bool resetState)
	{
		if (resetState)
		{
			_state = null;
#if FEATURE_FILESYSTEM_LINK
			_isLinkTargetCached = false;
			_linkTarget = null;
#endif
		}

		_isInitialized = false;
	}

	private CachedState CaptureState()
	{
		RefreshInternal();
		return new CachedState(Container,
			!string.IsNullOrWhiteSpace(Location.FriendlyName) &&
			Container is not NullContainer);
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

	protected virtual IDisposable RegisterPathProperty(string name, PropertyAccess access)
		=> new NoOpDisposable();

	protected virtual IDisposable RegisterPathMethod(string name)
		=> new NoOpDisposable();

	protected virtual IDisposable RegisterPathMethod<T1>(string name, T1 parameter1)
		=> new NoOpDisposable();

	protected sealed class CachedState
	{
		public FileAttributes Attributes { get; }
		public DateTime CreationTimeLocal { get; }
		public DateTime CreationTimeUtc { get; }
		public bool Exists { get; }
		public DateTime LastAccessTimeLocal { get; }
		public DateTime LastAccessTimeUtc { get; }
		public DateTime LastWriteTimeLocal { get; }
		public DateTime LastWriteTimeUtc { get; }
		public long Length { get; }
		public FileSystemTypes Type { get; }
#if FEATURE_FILESYSTEM_UNIXFILEMODE
		public UnixFileMode UnixFileMode { get; }
#endif

		public CachedState(IStorageContainer container, bool exists)
		{
			Exists = exists;
			Type = container.Type;
			Attributes = container.Attributes;
			CreationTimeLocal = container.CreationTime.Get(DateTimeKind.Local);
			CreationTimeUtc = container.CreationTime.Get(DateTimeKind.Utc);
			LastAccessTimeLocal = container.LastAccessTime.Get(DateTimeKind.Local);
			LastAccessTimeUtc = container.LastAccessTime.Get(DateTimeKind.Utc);
			LastWriteTimeLocal = container.LastWriteTime.Get(DateTimeKind.Local);
			LastWriteTimeUtc = container.LastWriteTime.Get(DateTimeKind.Utc);
			Length = container.GetBytes().Length;
#if FEATURE_FILESYSTEM_UNIXFILEMODE
			UnixFileMode = container.UnixFileMode;
#endif
		}
	}
}
