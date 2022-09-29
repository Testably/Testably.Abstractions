using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.Mock;

public static partial class MockFileSystem
{
    // ReSharper disable once UnusedMember.Global
    [SystemTest(nameof(MockFileSystem))]
    public sealed class
        FileInfoFactoryTests : FileSystemFileInfoFactoryTests<FileSystemMock>
    {
        public FileInfoFactoryTests() : this(new FileSystemMock())
        {
        }

        private FileInfoFactoryTests(FileSystemMock fileSystemMock) : base(
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