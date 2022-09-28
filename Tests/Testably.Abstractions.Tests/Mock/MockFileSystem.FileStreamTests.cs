namespace Testably.Abstractions.Tests.Mock;

public static partial class MockFileSystem
{
    // ReSharper disable once UnusedMember.Global
    public sealed class FileStreamTests : FileSystemFileStreamTests<FileSystemMock>
    {
        public FileStreamTests() : this(new FileSystemMock())
        {
        }

        private FileStreamTests(FileSystemMock fileSystemMock) : base(
            fileSystemMock,
            fileSystemMock.TimeSystem,
            fileSystemMock.Path.Combine(
                fileSystemMock.Path.GetTempPath(),
                fileSystemMock.Path.GetRandomFileName()))
        {
            FileSystem.Directory.CreateDirectory(BasePath);
            FileSystem.Directory.SetCurrentDirectory(BasePath);
        }
    }
}