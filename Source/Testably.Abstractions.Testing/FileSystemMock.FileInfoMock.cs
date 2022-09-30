using System;
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
    ///     A mocked file in the <see cref="InMemoryFileSystem" />.
    /// </summary>
    private sealed class FileInfoMock : FileSystemInfoMock,
        IInMemoryFileSystem.IFileInfoMock
    {
        private byte[] _bytes = Array.Empty<byte>();

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
            WriteBytes(_bytes.Concat(bytes).ToArray());
        }

        /// <inheritdoc cref="IInMemoryFileSystem.IFileInfoMock.GetBytes()" />
        public byte[] GetBytes() => _bytes;

        /// <inheritdoc cref="IInMemoryFileSystem.IFileInfoMock.WriteBytes(byte[])" />
        public void WriteBytes(byte[] bytes)
        {
            Drive.ChangeUsedBytes(bytes.Length - _bytes.Length);
            _bytes = bytes;
        }

        /// <inheritdoc cref="IInMemoryFileSystem.IFileInfoMock.ClearBytes()" />
        public void ClearBytes()
        {
            Drive.ChangeUsedBytes(0 - _bytes.Length);
            _bytes = Array.Empty<byte>();
        }

        #endregion

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