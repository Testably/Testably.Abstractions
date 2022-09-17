using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    private sealed class DirectoryMock : IFileSystem.IDirectory
    {
        private readonly FileSystemMock _fileSystem;

        internal DirectoryMock(FileSystemMock fileSystem,
                               FileSystemMockCallbackHandler callbackHandler)
        {
            _fileSystem = fileSystem;
            _ = callbackHandler;
        }

        #region IFile Members

        /// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
        public IFileSystem FileSystem => _fileSystem;

        #endregion
    }
}