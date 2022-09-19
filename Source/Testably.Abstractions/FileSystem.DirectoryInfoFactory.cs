using System.IO;
using Testably.Abstractions.Models;

namespace Testably.Abstractions;

public sealed partial class FileSystem
{
    private sealed class DirectoryInfoFactory : IFileSystem.IDirectoryInfoFactory
    {
        internal DirectoryInfoFactory(FileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        #region IDirectoryInfoFactory Members

        /// <inheritdoc cref="IFileSystem.IDirectoryInfoFactory.New" />
        public IFileSystem.IDirectoryInfo New(string path)
            => DirectoryInfoWrapper.FromDirectoryInfo(new DirectoryInfo(path),
                FileSystem);

        /// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
        public IFileSystem FileSystem { get; }

        #endregion
    }
}