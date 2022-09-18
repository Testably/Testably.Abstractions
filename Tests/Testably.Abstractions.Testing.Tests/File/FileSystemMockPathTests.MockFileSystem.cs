namespace Testably.Abstractions.Testing.Tests.File;

public abstract partial class FileSystemMockPathTests
{
    // ReSharper disable once UnusedMember.Global
    public class MockFileSystem : FileSystemMockPathTests
    {
        public MockFileSystem() : base(new FileSystemMock())
        {
        }
    }
}