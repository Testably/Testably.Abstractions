using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.Mock;

public static partial class MockFileSystem
{
    // ReSharper disable once UnusedMember.Global
    [SystemTest(nameof(MockFileSystem))]
    public sealed class PathTests : FileSystemPathTests<FileSystemMock>
    {
        public PathTests() : base(new FileSystemMock())
        {
        }
    }
}