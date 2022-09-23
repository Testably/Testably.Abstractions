using Xunit.Abstractions;

namespace Testably.Abstractions.Tests.Real;

public static partial class RealFileSystem
{
    // ReSharper disable once UnusedMember.Global
    [Collection(RealFileSystemCollection)]
    public sealed class FileTests : FileSystemFileTests<FileSystem>, IDisposable
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public FileTests(ITestOutputHelper testOutputHelper)
            : base(
                new FileSystem(),
                new TimeSystem(),
                UseBasePath(testOutputHelper))
        {
            _testOutputHelper = testOutputHelper;
        }

        #region IDisposable Members

        /// <inheritdoc cref="IDisposable.Dispose()" />
        public void Dispose()
            => TryCleanup(BasePath, _testOutputHelper);

        #endregion
    }
}