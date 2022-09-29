using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.Mock;

public static partial class MockFileSystem
{
    // ReSharper disable once UnusedMember.Global
    [SystemTest(nameof(MockFileSystem))]
    public sealed class Tests : FileSystemTests<FileSystemMock>
    {
        public Tests() : base(new FileSystemMock())
        {
        }
    }
}