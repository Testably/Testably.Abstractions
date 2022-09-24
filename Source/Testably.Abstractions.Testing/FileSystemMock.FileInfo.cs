using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    /// <summary>
    ///     A mocked file in the <see cref="InMemoryFileSystem" />.
    /// </summary>
    private sealed class FileInfoMock : FileSystemInfoMock,
        IInMemoryFileSystem.IWritableFileInfo
    {
        private byte[] _bytes = Array.Empty<byte>();

        internal FileInfoMock(string fullName, string originalPath,
                              FileSystemMock fileSystem)
            : base(fullName, originalPath, fileSystem)
        {
        }

        #region IWritableFileInfo Members

        /// <inheritdoc cref="IFileSystem.IFileInfo.Directory" />
        public IFileSystem.IDirectoryInfo? Directory { get; }

        /// <inheritdoc cref="IFileSystem.IFileInfo.DirectoryName" />
        public string? DirectoryName { get; }

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

        /// <inheritdoc cref="IInMemoryFileSystem.IWritableFileInfo.AppendBytes(byte[])" />
        public void AppendBytes(byte[] bytes)
        {
            _bytes = _bytes.Concat(bytes).ToArray();
        }

        /// <inheritdoc cref="IInMemoryFileSystem.IWritableFileInfo.GetBytes()" />
        public byte[] GetBytes() => _bytes;

        /// <inheritdoc cref="IInMemoryFileSystem.IWritableFileInfo.WriteBytes(byte[])" />
        public void WriteBytes(byte[] bytes)
        {
            _bytes = bytes;
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
                throw new ArgumentException("The path is not of a legal form.");
#else
                throw new ArgumentException("The path is empty.", nameof(path));
#endif
            }

#if NETFRAMEWORK
            var originalPath = fileSystem.Path.GetFileName(path.TrimEnd(' '));
#else
            string originalPath = path;
#endif
            string fullName = fileSystem.Path.GetFullPath(path).NormalizePath()
               .TrimOnWindows();
            return new FileInfoMock(fullName, originalPath, fileSystem);
        }
    }
}