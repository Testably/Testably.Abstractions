namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemTests
{
    // ReSharper disable once UnusedMember.Global
    public sealed class MockFileSystem : FileSystemTests<FileSystemMock>
    {
        public MockFileSystem() : base(new FileSystemMock())
        {
        }
    }
}