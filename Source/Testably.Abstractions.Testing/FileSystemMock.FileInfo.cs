using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    /// <summary>
    ///     A mocked file in the <see cref="InMemoryFileSystem" />.
    /// </summary>
    private sealed class FileInfoMock : FileSystemInfoMock, IFileSystem.IFileInfo
    {
        internal FileInfoMock(string path, FileSystemMock fileSystem)
            : base(path, fileSystem)
        {
        }

        #region IFileInfo Members

        /// <inheritdoc />
        public IFileSystem.IDirectoryInfo? Directory { get; }

        /// <inheritdoc />
        public string? DirectoryName { get; }

        /// <inheritdoc />
        public bool IsReadOnly { get; set; }

        /// <inheritdoc />
        public long Length { get; }

        /// <inheritdoc />
        public StreamWriter AppendText()
            => throw new NotImplementedException();

        /// <inheritdoc />
        public IFileSystem.IFileInfo CopyTo(string destFileName)
            => throw new NotImplementedException();

        /// <inheritdoc />
        public IFileSystem.IFileInfo CopyTo(string destFileName, bool overwrite)
            => throw new NotImplementedException();

        /// <inheritdoc />
        public FileStream Create()
            => throw new NotImplementedException();

        /// <inheritdoc />
        public StreamWriter CreateText()
            => throw new NotImplementedException();

        /// <inheritdoc />
        public void Decrypt()
            => throw new NotImplementedException();

        /// <inheritdoc />
        public void Encrypt()
            => throw new NotImplementedException();

        /// <inheritdoc />
        public void MoveTo(string destFileName)
            => throw new NotImplementedException();

        /// <inheritdoc />
        public void MoveTo(string destFileName, bool overwrite)
            => throw new NotImplementedException();

        /// <inheritdoc />
        public FileStream Open(FileMode mode)
            => throw new NotImplementedException();

        /// <inheritdoc />
        public FileStream Open(FileMode mode, FileAccess access)
            => throw new NotImplementedException();

        /// <inheritdoc />
        public FileStream Open(FileMode mode, FileAccess access, FileShare share)
            => throw new NotImplementedException();

        /// <inheritdoc />
        public FileStream OpenRead()
            => throw new NotImplementedException();

        /// <inheritdoc />
        public StreamReader OpenText()
            => throw new NotImplementedException();

        /// <inheritdoc />
        public FileStream OpenWrite()
            => throw new NotImplementedException();

        /// <inheritdoc />
        public IFileSystem.IFileInfo Replace(string destinationFileName,
                                             string? destinationBackupFileName)
            => throw new NotImplementedException();

        /// <inheritdoc />
        public IFileSystem.IFileInfo Replace(string destinationFileName,
                                             string? destinationBackupFileName,
                                             bool ignoreMetadataErrors)
            => throw new NotImplementedException();

        #endregion

        [return: NotNullIfNotNull("path")]
        internal static FileInfoMock? New(string? path, FileSystemMock fileSystem)
        {
            if (path == null)
            {
                return null;
            }

            return new FileInfoMock(path, fileSystem);
        }
    }
}