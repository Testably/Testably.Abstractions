using System;
using System.IO;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    private sealed class DirectoryInfoFactoryMock : IFileSystem.IDirectoryInfoFactory
    {
        private readonly FileSystemMock _fileSystem;

        internal DirectoryInfoFactoryMock(FileSystemMock fileSystem)
        {
            _fileSystem = fileSystem;
        }

        #region IDirectoryInfoFactory Members

        /// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
        public IFileSystem FileSystem
            => _fileSystem;

        /// <inheritdoc cref="IFileSystem.IDirectoryInfoFactory.New(string)" />
        public IFileSystem.IDirectoryInfo New(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return DirectoryInfoMock.New(InMemoryLocation.New(_fileSystem, path), _fileSystem);
        }

        /// <inheritdoc cref="IFileSystem.IDirectoryInfoFactory.Wrap(DirectoryInfo)" />
        public IFileSystem.IDirectoryInfo Wrap(DirectoryInfo directoryInfo)
            => DirectoryInfoMock.New(InMemoryLocation.New(_fileSystem, directoryInfo.FullName, directoryInfo.ToString()), _fileSystem);

        #endregion
    }
}