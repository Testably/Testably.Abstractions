namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemPathTests
{
    // ReSharper disable once UnusedMember.Global
    public sealed class MockFileSystem : FileSystemPathTests<FileSystemMock>
    {
        public MockFileSystem() : base(new FileSystemMock())
        {
        }
    }
}