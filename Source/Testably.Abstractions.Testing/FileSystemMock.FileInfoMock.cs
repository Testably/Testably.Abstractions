using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Testably.Abstractions.Testing.Internal;
#if NET6_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    /// <summary>
    ///     A mocked file in the <see cref="InMemoryFileSystem" />.
    /// </summary>
    private sealed class FileInfoMock : FileSystemInfoMock,
        IInMemoryFileSystem.IFileInfoMock
    {
        private byte[] _bytes = Array.Empty<byte>();

        private readonly ConcurrentDictionary<Guid, FileHandle> _fileHandles = new();

        internal FileInfoMock(string fullName, string originalPath,
                              FileSystemMock fileSystem)
            : base(fullName, originalPath, fileSystem)
        {
        }

        #region IWritableFileInfo Members

        /// <inheritdoc cref="IFileSystem.IFileInfo.Directory" />
        public IFileSystem.IDirectoryInfo? Directory
            => DirectoryInfoMock.New(DirectoryName, FileSystem);

        /// <inheritdoc cref="IFileSystem.IFileInfo.DirectoryName" />
        public string? DirectoryName
            => FileSystem.Path.GetDirectoryName(OriginalPath);

        /// <inheritdoc cref="IFileSystem.IFileInfo.IsReadOnly" />
        public bool IsReadOnly { get; set; }

        /// <inheritdoc cref="IFileSystem.IFileInfo.Length" />
        public long Length { get; }

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
        public FileStream Create()
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IFileInfo.CreateText()" />
        public StreamWriter CreateText()
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IFileInfo.Decrypt()" />
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        public void Decrypt()
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IFileInfo.Encrypt()" />
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        public void Encrypt()
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IFileInfo.MoveTo(string)" />
        public void MoveTo(string destFileName)
            => throw new NotImplementedException();

#if FEATURE_FILE_MOVETO_OVERWRITE
        /// <inheritdoc cref="IFileSystem.IFileInfo.MoveTo(string, bool)" />
        public void MoveTo(string destFileName, bool overwrite)
            => throw new NotImplementedException();
#endif

        /// <inheritdoc cref="IFileSystem.IFileInfo.Open(FileMode)" />
        public FileStream Open(FileMode mode)
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IFileInfo.Open(FileMode, FileAccess)" />
        public FileStream Open(FileMode mode, FileAccess access)
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IFileInfo.Open(FileMode, FileAccess, FileShare)" />
        public FileStream Open(FileMode mode, FileAccess access, FileShare share)
            => throw new NotImplementedException();

#if FEATURE_FILESYSTEM_STREAM_OPTIONS
        /// <inheritdoc cref="IFileSystem.IFileInfo.Open(FileStreamOptions)" />
        public FileStream Open(FileStreamOptions options)
            => throw new NotImplementedException();
#endif

        /// <inheritdoc cref="IFileSystem.IFileInfo.OpenRead()" />
        public FileStream OpenRead()
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IFileInfo.OpenText()" />
        public StreamReader OpenText()
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IFileInfo.OpenWrite()" />
        public FileStream OpenWrite()
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IFileInfo.Replace(string, string?)" />
        public IFileSystem.IFileInfo Replace(string destinationFileName,
                                             string? destinationBackupFileName)
            => throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IFileInfo.Replace(string, string?, bool)" />
        public IFileSystem.IFileInfo Replace(string destinationFileName,
                                             string? destinationBackupFileName,
                                             bool ignoreMetadataErrors)
            => throw new NotImplementedException();

        /// <inheritdoc cref="IInMemoryFileSystem.IFileInfoMock.AppendBytes(byte[])" />
        public void AppendBytes(byte[] bytes)
        {
            _bytes = _bytes.Concat(bytes).ToArray();
        }

        /// <inheritdoc cref="IInMemoryFileSystem.IFileInfoMock.GetBytes()" />
        public byte[] GetBytes() => _bytes;

        /// <inheritdoc cref="IInMemoryFileSystem.IFileInfoMock.WriteBytes(byte[])" />
        public void WriteBytes(byte[] bytes)
        {
            _bytes = bytes;
        }

        #endregion

        /// <inheritdoc cref="IInMemoryFileSystem.IFileInfoMock.RequestAccess(FileAccess, FileShare)" />
        public IDisposable RequestAccess(FileAccess access, FileShare share)
        {
            if (CanGetAccess(access, share))
            {
                Guid guid = Guid.NewGuid();
                var fileHandle = new FileHandle(guid, ReleaseAccess, access, share);
                _fileHandles.TryAdd(guid, fileHandle);
                return fileHandle;
            }

            throw ExceptionFactory.ProcessCannotAccessTheFile(FullName);
        }

        private void ReleaseAccess(Guid guid)
        {
            _fileHandles.TryRemove(guid, out _);
        }

        private bool CanGetAccess(FileAccess access, FileShare share)
        {
            foreach (var fileHandle in _fileHandles)
            {
                if (!fileHandle.Value.GrantAccess(access, share))
                {
                    return false;
                }
            }

            return true;
        }

        private sealed class FileHandle : IDisposable
        {
            private readonly Action<Guid> _releaseCallback;
            private readonly FileAccess _access;
            private readonly FileShare _share;
            private readonly Guid _key;

            public FileHandle(Guid key, Action<Guid> releaseCallback, FileAccess access, FileShare share)
            {
                _releaseCallback = releaseCallback;
                _access = access;
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    _share = FileShare.ReadWrite;
                }
                else
                {
                    _share = share;
                }
                _key = key;
            }

            public bool GrantAccess(FileAccess access, FileShare share)
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    share = FileShare.ReadWrite;
                }
                return CheckAccessWithShare(access, _share) &&
                       CheckAccessWithShare(_access, share);
            }

            private static bool CheckAccessWithShare(FileAccess access, FileShare share)
            {
                switch (access)
                {
                    case FileAccess.Read:
                        return share.HasFlag(FileShare.Read);
                    case FileAccess.Write:
                        return share.HasFlag(FileShare.Write);
                    default:
                        return share == FileShare.ReadWrite;
                }
            }

            /// <inheritdoc cref="IDisposable.Dispose()" />
            public void Dispose()
            {
                _releaseCallback.Invoke(_key);
            }

        }

        [return: NotNullIfNotNull("path")]
        internal static FileInfoMock? New(string? path, FileSystemMock fileSystem)
        {
            if (path == null)
            {
                return null;
            }

            if (path == string.Empty)
            {
#if NETFRAMEWORK
                throw ExceptionFactory.PathHasNoLegalForm();
#else
                throw ExceptionFactory.PathIsEmpty(nameof(path));
#endif
            }

#if NETFRAMEWORK
            string originalPath = path.TrimEnd(' ');
#else
            string originalPath = path;
#endif
            string fullName = fileSystem.Path.GetFullPath(path).NormalizePath()
               .TrimOnWindows();
            return new FileInfoMock(fullName, originalPath, fileSystem);
        }
    }
}