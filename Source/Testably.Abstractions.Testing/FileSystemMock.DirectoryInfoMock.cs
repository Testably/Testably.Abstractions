using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    /// <summary>
    ///     A mocked directory in the <see cref="InMemoryStorage" />.
    /// </summary>
    private sealed class DirectoryInfoMock : FileSystemInfoMock,
        IStorage.IDirectoryInfoMock
    {
        private DirectoryInfoMock(InMemoryLocation location,
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
            => New(InMemoryLocation.New(FileSystem, string.Empty.PrefixRoot()), FileSystem);

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.Create()" />
        public void Create()
        {
            FullName.ThrowCommonExceptionsIfPathIsInvalid(FileSystem);
            
            Container = FileSystem.Storage.GetOrCreateContainer(
                Location, InMemoryContainer.NewDirectory);
            Refresh();
        }

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.CreateSubdirectory(string)" />
        public IFileSystem.IDirectoryInfo CreateSubdirectory(string path)
            => FileSystem.Directory.CreateDirectory(
                FileSystem.Path.Combine(FullName, path));

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.Delete(bool)" />
        public void Delete(bool recursive)
        {
            FileSystem.Storage.Delete(FullName, recursive);
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
            => FileSystem.Storage.Enumerate(
                InMemoryLocation.New(FileSystem, FullName),
                InMemoryContainer.ContainerType.Directory,
                searchPattern,
                EnumerationOptionsHelper.FromSearchOption(searchOption),
                DirectoryNotFoundException(FullName),
                DirectoryInfoMock.New);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateDirectories(string, EnumerationOptions)" />
        public IEnumerable<IFileSystem.IDirectoryInfo> EnumerateDirectories(
            string searchPattern,
            EnumerationOptions enumerationOptions)
            => FileSystem.Storage.Enumerate<IFileSystem.IDirectoryInfo>(
                FullName,
                searchPattern,
                enumerationOptions,
                DirectoryNotFoundException(FullName));
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
            => FileSystem.Storage.Enumerate(
                InMemoryLocation.New(FileSystem, FullName),
                InMemoryContainer.ContainerType.File,
                searchPattern,
                EnumerationOptionsHelper.FromSearchOption(searchOption),
                DirectoryNotFoundException(FullName),
                FileInfoMock.New);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFiles(string, EnumerationOptions)" />
        public IEnumerable<IFileSystem.IFileInfo> EnumerateFiles(
            string searchPattern, EnumerationOptions enumerationOptions)
            => FileSystem.Storage.Enumerate<IFileSystem.IFileInfo>(
                FullName,
                searchPattern,
                enumerationOptions,
                DirectoryNotFoundException(FullName));
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
            => FileSystem.Storage.Enumerate<IFileSystem.IFileSystemInfo>(
                FullName,
                searchPattern,
                EnumerationOptionsHelper.FromSearchOption(searchOption),
                DirectoryNotFoundException(FullName));

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFileSystemInfos(string, EnumerationOptions)" />
        public IEnumerable<IFileSystem.IFileSystemInfo> EnumerateFileSystemInfos(
            string searchPattern,
            EnumerationOptions enumerationOptions)
            => FileSystem.Storage.Enumerate<IFileSystem.IFileSystemInfo>(
                FullName,
                searchPattern,
                enumerationOptions,
                DirectoryNotFoundException(FullName));
#endif

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetDirectories()" />
        public IFileSystem.IDirectoryInfo[] GetDirectories()
            => GetDirectories("*", SearchOption.TopDirectoryOnly);

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetDirectories(string)" />
        public IFileSystem.IDirectoryInfo[] GetDirectories(string searchPattern)
            => GetDirectories(searchPattern, SearchOption.TopDirectoryOnly);

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
            => GetFiles("*", SearchOption.TopDirectoryOnly);

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFiles(string)" />
        public IFileSystem.IFileInfo[] GetFiles(string searchPattern)
            => GetFiles(searchPattern, SearchOption.TopDirectoryOnly);

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
            => GetFileSystemInfos("*", SearchOption.TopDirectoryOnly);

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFileSystemInfos(string)" />
        public IFileSystem.IFileSystemInfo[] GetFileSystemInfos(string searchPattern)
            => GetFileSystemInfos(searchPattern, SearchOption.TopDirectoryOnly);

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
        internal static DirectoryInfoMock? New(InMemoryLocation? location, FileSystemMock fileSystem)
        {
            if (location == null)
            {
                return null;
            }
            return new DirectoryInfoMock(location, fileSystem);
        }

        private static Func<Exception> DirectoryNotFoundException(string path)
        {
            return () => new DirectoryNotFoundException(
                $"Could not find a part of the path '{path}'.");
        }
    }
}