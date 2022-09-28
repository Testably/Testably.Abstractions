using System.IO;
using Testably.Abstractions.Models;

namespace Testably.Abstractions;

public sealed partial class FileSystem
{
    private sealed class FileStreamFactory : IFileSystem.IFileStreamFactory
    {
        internal FileStreamFactory(FileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        #region IFileStreamFactory Members

        /// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
        public IFileSystem FileSystem { get; }

        /// <inheritdoc cref="IFileSystem.IFileStreamFactory.New(string, FileMode)" />
        public FileSystemStream New(string path, FileMode mode)
            => Wrap(new FileStream(path, mode));

        /// <inheritdoc cref="IFileSystem.IFileStreamFactory.New(string, FileMode, FileAccess)" />
        public FileSystemStream New(string path, FileMode mode, FileAccess access)
            => Wrap(new FileStream(path, mode, access));

        /// <inheritdoc cref="IFileSystem.IFileStreamFactory.New(string, FileMode, FileAccess, FileShare)" />
        public FileSystemStream New(string path,
                                    FileMode mode,
                                    FileAccess access,
                                    FileShare share)
            => Wrap(new FileStream(path, mode, access, share));

        /// <inheritdoc cref="IFileSystem.IFileStreamFactory.New(string, FileMode, FileAccess, FileShare, int)" />
        public FileSystemStream New(string path,
                                    FileMode mode,
                                    FileAccess access,
                                    FileShare share,
                                    int bufferSize)
            => Wrap(new FileStream(path, mode, access, share, bufferSize));

        /// <inheritdoc cref="IFileSystem.IFileStreamFactory.New(string, FileMode, FileAccess, FileShare, int, bool)" />
        public FileSystemStream New(string path,
                                    FileMode mode,
                                    FileAccess access,
                                    FileShare share,
                                    int bufferSize,
                                    bool useAsync)
            => Wrap(new FileStream(path, mode, access, share, bufferSize, useAsync));

        /// <inheritdoc cref="IFileSystem.IFileStreamFactory.New(string, FileMode, FileAccess, FileShare, int, FileOptions)" />
        public FileSystemStream New(string path,
                                    FileMode mode,
                                    FileAccess access,
                                    FileShare share,
                                    int bufferSize,
                                    FileOptions options)
            => Wrap(new FileStream(path, mode, access, share, bufferSize, options));

#if FEATURE_FILESYSTEM_STREAM_OPTIONS
        /// <inheritdoc cref="IFileSystem.IFileStreamFactory.New(string, FileStreamOptions)" />
        public FileSystemStream New(string path, FileStreamOptions options)
            => Wrap(new FileStream(path, options));
#endif

        /// <inheritdoc cref="IFileSystem.IFileStreamFactory.Wrap(FileStream)" />
        public FileSystemStream Wrap(FileStream fileStream)
            => new FileStreamWrapper(fileStream);

        #endregion
    }
}