namespace Testably.Abstractions.Testing.Tests.File;

public abstract partial class FileSystemMockDirectoryTests
{
    // ReSharper disable once UnusedMember.Global
    public class MockFileSystem : FileSystemMockDirectoryTests
    {
        public MockFileSystem() : this(new FileSystemMock(), "tmp".PrefixRoot())
        {
        }

        private MockFileSystem(FileSystemMock fileSystemMock, string basePath)
            : base(fileSystemMock, fileSystemMock.TimeSystem, basePath)
        {
            FileSystem.Directory.CreateDirectory(BasePath);
            FileSystem.Directory.SetCurrentDirectory(BasePath);
        }
    }
}