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

        /// <inheritdoc />
        public IFileSystem.IDirectoryInfo New(string path) =>
            new DirectoryInfoMock(path, _fileSystem);

        /// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
        public IFileSystem FileSystem => _fileSystem;

        #endregion
    }
}