namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemPathTests
{
    // ReSharper disable once UnusedMember.Global
    public sealed class MockFileSystem : FileSystemPathTests
    {
        public MockFileSystem() : base(new FileSystemMock())
        {
        }
    }
}