using System.IO;
using Testably.Abstractions.Models;

namespace Testably.Abstractions;

public sealed partial class FileSystem
{
    private sealed class FileInfoFactory : IFileSystem.IFileInfoFactory
    {
        internal FileInfoFactory(FileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        #region IFileInfoFactory Members

        /// <inheritdoc cref="IFileSystem.IFileInfoFactory.New" />
        public IFileSystem.IFileInfo New(string path)
            => FileInfoWrapper.FromFileInfo(new FileInfo(path), FileSystem);

        /// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
        public IFileSystem FileSystem { get; }

        #endregion
    }
}