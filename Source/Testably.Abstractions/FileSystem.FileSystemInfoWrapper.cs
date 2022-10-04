using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions;

public sealed partial class FileSystem
{
    private class FileSystemInfoWrapper : IFileSystem.IFileSystemInfo
    {
        private readonly FileSystemInfo _instance;
        private readonly IFileSystem _fileSystem;

        internal FileSystemInfoWrapper(FileSystemInfo instance, IFileSystem fileSystem)
        {
            _instance = instance;
            _fileSystem = fileSystem;
        }

        #region IFileSystemInfo Members

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Attributes" />
        public FileAttributes Attributes
        {
            get => _instance.Attributes;
            set => _instance.Attributes = value;
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.CreationTime" />
        public DateTime CreationTime
        {
            get => _instance.CreationTime;
            set => _instance.CreationTime = value;
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.CreationTimeUtc" />
        public DateTime CreationTimeUtc
        {
            get => _instance.CreationTimeUtc;
            set => _instance.CreationTimeUtc = value;
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Exists" />
        public bool Exists
            => _instance.Exists;

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Extension" />
        public string Extension
            => _instance.Extension;

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.FullName" />
        public string FullName
            => _instance.FullName;

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastAccessTime" />
        public DateTime LastAccessTime
        {
            get => _instance.LastAccessTime;
            set => _instance.LastAccessTime = value;
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastAccessTimeUtc" />
        public DateTime LastAccessTimeUtc
        {
            get => _instance.LastAccessTimeUtc;
            set => _instance.LastAccessTimeUtc = value;
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastWriteTime" />
        public DateTime LastWriteTime
        {
            get => _instance.LastWriteTime;
            set => _instance.LastWriteTime = value;
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LastWriteTimeUtc" />
        public DateTime LastWriteTimeUtc
        {
            get => _instance.LastWriteTimeUtc;
            set => _instance.LastWriteTimeUtc = value;
        }

#if FEATURE_FILESYSTEM_LINK
        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.LinkTarget" />
        public string? LinkTarget
            => _instance.LinkTarget;
#endif

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Name" />
        public string Name
            => _instance.Name;

#if FEATURE_FILESYSTEM_LINK
        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.CreateAsSymbolicLink(string)" />
        public void CreateAsSymbolicLink(string pathToTarget)
            => _instance.CreateAsSymbolicLink(pathToTarget);
#endif

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Delete()" />
        public void Delete()
            => _instance.Delete();

        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.Refresh()" />
        public void Refresh()
            => _instance.Refresh();

#if FEATURE_FILESYSTEM_LINK
        /// <inheritdoc cref="IFileSystem.IFileSystemInfo.ResolveLinkTarget(bool)" />
        public IFileSystem.IFileSystemInfo? ResolveLinkTarget(bool returnFinalTarget)
            => FromFileSystemInfo(_instance.ResolveLinkTarget(returnFinalTarget),
                _fileSystem);
#endif

        #endregion

#if NETSTANDARD2_0
/// <inheritdoc cref="object.ToString()" />
#else
        /// <inheritdoc cref="FileSystemInfo.ToString()" />
#endif
        public override string ToString()
            => _instance.ToString();

        [return: NotNullIfNotNull("instance")]
        internal static FileSystemInfoWrapper? FromFileSystemInfo(
            FileSystemInfo? instance,
            IFileSystem fileSystem)
        {
            if (instance == null)
            {
                return null;
            }

            return new FileSystemInfoWrapper(instance, fileSystem);
        }
    }
}