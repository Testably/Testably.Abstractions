namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemDirectoryTests
{
    // ReSharper disable once UnusedMember.Global
    public sealed class MockFileSystem : FileSystemDirectoryTests
    {
        public MockFileSystem() : this(new FileSystemMock())
        {
        }

        private MockFileSystem(FileSystemMock fileSystemMock) : base(
            fileSystemMock,
            fileSystemMock.TimeSystem,
            fileSystemMock.Path.Combine(fileSystemMock.Path.GetTempPath(),
                fileSystemMock.Path.GetRandomFileName()))
        {
            FileSystem.Directory.CreateDirectory(BasePath);
            FileSystem.Directory.SetCurrentDirectory(BasePath);
        }
    }
}