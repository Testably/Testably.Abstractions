using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    private sealed class FileInfoFactoryMock : IFileSystem.IFileInfoFactory
    {
        private readonly FileSystemMock _fileSystem;

        internal FileInfoFactoryMock(FileSystemMock fileSystem)
        {
            _fileSystem = fileSystem;
        }

        #region IFileInfoFactory Members

        /// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
        public IFileSystem FileSystem
            => _fileSystem;

        /// <inheritdoc cref="IFileSystem.IFileInfoFactory.New(string)" />
        public IFileSystem.IFileInfo New(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            return FileInfoMock.New(
                _fileSystem.Storage.GetLocation(fileName),
                _fileSystem);
        }

        /// <inheritdoc cref="IFileSystem.IFileInfoFactory.Wrap(FileInfo)" />
        [return: NotNullIfNotNull("fileInfo")]
        public IFileSystem.IFileInfo? Wrap(FileInfo? fileInfo)
            => FileInfoMock.New(
                _fileSystem.Storage.GetLocation(
                    fileInfo?.FullName,
                    fileInfo?.ToString()),
                _fileSystem);

        #endregion
    }
}