using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Internal;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    /// <summary>
    ///     A mocked directory in the <see cref="InMemoryStorage" />.
    /// </summary>
    private sealed class DirectoryInfoMock
        : FileSystemInfoMock, IFileSystem.IDirectoryInfo
    {
        private DirectoryInfoMock(IStorageLocation location,
                                  FileSystemMock fileSystem)
            : base(fileSystem, location)
        {
        }

        #region IDirectoryInfo Members

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.Parent" />
        public IFileSystem.IDirectoryInfo? Parent
            => New(Location.GetParent(), FileSystem);

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.Root" />
        public IFileSystem.IDirectoryInfo Root
            => New(FileSystem.Storage.GetLocation(string.Empty.PrefixRoot()),
                FileSystem);

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.Create()" />
        public void Create()
        {
            FullName.ThrowCommonExceptionsIfPathIsInvalid(FileSystem);

            Container = FileSystem.Storage.GetOrCreateContainer(Location,
                InMemoryContainer.NewDirectory);

            Refresh();
        }

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.CreateSubdirectory(string)" />
        public IFileSystem.IDirectoryInfo CreateSubdirectory(string path)
        {
            path.ThrowCommonExceptionsIfPathIsInvalid(FileSystem);

            DirectoryInfoMock directory = New(
                FileSystem.Storage.GetLocation(FileSystem.Path.Combine(FullName, path)),
                FileSystem);
            directory.Create();
            return directory;
        }

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.Delete(bool)" />
        public void Delete(bool recursive)
        {
            if (!FileSystem.Storage.DeleteContainer(
                FileSystem.Storage.GetLocation(FullName), recursive))
            {
                throw ExceptionFactory.DirectoryNotFound(FullName);
            }

            Refresh();
        }

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateDirectories()" />
        public IEnumerable<IFileSystem.IDirectoryInfo> EnumerateDirectories()
            => EnumerateDirectories("*", SearchOption.TopDirectoryOnly);

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateDirectories(string)" />
        public IEnumerable<IFileSystem.IDirectoryInfo>
            EnumerateDirectories(string searchPattern)
            => EnumerateDirectories(searchPattern, SearchOption.TopDirectoryOnly);

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateDirectories(string, SearchOption)" />
        public IEnumerable<IFileSystem.IDirectoryInfo> EnumerateDirectories(
            string searchPattern, SearchOption searchOption)
            => FileSystem.Storage.EnumerateLocations(
                    FileSystem.Storage.GetLocation(FullName),
                    ContainerTypes.Directory,
                    searchPattern,
                    EnumerationOptionsHelper.FromSearchOption(searchOption))
               .Select(location => New(location, FileSystem));

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateDirectories(string, EnumerationOptions)" />
        public IEnumerable<IFileSystem.IDirectoryInfo> EnumerateDirectories(
            string searchPattern,
            EnumerationOptions enumerationOptions)
            => FileSystem.Storage.EnumerateLocations(
                    FileSystem.Storage.GetLocation(FullName),
                    ContainerTypes.Directory,
                    searchPattern,
                    enumerationOptions)
               .Select(location => New(location, FileSystem));
#endif

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFiles()" />
        public IEnumerable<IFileSystem.IFileInfo> EnumerateFiles()
            => EnumerateFiles("*", SearchOption.TopDirectoryOnly);

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFiles(string)" />
        public IEnumerable<IFileSystem.IFileInfo> EnumerateFiles(string searchPattern)
            => EnumerateFiles(searchPattern, SearchOption.TopDirectoryOnly);

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFiles(string, SearchOption)" />
        public IEnumerable<IFileSystem.IFileInfo> EnumerateFiles(
            string searchPattern, SearchOption searchOption)
            => FileSystem.Storage.EnumerateLocations(
                    FileSystem.Storage.GetLocation(FullName),
                    ContainerTypes.File,
                    searchPattern,
                    EnumerationOptionsHelper.FromSearchOption(searchOption))
               .Select(location => FileInfoMock.New(location, FileSystem));

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFiles(string, EnumerationOptions)" />
        public IEnumerable<IFileSystem.IFileInfo> EnumerateFiles(
            string searchPattern, EnumerationOptions enumerationOptions)
            => FileSystem.Storage.EnumerateLocations(
                    FileSystem.Storage.GetLocation(FullName),
                    ContainerTypes.File,
                    searchPattern,
                    enumerationOptions)
               .Select(location => FileInfoMock.New(location, FileSystem));
#endif

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFileSystemInfos()" />
        public IEnumerable<IFileSystem.IFileSystemInfo> EnumerateFileSystemInfos()
            => EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly);

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFileSystemInfos(string)" />
        public IEnumerable<IFileSystem.IFileSystemInfo>
            EnumerateFileSystemInfos(string searchPattern)
            => EnumerateFileSystemInfos(searchPattern, SearchOption.TopDirectoryOnly);

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFileSystemInfos(string, SearchOption)" />
        public IEnumerable<IFileSystem.IFileSystemInfo> EnumerateFileSystemInfos(
            string searchPattern, SearchOption searchOption)
            => FileSystem.Storage.EnumerateLocations(
                    FileSystem.Storage.GetLocation(FullName),
                    ContainerTypes.DirectoryOrFile,
                    searchPattern,
                    EnumerationOptionsHelper.FromSearchOption(searchOption))
               .Select(location => FileSystemInfoMock.New(location, FileSystem));

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFileSystemInfos(string, EnumerationOptions)" />
        public IEnumerable<IFileSystem.IFileSystemInfo> EnumerateFileSystemInfos(
            string searchPattern,
            EnumerationOptions enumerationOptions)
            => FileSystem.Storage.EnumerateLocations(
                    FileSystem.Storage.GetLocation(FullName),
                    ContainerTypes.DirectoryOrFile,
                    searchPattern,
                    enumerationOptions)
               .Select(location => FileSystemInfoMock.New(location, FileSystem));
#endif

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetDirectories()" />
        public IFileSystem.IDirectoryInfo[] GetDirectories()
            => EnumerateDirectories().ToArray();

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetDirectories(string)" />
        public IFileSystem.IDirectoryInfo[] GetDirectories(string searchPattern)
            => EnumerateDirectories(searchPattern).ToArray();

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetDirectories(string, SearchOption)" />
        public IFileSystem.IDirectoryInfo[] GetDirectories(
            string searchPattern, SearchOption searchOption)
            => EnumerateDirectories(searchPattern, searchOption).ToArray();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetDirectories(string, EnumerationOptions)" />
        public IFileSystem.IDirectoryInfo[] GetDirectories(
            string searchPattern,
            EnumerationOptions enumerationOptions)
            => EnumerateDirectories(searchPattern, enumerationOptions).ToArray();
#endif

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFiles()" />
        public IFileSystem.IFileInfo[] GetFiles()
            => EnumerateFiles().ToArray();

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFiles(string)" />
        public IFileSystem.IFileInfo[] GetFiles(string searchPattern)
            => EnumerateFiles(searchPattern).ToArray();

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFiles(string, SearchOption)" />
        public IFileSystem.IFileInfo[] GetFiles(string searchPattern,
                                                SearchOption searchOption)
            => EnumerateFiles(searchPattern, searchOption).ToArray();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFiles(string, EnumerationOptions)" />
        public IFileSystem.IFileInfo[] GetFiles(string searchPattern,
                                                EnumerationOptions enumerationOptions)
            => EnumerateFiles(searchPattern, enumerationOptions).ToArray();
#endif

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFileSystemInfos()" />
        public IFileSystem.IFileSystemInfo[] GetFileSystemInfos()
            => EnumerateFileSystemInfos().ToArray();

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFileSystemInfos(string)" />
        public IFileSystem.IFileSystemInfo[] GetFileSystemInfos(string searchPattern)
            => EnumerateFileSystemInfos(searchPattern).ToArray();

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFileSystemInfos(string, SearchOption)" />
        public IFileSystem.IFileSystemInfo[] GetFileSystemInfos(
            string searchPattern, SearchOption searchOption)
            => EnumerateFileSystemInfos(searchPattern, searchOption).ToArray();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFileSystemInfos(string, EnumerationOptions)" />
        public IFileSystem.IFileSystemInfo[] GetFileSystemInfos(
            string searchPattern,
            EnumerationOptions enumerationOptions)
            => EnumerateFileSystemInfos(searchPattern, enumerationOptions).ToArray();
#endif

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.MoveTo(string)" />
        public void MoveTo(string destDirName)
            => throw new NotImplementedException();

        #endregion

        [return: NotNullIfNotNull("location")]
        internal static new DirectoryInfoMock? New(IStorageLocation? location,
                                                   FileSystemMock fileSystem)
        {
            if (location == null)
            {
                return null;
            }

            return new DirectoryInfoMock(location, fileSystem);
        }
    }
}