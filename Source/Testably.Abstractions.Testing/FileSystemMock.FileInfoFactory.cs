using System;
using Testably.Abstractions.Testing.Internal;
using Testably.Abstractions.Testing.Internal.Models;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    private sealed class FileInfoFactoryMock : IFileSystem.IFileInfoFactory
    {
        private readonly FileSystemMock _fileSystem;

        internal FileInfoFactoryMock(FileSystemMock fileSystem,
                                     FileSystemMockCallbackHandler callbackHandler)
        {
            _fileSystem = fileSystem;
            _ = callbackHandler;
        }

        #region IFileInfoFactory Members

        /// <inheritdoc cref="IFileSystem.IFileInfoFactory.New(string)" />
        public IFileSystem.IFileInfo New(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return FileInfoMock.New(path, _fileSystem);
        }

        /// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
        public IFileSystem FileSystem
            => _fileSystem;

        #endregion
    }
}