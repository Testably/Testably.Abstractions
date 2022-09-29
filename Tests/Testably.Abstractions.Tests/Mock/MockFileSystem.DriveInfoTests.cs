namespace Testably.Abstractions.Tests.Mock;

public static partial class MockFileSystem
{
    // ReSharper disable once UnusedMember.Global
    public sealed class DriveInfoTests : FileSystemDriveInfoTests<FileSystemMock>
    {
        public DriveInfoTests() : this(new FileSystemMock())
        {
        }

        private DriveInfoTests(FileSystemMock fileSystemMock) : base(
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