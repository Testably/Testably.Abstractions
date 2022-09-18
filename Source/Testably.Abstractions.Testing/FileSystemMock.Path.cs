using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
    private sealed class PathMock : PathSystem
    {
        private readonly FileSystemMock _fileSystem;

        internal PathMock(FileSystemMock fileSystem)
            : base(fileSystem)
        {
            _fileSystem = fileSystem;
        }

        /// <inheritdoc cref="IFileSystem.IPath.GetFullPath(string)" />
        public override string GetFullPath(string path)
        {
            return System.IO.Path.Combine(_fileSystem.InMemoryFileSystem.CurrentDirectory,
                path);
        }
    }
}