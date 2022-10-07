using System;
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

            return FileInfoMock.New(InMemoryLocation.New(_fileSystem, fileName), _fileSystem);
        }

        /// <inheritdoc cref="IFileSystem.IFileInfoFactory.Wrap(FileInfo)" />
        public IFileSystem.IFileInfo Wrap(FileInfo fileInfo)
            => FileInfoMock.New(
                InMemoryLocation.New(_fileSystem, fileInfo.FullName, fileInfo.ToString()),
                _fileSystem);

        #endregion
    }
}