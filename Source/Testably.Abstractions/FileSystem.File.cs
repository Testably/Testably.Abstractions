namespace Testably.Abstractions;

public sealed partial class FileSystem
{
    private sealed class FileFileSystem : IFileSystem.IFile
    {
        internal FileFileSystem(FileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        #region IFile Members

        /// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
        public IFileSystem FileSystem { get; }

        #endregion
    }
}