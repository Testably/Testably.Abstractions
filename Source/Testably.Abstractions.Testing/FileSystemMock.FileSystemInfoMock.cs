using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Testably.Abstractions.Testing.Internal;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    private class FileSystemInfoMock : IFileSystem.IFileSystemInfo
    {
        protected readonly IStorageLocation Location;
        protected readonly FileSystemMock FileSystem;
        protected IStorageContainer Container { get; set; }

        private bool? _exists;
        private bool _isInitialized;

        internal FileSystemInfoMock(FileSystemMock fileSystem, IStorageLocation location)
        {
            FileSystem = fileSystem;
            Location = location;
            Container = fileSystem.Storage.GetContainer(location);
        }

        #region IFileSystemInfo Members

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Attributes" />
        public FileAttributes Attributes
        {
            get => Container.Attributes;
            set => Container.Attributes = value;
        }

#if FEATURE_FILESYSTEM_LINK
        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.CreateAsSymbolicLink(string)" />
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

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.CreationTime" />
        public DateTime CreationTime
        {
            get => Container.CreationTime.ToLocalTime();
            set => Container.CreationTime = value;
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.CreationTimeUtc" />
        public DateTime CreationTimeUtc
        {
            get => Container.CreationTime.ToUniversalTime();
            set => Container.CreationTime = value;
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Delete()" />
        public void Delete()
        {
            if (!FileSystem.Storage.DeleteContainer(
                FileSystem.Storage.GetLocation(FullName)))
            {
                throw ExceptionFactory.DirectoryNotFound(FullName);
            }

            Refresh();
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Exists" />
        public virtual bool Exists
        {
            get
            {
                RefreshInternal();
                _exists ??= Container is not NullContainer;
                return _exists.Value;
            }
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Extension" />
        public string Extension
            => FileSystem.Path.GetExtension(FullName);

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.FullName" />
        public string FullName => Location.FullPath;

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastAccessTime" />
        public DateTime LastAccessTime
        {
            get => Container.LastAccessTime.ToLocalTime();
            set => Container.LastAccessTime = value;
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastAccessTimeUtc" />
        public DateTime LastAccessTimeUtc
        {
            get => Container.LastAccessTime.ToUniversalTime();
            set => Container.LastAccessTime = value;
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastWriteTime" />
        public DateTime LastWriteTime
        {
            get => Container.LastWriteTime.ToLocalTime();
            set => Container.LastWriteTime = value;
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastWriteTimeUtc" />
        public DateTime LastWriteTimeUtc
        {
            get => Container.LastWriteTime.ToUniversalTime();
            set => Container.LastWriteTime = value;
        }

#if FEATURE_FILESYSTEM_LINK
        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LinkTarget" />
        public string? LinkTarget
            => Container.LinkTarget;
#endif

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Name" />
        public string Name
            => FileSystem.Path.GetFileName(FullName.TrimEnd(
                FileSystem.Path.DirectorySeparatorChar,
                FileSystem.Path.AltDirectorySeparatorChar));
        
        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Refresh()" />
        public void Refresh()
        {
#if !NETFRAMEWORK
            // The DirectoryInfo is not updated in .NET Framework!
            _exists = null;
#endif
            _isInitialized = false;
        }

#if FEATURE_FILESYSTEM_LINK
        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.ResolveLinkTarget(bool)" />
        public IFileSystem.IFileSystemInfo? ResolveLinkTarget(bool returnFinalTarget)
        {
            try
            {
                IStorageLocation? targetLocation =
                    FileSystem.Storage.ResolveLinkTarget(
                        FileSystem.Storage.GetLocation(FullName),
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

        [return: NotNullIfNotNull("location")]
        internal static FileSystemInfoMock? New(IStorageLocation? location,
                                                FileSystemMock fileSystem)
        {
            if (location == null)
            {
                return null;
            }

            return new FileSystemInfoMock(fileSystem, location);
        }

        protected void RefreshInternal()
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