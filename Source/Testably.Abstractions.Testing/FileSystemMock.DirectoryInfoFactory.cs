using System;
using Testably.Abstractions.Testing.Internal;
using Testably.Abstractions.Testing.Internal.Models;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    private sealed class DirectoryInfoFactoryMock : IFileSystem.IDirectoryInfoFactory
    {
        private readonly FileSystemMock _fileSystem;

        internal DirectoryInfoFactoryMock(FileSystemMock fileSystem,
                                          FileSystemMockCallbackHandler callbackHandler)
        {
            _fileSystem = fileSystem;
            _ = callbackHandler;
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

            return DirectoryInfoMock.New(path, _fileSystem);
        }

        #endregion
    }
}