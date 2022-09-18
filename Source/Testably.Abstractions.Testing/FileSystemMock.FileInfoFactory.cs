using System;
using Testably.Abstractions.Testing.Internal;

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

        /// <inheritdoc />
        public IFileSystem.IFileInfo Create(string path) =>
            throw new NotImplementedException();

        /// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
        public IFileSystem FileSystem => _fileSystem;

        #endregion
    }
}