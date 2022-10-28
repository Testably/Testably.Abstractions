using System;
using System.IO;
using Testably.Abstractions.Testing.Internal;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
	private class FileSystemInfoMock : IFileSystemInfo
	{
		protected FileSystemTypes FileSystemType { get; }
		protected IStorageLocation Location;
		protected readonly FileSystemMock FileSystem;

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

		protected FileSystemInfoMock(FileSystemMock fileSystem, IStorageLocation location, FileSystemTypes fileSystemType)
		{
			FileSystem = fileSystem;
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
			if (FileSystem.Storage.TryAddContainer(Location, InMemoryContainer.NewFile,
				out IStorageContainer? container))
			{
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
		public void Delete()
		{
			if (!FileSystem.Storage.DeleteContainer(Location))
			{
				throw ExceptionFactory.DirectoryNotFound(Location.FullPath);
			}

			Refresh();
		}

		/// <inheritdoc cref="IFileSystemInfo.Exists" />
		public virtual bool Exists
		{
			get
			{
				RefreshInternal();
				_exists ??= Container is not NullContainer;
				return _exists.Value;
			}
		}

		/// <inheritdoc cref="IFileSystemInfo.Extension" />
		public string Extension
			=> FileSystem.Path.GetExtension(Location.FullPath);

		/// <inheritdoc cref="IFileSystemInfo.ExtensionContainer" />
		public IFileSystemExtensionContainer ExtensionContainer
			=> Container.ExtensionContainer;

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
			=> FileSystem.Path.GetPathRoot(Location.FullPath) == Location.FullPath
				? Location.FullPath
				: FileSystem.Path.GetFileName(Location.FullPath.TrimEnd(
					FileSystem.Path.DirectorySeparatorChar,
					FileSystem.Path.AltDirectorySeparatorChar));

		/// <inheritdoc cref="IFileSystemInfo.Refresh()" />
		public void Refresh()
		{
#if !NETFRAMEWORK
			// The DirectoryInfo is not updated in .NET Framework!
			_exists = null;
#endif
			_isInitialized = false;
		}

#if FEATURE_FILESYSTEM_LINK
		/// <inheritdoc cref="IFileSystemInfo.ResolveLinkTarget(bool)" />
		public IFileSystemInfo? ResolveLinkTarget(bool returnFinalTarget)
		{
			try
			{
				IStorageLocation? targetLocation =
					FileSystem.Storage.ResolveLinkTarget(
						Location,
						returnFinalTarget);
				if (targetLocation != null)
				{
					return New(targetLocation, FileSystem);
				}

				return null;
			}
			catch (IOException)
			{
				throw ExceptionFactory.FileNameCannotBeResolved(Location.FullPath);
			}
		}
#endif

		#endregion

#if NETSTANDARD2_0
		/// <inheritdoc cref="object.ToString()" />
#else
		/// <inheritdoc cref="System.IO.FileSystemInfo.ToString()" />
#endif
		public override string ToString()
			=> Location.FriendlyName;

		internal static FileSystemInfoMock New(IStorageLocation location,
		                                       FileSystemMock fileSystem)
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

			return new FileSystemInfoMock(fileSystem, location, FileSystemTypes.DirectoryOrFile);
		}

		private void RefreshInternal()
		{
			if (_isInitialized)
			{
				return;
			}

			Container = FileSystem.Storage.GetContainer(Location);
			_isInitialized = true;
		}
	}
}