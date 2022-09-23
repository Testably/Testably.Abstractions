namespace Testably.Abstractions.Tests.Mock;

public static partial class MockFileSystem
{
    // ReSharper disable once UnusedMember.Global
    public sealed class Tests : FileSystemTests<FileSystemMock>
    {
        public Tests() : base(new FileSystemMock())
        {
        }
    }
}