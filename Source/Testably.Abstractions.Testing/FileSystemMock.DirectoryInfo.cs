using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    /// <summary>
    ///     A mocked directory in the <see cref="InMemoryFileSystem" />.
    /// </summary>
    private sealed class DirectoryInfoMock : FileSystemInfoMock,
        IFileSystem.IDirectoryInfo
    {
        internal DirectoryInfoMock(string fullName, string originalPath, FileSystemMock fileSystem)
            : base(fullName, originalPath, fileSystem)
        {
        }

        #region IDirectoryInfo Members

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.Parent" />
        public IFileSystem.IDirectoryInfo? Parent
            => CreateParent(this, FileSystem);

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.Root" />
        public IFileSystem.IDirectoryInfo Root
            => New(string.Empty.PrefixRoot(), FileSystem);

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.Create()" />
        public void Create()
        {
            FileSystem.Directory.CreateDirectory(FullName);
            ResetExists();
        }

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.CreateSubdirectory(string)" />
        public IFileSystem.IDirectoryInfo CreateSubdirectory(string path)
            => FileSystem.Directory.CreateDirectory(
                FileSystem.Path.Combine(FullName, path));

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.Delete(bool)" />
        public void Delete(bool recursive)
        {
            FileSystem.FileSystemContainer.Delete(FullName, recursive);
            ResetExists();
        }

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateDirectories()" />
        public IEnumerable<IFileSystem.IDirectoryInfo> EnumerateDirectories()
            => FileSystem.FileSystemContainer.Enumerate<IFileSystem.IDirectoryInfo>(
                FullName,
                "*", EnumerationOptionsHelper.Compatible);

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateDirectories(string)" />
        public IEnumerable<IFileSystem.IDirectoryInfo>
            EnumerateDirectories(string searchPattern)
            => FileSystem.FileSystemContainer.Enumerate<IFileSystem.IDirectoryInfo>(
                FullName,
                searchPattern, EnumerationOptionsHelper.Compatible);

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateDirectories(string, SearchOption)" />
        public IEnumerable<IFileSystem.IDirectoryInfo> EnumerateDirectories(
            string searchPattern, SearchOption searchOption)
            => FileSystem.FileSystemContainer.Enumerate<IFileSystem.IDirectoryInfo>(
                FullName,
                searchPattern, EnumerationOptionsHelper.FromSearchOption(searchOption));

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateDirectories(string, EnumerationOptions)" />
        public IEnumerable<IFileSystem.IDirectoryInfo> EnumerateDirectories(
            string searchPattern,
            EnumerationOptions enumerationOptions)
            => FileSystem.FileSystemContainer.Enumerate<IFileSystem.IDirectoryInfo>(
                FullName,
                searchPattern, enumerationOptions);
#endif

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFiles()" />
        public IEnumerable<IFileSystem.IFileInfo> EnumerateFiles()
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFiles(string)" />
        public IEnumerable<IFileSystem.IFileInfo> EnumerateFiles(string searchPattern)
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFiles(string, SearchOption)" />
        public IEnumerable<IFileSystem.IFileInfo> EnumerateFiles(
            string searchPattern, SearchOption searchOption)
            => throw new NotImplementedException();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFiles(string, EnumerationOptions)" />
        public IEnumerable<IFileSystem.IFileInfo> EnumerateFiles(
            string searchPattern, EnumerationOptions enumerationOptions)
            => throw new NotImplementedException();
#endif

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFileSystemInfos()" />
        public IEnumerable<IFileSystem.IFileSystemInfo> EnumerateFileSystemInfos()
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFileSystemInfos(string)" />
        public IEnumerable<IFileSystem.IFileSystemInfo>
            EnumerateFileSystemInfos(string searchPattern)
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFileSystemInfos(string, SearchOption)" />
        public IEnumerable<IFileSystem.IFileSystemInfo> EnumerateFileSystemInfos(
            string searchPattern, SearchOption searchOption)
            => throw new NotImplementedException();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.EnumerateFileSystemInfos(string, EnumerationOptions)" />
        public IEnumerable<IFileSystem.IFileSystemInfo> EnumerateFileSystemInfos(
            string searchPattern,
            EnumerationOptions enumerationOptions)
            => throw new NotImplementedException();
#endif

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetDirectories()" />
        public IFileSystem.IDirectoryInfo[] GetDirectories()
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetDirectories(string)" />
        public IFileSystem.IDirectoryInfo[] GetDirectories(string searchPattern)
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetDirectories(string, SearchOption)" />
        public IFileSystem.IDirectoryInfo[] GetDirectories(
            string searchPattern, SearchOption searchOption)
            => throw new NotImplementedException();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetDirectories(string, EnumerationOptions)" />
        public IFileSystem.IDirectoryInfo[] GetDirectories(string searchPattern,
                                                           EnumerationOptions
                                                               enumerationOptions)
            => throw new NotImplementedException();
#endif

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFiles()" />
        public IFileSystem.IFileInfo[] GetFiles()
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFiles(string)" />
        public IFileSystem.IFileInfo[] GetFiles(string searchPattern)
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFiles(string, SearchOption)" />
        public IFileSystem.IFileInfo[] GetFiles(string searchPattern,
                                                SearchOption searchOption)
            => throw new NotImplementedException();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFiles(string, EnumerationOptions)" />
        public IFileSystem.IFileInfo[] GetFiles(string searchPattern,
                                                EnumerationOptions enumerationOptions)
            => throw new NotImplementedException();
#endif

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFileSystemInfos()" />
        public IFileSystem.IFileSystemInfo[] GetFileSystemInfos()
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFileSystemInfos(string)" />
        public IFileSystem.IFileSystemInfo[] GetFileSystemInfos(string searchPattern)
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFileSystemInfos(string, SearchOption)" />
        public IFileSystem.IFileSystemInfo[] GetFileSystemInfos(
            string searchPattern, SearchOption searchOption)
            => throw new NotImplementedException();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.GetFileSystemInfos(string, EnumerationOptions)" />
        public IFileSystem.IFileSystemInfo[] GetFileSystemInfos(string searchPattern,
            EnumerationOptions enumerationOptions)
            => throw new NotImplementedException();
#endif

        /// <inheritdoc cref="IFileSystem.IDirectoryInfo.MoveTo(string)" />
        public void MoveTo(string destDirName)
            => throw new NotImplementedException();

        #endregion

        [return: NotNullIfNotNull("path")]
        internal static DirectoryInfoMock? New(string? path, FileSystemMock fileSystem)
        {
            if (path == null)
            {
                return null;
            }

            if (path == string.Empty)
            {
#if NETFRAMEWORK
                throw new ArgumentException("The path is not of a legal form.");
#else
                throw new ArgumentException("The path is empty.", nameof(path));
#endif
            }

            var originalPath = path;
            var fullName = fileSystem.Path.GetFullPath(path).NormalizePath().TrimOnWindows();
            return new DirectoryInfoMock(fullName, originalPath, fileSystem);
        }

        [return: NotNullIfNotNull("path")]
        internal static DirectoryInfoMock? New(string? path, string originalpath, FileSystemMock fileSystem)
        {
            if (path == null)
            {
                return null;
            }

            if (path == string.Empty)
            {
#if NETFRAMEWORK
                throw new ArgumentException("The path is not of a legal form.");
#else
                throw new ArgumentException("The path is empty.", nameof(originalpath));
#endif
            }

            var originalPath = originalpath;
            path = fileSystem.Path.GetFullPath(path).NormalizePath()
               .TrimOnWindows();
            return new DirectoryInfoMock(path, originalPath, fileSystem);
        }

        private static IFileSystem.IDirectoryInfo CreateParent(
            DirectoryInfoMock child, FileSystemMock fileSystem)
        {
            var parentPath = fileSystem.Path.GetDirectoryName(child.FullName);
            if (parentPath == null)
            {
                return child.Root;
            }
            return New(parentPath, parentPath, fileSystem);
        }
    }
}