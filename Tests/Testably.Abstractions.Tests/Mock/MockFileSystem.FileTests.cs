using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.Mock;

public static partial class MockFileSystem
{
    // ReSharper disable once UnusedMember.Global
    [SystemTest(nameof(MockFileSystem))]
    public sealed class FileTests
        : FileSystemFileTests<FileSystemMock>, IDisposable
    {
        /// <inheritdoc cref="FileSystemFileTests{TFileSystem}.BasePath" />
        public override string BasePath => _directoryCleaner.BasePath;

        private readonly FileSystemInitializer.IDirectoryCleaner _directoryCleaner;

        public FileTests() : this(new FileSystemMock())
        {
        }

        private FileTests(FileSystemMock fileSystemMock) : base(
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