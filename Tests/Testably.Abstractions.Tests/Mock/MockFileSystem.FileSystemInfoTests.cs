using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.Mock;

public static partial class MockFileSystem
{
    // ReSharper disable once UnusedMember.Global
    [SystemTest(nameof(MockFileSystem))]
    public sealed class FileSystemInfoTests
        : FileSystemFileSystemInfoTests<FileSystemMock>, IDisposable
    {
        /// <inheritdoc cref="FileSystemFileSystemInfoTests{TFileSystem}.BasePath" />
        public override string BasePath => _directoryCleaner.BasePath;

        private readonly FileSystemInitializer.IDirectoryCleaner _directoryCleaner;

        public FileSystemInfoTests() : this(new FileSystemMock())
        {
        }

        private FileSystemInfoTests(FileSystemMock fileSystemMock) : base(
            fileSystemMock,
            fileSystemMock.TimeSystem)
        {
            _directoryCleaner = FileSystem
               .SetCurrentDirectoryToEmptyTemporaryDirectory();
        }

        #region IDisposable Members

        /// <inheritdoc cref="IDisposable.Dispose()" />
        public void Dispose()
            => _directoryCleaner.Dispose();

        #endregion
    }
}