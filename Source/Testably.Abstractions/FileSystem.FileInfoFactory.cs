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

        /// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
        public IFileSystem FileSystem { get; }

        /// <inheritdoc cref="IFileSystem.IFileInfoFactory.New" />
        public IFileSystem.IFileInfo New(string fileName)
            => FileInfoWrapper.FromFileInfo(
                new FileInfo(fileName),
                FileSystem);

        /// <inheritdoc cref="IFileSystem.IFileInfoFactory.Wrap(FileInfo)" />
        public IFileSystem.IFileInfo Wrap(FileInfo fileInfo)
            => FileInfoWrapper.FromFileInfo(
                fileInfo,
                FileSystem);

        #endregion
    }
}