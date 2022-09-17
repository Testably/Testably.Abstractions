namespace Testably.Abstractions;

public sealed partial class FileSystem
{
    private sealed class DirectoryFileSystem : IFileSystem.IDirectory
    {
        internal DirectoryFileSystem(FileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        #region IFile Members

        /// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
        public IFileSystem FileSystem { get; }

        #endregion
    }
}