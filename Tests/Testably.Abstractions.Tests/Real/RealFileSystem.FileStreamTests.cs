using Testably.Abstractions.Tests.TestHelpers.Attributes;
using Xunit.Abstractions;

namespace Testably.Abstractions.Tests.Real;

public static partial class RealFileSystem
{
    // ReSharper disable once UnusedMember.Global
    [Collection(nameof(RealFileSystemTestAttribute))]
    [RealFileSystemTest]
    public sealed class FileStreamTests : FileSystemFileStreamTests<FileSystem>, IDisposable
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public FileStreamTests(ITestOutputHelper testOutputHelper)
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