﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Testably.Abstractions.Models;

namespace Testably.Abstractions;

public sealed partial class FileSystem
{
    private sealed class DirectoryFileSystem : IFileSystem.IDirectory
    {
        internal DirectoryFileSystem(FileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        #region IDirectory Members

        /// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
        public IFileSystem FileSystem { get; }

        /// <inheritdoc cref="Directory.CreateDirectory(string)" />
        public IFileSystem.IDirectoryInfo CreateDirectory(string path)
            => DirectoryInfoWrapper.FromDirectoryInfo(
                System.IO.Directory.CreateDirectory(path), FileSystem);

#if FEATURE_FILESYSTEM_LINK
        /// <inheritdoc cref="Directory.CreateSymbolicLink(string, string)" />
        public FileSystemInfo CreateSymbolicLink(string path, string pathToTarget)
            => System.IO.Directory.CreateSymbolicLink(path, pathToTarget);
#endif

        /// <inheritdoc cref="Directory.Delete(string)" />
        public void Delete(string path)
            => System.IO.Directory.Delete(path);

        /// <inheritdoc cref="Directory.Delete(string, bool)" />
        public void Delete(string path, bool recursive)
            => System.IO.Directory.Delete(path, recursive);

        /// <inheritdoc cref="Directory.EnumerateDirectories(string)" />
        public IEnumerable<string> EnumerateDirectories(string path)
            => System.IO.Directory.EnumerateDirectories(path);

        /// <inheritdoc cref="Directory.EnumerateDirectories(string, string)" />
        public IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
            => System.IO.Directory.EnumerateDirectories(path, searchPattern);

        /// <inheritdoc cref="Directory.EnumerateDirectories(string, string, SearchOption)" />
        public IEnumerable<string> EnumerateDirectories(string path,
                                                        string searchPattern,
                                                        SearchOption searchOption)
            => System.IO.Directory.EnumerateDirectories(
                path,
                searchPattern,
                searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="Directory.EnumerateDirectories(string, string, EnumerationOptions)" />
        public IEnumerable<string> EnumerateDirectories(string path,
                                                        string searchPattern,
                                                        EnumerationOptions
                                                            enumerationOptions)
            => System.IO.Directory.EnumerateDirectories(path, searchPattern,
                enumerationOptions);
#endif

        /// <inheritdoc cref="Directory.EnumerateFiles(string)" />
        public IEnumerable<string> EnumerateFiles(string path)
            => System.IO.Directory.EnumerateFiles(path);

        /// <inheritdoc cref="Directory.EnumerateFiles(string, string)" />
        public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
            => System.IO.Directory.EnumerateFiles(path, searchPattern);

        /// <inheritdoc cref="Directory.EnumerateFiles(string, string, SearchOption)" />
        public IEnumerable<string> EnumerateFiles(string path,
                                                  string searchPattern,
                                                  SearchOption searchOption)
            => System.IO.Directory.EnumerateFiles(path, searchPattern, searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="Directory.EnumerateFiles(string, string, EnumerationOptions)" />
        public IEnumerable<string> EnumerateFiles(string path,
                                                  string searchPattern,
                                                  EnumerationOptions enumerationOptions)
            => System.IO.Directory.EnumerateFiles(path, searchPattern,
                enumerationOptions);
#endif

        /// <inheritdoc cref="Directory.EnumerateFileSystemEntries(string)" />
        public IEnumerable<string> EnumerateFileSystemEntries(string path)
            => System.IO.Directory.EnumerateFileSystemEntries(path);

        /// <inheritdoc cref="Directory.EnumerateFileSystemEntries(string, string)" />
        public IEnumerable<string> EnumerateFileSystemEntries(
            string path, string searchPattern)
            => System.IO.Directory.EnumerateFileSystemEntries(path, searchPattern);

        /// <inheritdoc cref="Directory.EnumerateFileSystemEntries(string, string, SearchOption)" />
        public IEnumerable<string> EnumerateFileSystemEntries(string path,
            string searchPattern,
            SearchOption searchOption)
            => System.IO.Directory.EnumerateFileSystemEntries(path, searchPattern,
                searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="Directory.EnumerateFileSystemEntries(string, string, EnumerationOptions)" />
        public IEnumerable<string> EnumerateFileSystemEntries(string path,
            string searchPattern,
            EnumerationOptions
                enumerationOptions)
            => System.IO.Directory.EnumerateFileSystemEntries(path, searchPattern,
                enumerationOptions);
#endif

        /// <inheritdoc cref="Directory.Exists(string)" />
        public bool Exists([NotNullWhen(true)] string? path)
            => System.IO.Directory.Exists(path);

        /// <inheritdoc cref="Directory.GetCreationTime(string)" />
        public DateTime GetCreationTime(string path)
            => System.IO.Directory.GetCreationTime(path);

        /// <inheritdoc cref="Directory.GetCreationTimeUtc(string)" />
        public DateTime GetCreationTimeUtc(string path)
            => System.IO.Directory.GetCreationTimeUtc(path);

        /// <inheritdoc cref="Directory.GetCurrentDirectory()" />
        public string GetCurrentDirectory()
            => System.IO.Directory.GetCurrentDirectory();

        /// <inheritdoc cref="Directory.GetDirectories(string)" />
        public string[] GetDirectories(string path)
            => System.IO.Directory.GetDirectories(path);

        /// <inheritdoc cref="Directory.GetDirectories(string, string)" />
        public string[] GetDirectories(string path, string searchPattern)
            => System.IO.Directory.GetDirectories(path, searchPattern);

        /// <inheritdoc cref="Directory.GetDirectories(string, string, SearchOption)" />
        public string[] GetDirectories(string path,
                                       string searchPattern,
                                       SearchOption searchOption)
            => System.IO.Directory.GetDirectories(path, searchPattern, searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="Directory.GetDirectories(string, string, EnumerationOptions)" />
        public string[] GetDirectories(string path,
                                       string searchPattern,
                                       EnumerationOptions enumerationOptions)
            => System.IO.Directory.GetDirectories(path, searchPattern,
                enumerationOptions);
#endif

        /// <inheritdoc cref="Directory.GetDirectoryRoot(string)" />
        public string GetDirectoryRoot(string path)
            => System.IO.Directory.GetDirectoryRoot(path);

        /// <inheritdoc cref="Directory.GetFiles(string)" />
        public string[] GetFiles(string path)
            => System.IO.Directory.GetFiles(path);

        /// <inheritdoc cref="Directory.GetFiles(string, string)" />
        public string[] GetFiles(string path, string searchPattern)
            => System.IO.Directory.GetFiles(path, searchPattern);

        /// <inheritdoc cref="Directory.GetFiles(string, string, SearchOption)" />
        public string[] GetFiles(string path,
                                 string searchPattern,
                                 SearchOption searchOption)
            => System.IO.Directory.GetFiles(path, searchPattern, searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="Directory.GetFiles(string, string, EnumerationOptions)" />
        public string[] GetFiles(string path,
                                 string searchPattern,
                                 EnumerationOptions enumerationOptions)
            => System.IO.Directory.GetFiles(path, searchPattern, enumerationOptions);
#endif

        /// <inheritdoc cref="Directory.GetFileSystemEntries(string)" />
        public string[] GetFileSystemEntries(string path)
            => System.IO.Directory.GetFileSystemEntries(path);

        /// <inheritdoc cref="Directory.GetFileSystemEntries(string, string)" />
        public string[] GetFileSystemEntries(string path, string searchPattern)
            => System.IO.Directory.GetFileSystemEntries(path, searchPattern);

        /// <inheritdoc cref="Directory.GetFileSystemEntries(string, string, SearchOption)" />
        public string[] GetFileSystemEntries(string path,
                                             string searchPattern,
                                             SearchOption searchOption)
            => System.IO.Directory.GetFileSystemEntries(path, searchPattern,
                searchOption);

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
        /// <inheritdoc cref="Directory.GetFileSystemEntries(string, string, EnumerationOptions)" />
        public string[] GetFileSystemEntries(string path,
                                             string searchPattern,
                                             EnumerationOptions enumerationOptions)
            => System.IO.Directory.GetFileSystemEntries(path, searchPattern,
                enumerationOptions);
#endif

        /// <inheritdoc cref="Directory.GetLastAccessTime(string)" />
        public DateTime GetLastAccessTime(string path)
            => System.IO.Directory.GetLastAccessTime(path);

        /// <inheritdoc cref="Directory.GetLastAccessTimeUtc(string)" />
        public DateTime GetLastAccessTimeUtc(string path)
            => System.IO.Directory.GetLastAccessTimeUtc(path);

        /// <inheritdoc cref="Directory.GetLastWriteTime(string)" />
        public DateTime GetLastWriteTime(string path)
            => System.IO.Directory.GetLastWriteTime(path);

        /// <inheritdoc cref="Directory.GetLastWriteTimeUtc(string)" />
        public DateTime GetLastWriteTimeUtc(string path)
            => System.IO.Directory.GetLastWriteTimeUtc(path);

        /// <inheritdoc cref="Directory.GetLogicalDrives()" />
        public string[] GetLogicalDrives()
            => System.IO.Directory.GetLogicalDrives();

        /// <inheritdoc cref="Directory.GetParent(string)" />
        public IFileSystem.IDirectoryInfo? GetParent(string path)
            => DirectoryInfoWrapper.FromDirectoryInfo(System.IO.Directory.GetParent(path),
                FileSystem);

        /// <inheritdoc cref="Directory.Move(string, string)" />
        public void Move(string sourceDirName, string destDirName)
            => System.IO.Directory.Move(sourceDirName, destDirName);

#if FEATURE_FILESYSTEM_LINK
        /// <inheritdoc cref="Directory.ResolveLinkTarget(string, bool)" />
        public FileSystemInfo? ResolveLinkTarget(string linkPath, bool returnFinalTarget)
            => System.IO.Directory.ResolveLinkTarget(linkPath, returnFinalTarget);
#endif

        /// <inheritdoc cref="Directory.SetCreationTime(string, DateTime)" />
        public void SetCreationTime(string path, DateTime creationTime)
            => System.IO.Directory.SetCreationTime(path, creationTime);

        /// <inheritdoc cref="Directory.SetCreationTimeUtc(string, DateTime)" />
        public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
            => System.IO.Directory.SetCreationTimeUtc(path, creationTimeUtc);

        /// <inheritdoc cref="Directory.SetCurrentDirectory(string)" />
        public void SetCurrentDirectory(string path)
            => System.IO.Directory.SetCurrentDirectory(path);

        /// <inheritdoc cref="Directory.SetLastAccessTime(string, DateTime)" />
        public void SetLastAccessTime(string path, DateTime lastAccessTime)
            => System.IO.Directory.SetLastAccessTime(path, lastAccessTime);

        /// <inheritdoc cref="Directory.SetLastAccessTimeUtc(string, DateTime)" />
        public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
            => System.IO.Directory.SetLastAccessTimeUtc(path, lastAccessTimeUtc);

        /// <inheritdoc cref="Directory.SetLastWriteTime(string, DateTime)" />
        public void SetLastWriteTime(string path, DateTime lastWriteTime)
            => System.IO.Directory.SetLastWriteTime(path, lastWriteTime);

        /// <inheritdoc cref="Directory.SetLastWriteTimeUtc(string, DateTime)" />
        public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
            => System.IO.Directory.SetLastWriteTimeUtc(path, lastWriteTimeUtc);

        #endregion
    }
}