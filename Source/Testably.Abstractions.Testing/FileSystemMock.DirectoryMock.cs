using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    private sealed class DirectoryMock : IFileSystem.IDirectory
    {
        private readonly FileSystemMock _fileSystem;

        internal DirectoryMock(FileSystemMock fileSystem)
        {
            _fileSystem = fileSystem;
        }

        #region IDirectory Members

        /// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
        public IFileSystem FileSystem
            => _fileSystem;

        /// <inheritdoc cref="IFileSystem.IDirectory.CreateDirectory(string)" />
        public IFileSystem.IDirectoryInfo CreateDirectory(string path)
            => CreateDirectoryInternal(path);

#if FEATURE_FILESYSTEM_LINK
        /// <inheritdoc cref="IFileSystem.IDirectory.CreateSymbolicLink(string, string)" />
        public IFileSystem.IFileSystemInfo CreateSymbolicLink(
            string path, string pathToTarget)
            => throw new NotImplementedException();
#endif

        /// <inheritdoc cref="IFileSystem.IDirectory.Delete(string)" />
        public void Delete(string path)
        {
            if (!_fileSystem.Storage.Delete(path))
            {
                throw ExceptionFactory.DirectoryNotFound(
                    _fileSystem.Path.GetFullPath(path));
            }
        }

        /// <inheritdoc cref="IFileSystem.IDirectory.Delete(string, bool)" />
        public void Delete(string path, bool recursive)
        {
            if (!_fileSystem.Storage.Delete(path, recursive))
            {
                throw ExceptionFactory.DirectoryNotFound(
                    _fileSystem.Path.GetFullPath(path));
            }
        }

        /// <inheritdoc cref="IFileSystem.IDirectory.EnumerateDirectories(string)" />
        public IEnumerable<string> EnumerateDirectories(string path)
            => EnumerateDirectories(path, "*");

        /// <inheritdoc cref="IFileSystem.IDirectory.EnumerateDirectories(string, string)" />
        public IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
            => _fileSystem.Storage.Enumerate<IFileSystem.IDirectoryInfo>(
                    path,
                    searchPattern,
                    EnumerationOptionsHelper.Compatible,
                    DirectoryNotFoundException(_fileSystem.Path.GetFullPath(path)))
               .Select(x => _fileSystem.Storage.GetSubdirectoryPath(
                    x.FullName,
                    path));

        /// <inheritdoc cref="IFileSystem.IDirectory.EnumerateDirectories(string, string, SearchOption)" />
        public IEnumerable<string> EnumerateDirectories(string path,
                                                        string searchPattern,
                                                        SearchOption searchOption)
            => _fileSystem.Storage
               .Enumerate<IFileSystem.IDirectoryInfo>(
                    path,
                    searchPattern,
                    EnumerationOptionsHelper.FromSearchOption(searchOption),
                    DirectoryNotFoundException(_fileSystem.Path.GetFullPath(path)))
               .Select(x => _fileSystem.Storage.GetSubdirectoryPath(
                    x.FullName,
                    path));

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="IFileSystem.IDirectory.EnumerateDirectories(string, string, EnumerationOptions)" />
        public IEnumerable<string> EnumerateDirectories(string path,
                                                        string searchPattern,
                                                        EnumerationOptions
                                                            enumerationOptions)
            => _fileSystem.Storage
               .Enumerate<IFileSystem.IDirectoryInfo>(
                    path,
                    searchPattern,
                    enumerationOptions,
                    DirectoryNotFoundException(_fileSystem.Path.GetFullPath(path)))
               .Select(x => _fileSystem.Storage.GetSubdirectoryPath(
                    x.FullName,
                    path));
#endif

        /// <inheritdoc cref="IFileSystem.IDirectory.EnumerateFiles(string)" />
        public IEnumerable<string> EnumerateFiles(string path)
            => EnumerateFiles(path, "*");

        /// <inheritdoc cref="IFileSystem.IDirectory.EnumerateFiles(string, string)" />
        public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
            => _fileSystem.Storage.Enumerate<IFileSystem.IFileInfo>(
                    path,
                    searchPattern,
                    EnumerationOptionsHelper.Compatible,
                    DirectoryNotFoundException(_fileSystem.Path.GetFullPath(path)))
               .Select(x => _fileSystem.Storage.GetSubdirectoryPath(
                    x.FullName,
                    path));

        /// <inheritdoc cref="IFileSystem.IDirectory.EnumerateFiles(string, string, SearchOption)" />
        public IEnumerable<string> EnumerateFiles(string path,
                                                  string searchPattern,
                                                  SearchOption searchOption)
            => _fileSystem.Storage.Enumerate<IFileSystem.IFileInfo>(
                    path,
                    searchPattern,
                    EnumerationOptionsHelper.FromSearchOption(searchOption),
                    DirectoryNotFoundException(_fileSystem.Path.GetFullPath(path)))
               .Select(x => _fileSystem.Storage.GetSubdirectoryPath(
                    x.FullName,
                    path));

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="IFileSystem.IDirectory.EnumerateFiles(string, string, EnumerationOptions)" />
        public IEnumerable<string> EnumerateFiles(string path,
                                                  string searchPattern,
                                                  EnumerationOptions enumerationOptions)
            => _fileSystem.Storage.Enumerate<IFileSystem.IFileInfo>(
                    path,
                    searchPattern,
                    enumerationOptions,
                    DirectoryNotFoundException(_fileSystem.Path.GetFullPath(path)))
               .Select(x => _fileSystem.Storage.GetSubdirectoryPath(
                    x.FullName,
                    path));
#endif

        /// <inheritdoc cref="IFileSystem.IDirectory.EnumerateFileSystemEntries(string)" />
        public IEnumerable<string> EnumerateFileSystemEntries(string path)
            => EnumerateFileSystemEntries(path, "*");

        /// <inheritdoc cref="IFileSystem.IDirectory.EnumerateFileSystemEntries(string, string)" />
        public IEnumerable<string> EnumerateFileSystemEntries(
            string path, string searchPattern)
            => _fileSystem.Storage.Enumerate<IFileSystem.IFileSystemInfo>(
                    path,
                    searchPattern,
                    EnumerationOptionsHelper.Compatible,
                    DirectoryNotFoundException(_fileSystem.Path.GetFullPath(path)))
               .Select(x => _fileSystem.Storage.GetSubdirectoryPath(
                    x.FullName,
                    path));

        /// <inheritdoc cref="IFileSystem.IDirectory.EnumerateFileSystemEntries(string, string, SearchOption)" />
        public IEnumerable<string> EnumerateFileSystemEntries(string path,
            string searchPattern,
            SearchOption searchOption)
            => _fileSystem.Storage.Enumerate<IFileSystem.IFileSystemInfo>(
                    path,
                    searchPattern,
                    EnumerationOptionsHelper.FromSearchOption(searchOption),
                    DirectoryNotFoundException(_fileSystem.Path.GetFullPath(path)))
               .Select(x => _fileSystem.Storage.GetSubdirectoryPath(
                    x.FullName,
                    path));

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="IFileSystem.IDirectory.EnumerateFileSystemEntries(string, string, EnumerationOptions)" />
        public IEnumerable<string> EnumerateFileSystemEntries(string path,
            string searchPattern,
            EnumerationOptions enumerationOptions)
            => _fileSystem.Storage.Enumerate<IFileSystem.IFileSystemInfo>(
                    path,
                    searchPattern,
                    enumerationOptions,
                    DirectoryNotFoundException(_fileSystem.Path.GetFullPath(path)))
               .Select(x => _fileSystem.Storage.GetSubdirectoryPath(
                    x.FullName,
                    path));
#endif

        /// <inheritdoc cref="IFileSystem.IDirectory.Exists(string)" />
        public bool Exists([NotNullWhen(true)] string? path)
            => _fileSystem.Storage.Exists(path);

        /// <inheritdoc cref="IFileSystem.IDirectory.GetCreationTime(string)" />
        public DateTime GetCreationTime(string path)
            => (_fileSystem.Storage.GetDirectory(path) ??
                _fileSystem.NullFileSystemInfo).CreationTime;

        /// <inheritdoc cref="IFileSystem.IDirectory.GetCreationTimeUtc(string)" />
        public DateTime GetCreationTimeUtc(string path)
            => (_fileSystem.Storage.GetDirectory(path) ??
                _fileSystem.NullFileSystemInfo).CreationTimeUtc;

        /// <inheritdoc cref="IFileSystem.IDirectory.GetCurrentDirectory()" />
        public string GetCurrentDirectory()
            => _fileSystem.Storage.CurrentDirectory;

        /// <inheritdoc cref="IFileSystem.IDirectory.GetDirectories(string)" />
        public string[] GetDirectories(string path)
            => EnumerateDirectories(path).ToArray();

        /// <inheritdoc cref="IFileSystem.IDirectory.GetDirectories(string, string)" />
        public string[] GetDirectories(string path, string searchPattern)
            => EnumerateDirectories(path, searchPattern).ToArray();

        /// <inheritdoc cref="IFileSystem.IDirectory.GetDirectories(string, string, SearchOption)" />
        public string[] GetDirectories(string path,
                                       string searchPattern,
                                       SearchOption searchOption)
            => EnumerateDirectories(path, searchPattern, searchOption).ToArray();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="IFileSystem.IDirectory.GetDirectories(string, string, EnumerationOptions)" />
        public string[] GetDirectories(string path,
                                       string searchPattern,
                                       EnumerationOptions enumerationOptions)
            => EnumerateDirectories(path, searchPattern, enumerationOptions).ToArray();
#endif

        /// <inheritdoc cref="IFileSystem.IDirectory.GetDirectoryRoot(string)" />
        public string GetDirectoryRoot(string path)
            => _fileSystem.Path.GetPathRoot(_fileSystem.Path.GetFullPath(path)) ??
               throw ExceptionFactory.PathIsEmpty(nameof(path));

        /// <inheritdoc cref="IFileSystem.IDirectory.GetFiles(string)" />
        public string[] GetFiles(string path)
            => EnumerateFiles(path).ToArray();

        /// <inheritdoc cref="IFileSystem.IDirectory.GetFiles(string, string)" />
        public string[] GetFiles(string path, string searchPattern)
            => EnumerateFiles(path, searchPattern).ToArray();

        /// <inheritdoc cref="IFileSystem.IDirectory.GetFiles(string, string, SearchOption)" />
        public string[] GetFiles(string path,
                                 string searchPattern,
                                 SearchOption searchOption)
            => EnumerateFiles(path, searchPattern, searchOption).ToArray();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="IFileSystem.IDirectory.GetFiles(string, string, EnumerationOptions)" />
        public string[] GetFiles(string path,
                                 string searchPattern,
                                 EnumerationOptions enumerationOptions)
            => EnumerateFiles(path, searchPattern, enumerationOptions).ToArray();
#endif

        /// <inheritdoc cref="IFileSystem.IDirectory.GetFileSystemEntries(string)" />
        public string[] GetFileSystemEntries(string path)
            => EnumerateFileSystemEntries(path).ToArray();

        /// <inheritdoc cref="IFileSystem.IDirectory.GetFileSystemEntries(string, string)" />
        public string[] GetFileSystemEntries(string path, string searchPattern)
            => EnumerateFileSystemEntries(path, searchPattern).ToArray();

        /// <inheritdoc cref="IFileSystem.IDirectory.GetFileSystemEntries(string, string, SearchOption)" />
        public string[] GetFileSystemEntries(string path,
                                             string searchPattern,
                                             SearchOption searchOption)
            => EnumerateFileSystemEntries(path, searchPattern, searchOption).ToArray();

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="IFileSystem.IDirectory.GetFileSystemEntries(string, string, EnumerationOptions)" />
        public string[] GetFileSystemEntries(string path,
                                             string searchPattern,
                                             EnumerationOptions enumerationOptions)
            => EnumerateFileSystemEntries(path, searchPattern, enumerationOptions)
               .ToArray();
#endif

        /// <inheritdoc cref="IFileSystem.IDirectory.GetLastAccessTime(string)" />
        public DateTime GetLastAccessTime(string path)
            => (_fileSystem.Storage.GetDirectory(path) ??
                _fileSystem.NullFileSystemInfo).LastAccessTime;

        /// <inheritdoc cref="IFileSystem.IDirectory.GetLastAccessTimeUtc(string)" />
        public DateTime GetLastAccessTimeUtc(string path)
            => (_fileSystem.Storage.GetDirectory(path) ??
                _fileSystem.NullFileSystemInfo).LastAccessTimeUtc;

        /// <inheritdoc cref="IFileSystem.IDirectory.GetLastWriteTime(string)" />
        public DateTime GetLastWriteTime(string path)
            => (_fileSystem.Storage.GetDirectory(path) ??
                _fileSystem.NullFileSystemInfo).LastWriteTime;

        /// <inheritdoc cref="IFileSystem.IDirectory.GetLastWriteTimeUtc(string)" />
        public DateTime GetLastWriteTimeUtc(string path)
            => (_fileSystem.Storage.GetDirectory(path) ??
                _fileSystem.NullFileSystemInfo).LastWriteTimeUtc;

        /// <inheritdoc cref="IFileSystem.IDirectory.GetLogicalDrives()" />
        public string[] GetLogicalDrives()
            => _fileSystem.DriveInfo.GetDrives().Select(x => x.Name).ToArray();

        /// <inheritdoc cref="IFileSystem.IDirectory.GetParent(string)" />
        public IFileSystem.IDirectoryInfo? GetParent(string path)
            => _fileSystem.DirectoryInfo.New(path).Parent;

        /// <inheritdoc cref="IFileSystem.IDirectory.Move(string, string)" />
        public void Move(string sourceDirName, string destDirName)
            => throw new NotImplementedException();

#if FEATURE_FILESYSTEM_LINK
        /// <inheritdoc cref="IFileSystem.IDirectory.ResolveLinkTarget(string, bool)" />
        public IFileSystem.IFileSystemInfo? ResolveLinkTarget(
            string linkPath, bool returnFinalTarget)
            => throw new NotImplementedException();
#endif

        /// <inheritdoc cref="IFileSystem.IDirectory.SetCreationTime(string, DateTime)" />
        public void SetCreationTime(string path, DateTime creationTime)
        {
            IFileSystem.IDirectoryInfo? fileSystemInfo =
                _fileSystem.Storage.GetDirectory(path);
            if (fileSystemInfo == null)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    throw ExceptionFactory.FileNotFound(
                        FileSystem.Path.GetFullPath(path));
                }

                throw ExceptionFactory.DirectoryNotFound(
                    FileSystem.Path.GetFullPath(path));
            }

            fileSystemInfo.CreationTime = creationTime;
        }

        /// <inheritdoc cref="IFileSystem.IDirectory.SetCreationTimeUtc(string, DateTime)" />
        public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
        {
            IFileSystem.IDirectoryInfo? fileSystemInfo =
                _fileSystem.Storage.GetDirectory(path);
            if (fileSystemInfo == null)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    throw ExceptionFactory.FileNotFound(
                        FileSystem.Path.GetFullPath(path));
                }

                throw ExceptionFactory.DirectoryNotFound(
                    FileSystem.Path.GetFullPath(path));
            }

            fileSystemInfo.CreationTimeUtc = creationTimeUtc;
        }

        /// <inheritdoc cref="IFileSystem.IDirectory.SetCurrentDirectory(string)" />
        public void SetCurrentDirectory(string path)
            => _fileSystem.Storage.CurrentDirectory = path;

        /// <inheritdoc cref="IFileSystem.IDirectory.SetLastAccessTime(string, DateTime)" />
        public void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            IFileSystem.IDirectoryInfo? fileSystemInfo =
                _fileSystem.Storage.GetDirectory(path);
            if (fileSystemInfo == null)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    throw ExceptionFactory.FileNotFound(
                        FileSystem.Path.GetFullPath(path));
                }

                throw ExceptionFactory.DirectoryNotFound(
                    FileSystem.Path.GetFullPath(path));
            }

            fileSystemInfo.LastAccessTime = lastAccessTime;
        }

        /// <inheritdoc cref="IFileSystem.IDirectory.SetLastAccessTimeUtc(string, DateTime)" />
        public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            IFileSystem.IDirectoryInfo? fileSystemInfo =
                _fileSystem.Storage.GetDirectory(path);
            if (fileSystemInfo == null)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    throw ExceptionFactory.FileNotFound(
                        FileSystem.Path.GetFullPath(path));
                }

                throw ExceptionFactory.DirectoryNotFound(
                    FileSystem.Path.GetFullPath(path));
            }

            fileSystemInfo.LastAccessTimeUtc = lastAccessTimeUtc;
        }

        /// <inheritdoc cref="IFileSystem.IDirectory.SetLastWriteTime(string, DateTime)" />
        public void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            IFileSystem.IDirectoryInfo? fileSystemInfo =
                _fileSystem.Storage.GetDirectory(path);
            if (fileSystemInfo == null)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    throw ExceptionFactory.FileNotFound(
                        FileSystem.Path.GetFullPath(path));
                }

                throw ExceptionFactory.DirectoryNotFound(
                    FileSystem.Path.GetFullPath(path));
            }

            fileSystemInfo.LastWriteTime = lastWriteTime;
        }

        /// <inheritdoc cref="IFileSystem.IDirectory.SetLastWriteTimeUtc(string, DateTime)" />
        public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            IFileSystem.IDirectoryInfo? fileSystemInfo =
                _fileSystem.Storage.GetDirectory(path);
            if (fileSystemInfo == null)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    throw ExceptionFactory.FileNotFound(
                        FileSystem.Path.GetFullPath(path));
                }

                throw ExceptionFactory.DirectoryNotFound(
                    FileSystem.Path.GetFullPath(path));
            }

            fileSystemInfo.LastWriteTimeUtc = lastWriteTimeUtc;
        }

        #endregion

        private IFileSystem.IDirectoryInfo CreateDirectoryInternal(string? path)
        {
            path.ThrowCommonExceptionsIfPathIsInvalid(_fileSystem);

            IFileSystem.IDirectoryInfo? directory =
                _fileSystem.Storage.GetOrAddDirectory(path);
            return directory ?? throw new NotImplementedException();
        }

        private static Func<Exception> DirectoryNotFoundException(string path)
        {
            return () => new DirectoryNotFoundException(
                $"Could not find a part of the path '{path}'.");
        }
    }
}