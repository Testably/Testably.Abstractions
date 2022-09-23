namespace Testably.Abstractions.Tests.Mock;

public static partial class MockFileSystem
{
    // ReSharper disable once UnusedMember.Global
    public sealed class PathTests : FileSystemPathTests<FileSystemMock>
    {
        public PathTests() : base(new FileSystemMock())
        {
        }
    }
}