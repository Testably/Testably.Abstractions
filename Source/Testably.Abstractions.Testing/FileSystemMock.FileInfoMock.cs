﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Internal;
#if NET6_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    /// <summary>
    ///     A mocked file in the <see cref="InMemoryStorage" />.
    /// </summary>
    private sealed class FileInfoMock : FileSystemInfoMock,
        IStorage.IFileInfoMock
    {
        private byte[] _bytes = Array.Empty<byte>();

        internal FileInfoMock(string fullName, string originalPath,
                              FileSystemMock fileSystem)
            : base(fullName, originalPath, fileSystem)
        {
        }

        /// <inheritdoc cref="IFileSystem.IFileInfo.Directory" />
        public IFileSystem.IDirectoryInfo? Directory
            => DirectoryInfoMock.New(
                FileSystem.Path.GetDirectoryName(OriginalPath),
                FileSystem);

        /// <inheritdoc cref="IFileSystem.IFileInfo.DirectoryName" />
        public string? DirectoryName
            => Directory?.FullName;

        /// <inheritdoc cref="IFileSystem.IFileInfo.IsReadOnly" />
        public bool IsReadOnly
        {
            get => (Attributes & FileAttributes.ReadOnly) != 0;
            set
            {
                if (value)
                    Attributes |= FileAttributes.ReadOnly;
                else
                    Attributes &= ~FileAttributes.ReadOnly;
            }
        }

        /// <inheritdoc cref="IFileSystem.IFileInfo.Length" />
        public long Length
        {
            get
            {
                RefreshInternal();
                return (Container as IStorage.IFileInfoMock)?.GetBytes().Length
                       ?? throw ExceptionFactory.FileNotFound(Framework.IsNetFramework
                           ? OriginalPath
                           : FullName);
            }
        }

        /// <inheritdoc cref="IFileSystem.IFileInfo.AppendText()" />
        public StreamWriter AppendText()
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IFileInfo.CopyTo(string)" />
        public IFileSystem.IFileInfo CopyTo(string destFileName)
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IFileInfo.CopyTo(string, bool)" />
        public IFileSystem.IFileInfo CopyTo(string destFileName, bool overwrite)
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IFileInfo.Create()" />
        public FileSystemStream Create()
            => FileSystem.File.Create(FullName);

        /// <inheritdoc cref="IFileSystem.IFileInfo.CreateText()" />
        public StreamWriter CreateText()
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IFileInfo.Decrypt()" />
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        public void Decrypt()
        {
            using (RequestAccess(FileAccess.Write, FileShare.Read))
            {
                IsEncrypted = false;
                WriteBytes(EncryptionHelper.Decrypt(GetBytes()));
            }
        }

        /// <inheritdoc cref="IFileSystem.IFileInfo.Encrypt()" />
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        public void Encrypt()
        {
            using (RequestAccess(FileAccess.Write, FileShare.Read))
            {
                IsEncrypted = true;
                WriteBytes(EncryptionHelper.Encrypt(GetBytes()));
            }
        }

        /// <inheritdoc cref="IFileSystem.IFileInfo.MoveTo(string)" />
        public void MoveTo(string destFileName)
            => throw new NotImplementedException();

#if FEATURE_FILE_MOVETO_OVERWRITE
        /// <inheritdoc cref="IFileSystem.IFileInfo.MoveTo(string, bool)" />
        public void MoveTo(string destFileName, bool overwrite)
            => throw new NotImplementedException();
#endif

        /// <inheritdoc cref="IFileSystem.IFileInfo.Open(FileMode)" />
        public FileSystemStream Open(FileMode mode)
        {
            if (mode == FileMode.Append && Framework.IsNetFramework)
            {
                throw ExceptionFactory.AppendAccessOnlyInWriteOnlyMode();
            }

            return new FileStreamMock(
                FileSystem,
                FullName,
                mode,
                mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite,
                FileShare.None);
        }

        /// <inheritdoc cref="IFileSystem.IFileInfo.Open(FileMode, FileAccess)" />
        public FileSystemStream Open(FileMode mode, FileAccess access)
            => new FileStreamMock(
                FileSystem,
                FullName,
                mode,
                access,
                FileShare.None);

        /// <inheritdoc cref="IFileSystem.IFileInfo.Open(FileMode, FileAccess, FileShare)" />
        public FileSystemStream Open(FileMode mode, FileAccess access, FileShare share)
            => new FileStreamMock(
                FileSystem,
                FullName,
                mode,
                access,
                share);

#if FEATURE_FILESYSTEM_STREAM_OPTIONS
        /// <inheritdoc cref="IFileSystem.IFileInfo.Open(FileStreamOptions)" />
        public FileSystemStream Open(FileStreamOptions options)
            => FileSystem.File.Open(FullName, options);
#endif

        /// <inheritdoc cref="IFileSystem.IFileInfo.OpenRead()" />
        public FileSystemStream OpenRead()
            => new FileStreamMock(
                FileSystem,
                FullName,
                FileMode.Open,
                FileAccess.Read);

        /// <inheritdoc cref="IFileSystem.IFileInfo.OpenText()" />
        public StreamReader OpenText()
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IFileInfo.OpenWrite()" />
        public FileSystemStream OpenWrite()
            => new FileStreamMock(
                FileSystem,
                FullName,
                FileMode.OpenOrCreate,
                FileAccess.Write,
                FileShare.None);

        /// <inheritdoc cref="IFileSystem.IFileInfo.Replace(string, string?)" />
        public IFileSystem.IFileInfo Replace(string destinationFileName,
                                             string? destinationBackupFileName)
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IFileInfo.Replace(string, string?, bool)" />
        public IFileSystem.IFileInfo Replace(string destinationFileName,
                                             string? destinationBackupFileName,
                                             bool ignoreMetadataErrors)
            => throw new NotImplementedException();

        /// <inheritdoc cref="IStorage.IFileInfoMock.AppendBytes(byte[])" />
        public void AppendBytes(byte[] bytes)
        {
            WriteBytes(_bytes.Concat(bytes).ToArray());
        }

        /// <inheritdoc cref="IStorage.IFileInfoMock.GetBytes()" />
        public byte[] GetBytes() => _bytes;

        /// <inheritdoc cref="IStorage.IFileInfoMock.WriteBytes(byte[])" />
        public void WriteBytes(byte[] bytes)
        {
            Drive?.ChangeUsedBytes(bytes.Length - _bytes.Length);
            _bytes = bytes;
        }

        /// <inheritdoc cref="IStorage.IFileInfoMock.ClearBytes()" />
        public void ClearBytes()
        {
            Drive?.ChangeUsedBytes(0 - _bytes.Length);
            _bytes = Array.Empty<byte>();
        }

#if FEATURE_FILESYSTEM_LINK
        /// <inheritdoc cref="IStorage.IFileInfoMock.SetLinkTarget(string)" />
        public void SetLinkTarget(string pathToTarget)
        {
            LinkTarget = pathToTarget;
            Attributes |= FileAttributes.ReparsePoint;
        }
#endif

        [return: NotNullIfNotNull("path")]
        internal static FileInfoMock? New(string? path, FileSystemMock fileSystem)
        {
            if (path == null)
            {
                return null;
            }

            if (path == string.Empty)
            {
                if (Framework.IsNetFramework)
                {
                    throw ExceptionFactory.PathHasNoLegalForm();
                }

                throw ExceptionFactory.PathIsEmpty(nameof(path));
            }

            string originalPath = path;
            if (Framework.IsNetFramework)
            {
                originalPath = originalPath.TrimEnd(' ');
            }

            string fullName = fileSystem.Path.GetFullPath(path).NormalizePath()
               .TrimOnWindows();
            return new FileInfoMock(fullName, originalPath, fileSystem);
        }
    }
}