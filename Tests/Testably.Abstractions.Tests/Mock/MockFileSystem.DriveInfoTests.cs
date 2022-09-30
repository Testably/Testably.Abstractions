using System.Linq;
using System.Text;
using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.Mock;

public static partial class MockFileSystem
{
    // ReSharper disable once UnusedMember.Global
    [SystemTest(nameof(MockFileSystem))]
    public sealed class DriveInfoTests : FileSystemDriveInfoTests<FileSystemMock>
    {
        #region Test Setup

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

        #endregion

        [Theory]
        [AutoData]
        [FileSystemTests.DriveInfo(nameof(IFileSystem.IDriveInfo.AvailableFreeSpace))]
        public void AvailableFreeSpace_ShouldBeSetTotalSize(long size)
        {
            FileSystemMock sut = new FileSystemMock()
               .WithDrive(d => d.SetTotalSize(size));

            IFileSystem.IDriveInfo drive = sut.DriveInfo.GetDrives().Single();

            drive.AvailableFreeSpace.Should().Be(size);
        }

        [Theory]
        [AutoData]
        [FileSystemTests.DriveInfo(nameof(IFileSystem.IDriveInfo.AvailableFreeSpace))]
        public void AvailableFreeSpace_ShouldBeReducedByWritingToFile(
            int fileSize, string path)
        {
            byte[] bytes = new byte[fileSize];
            FileSystemMock sut = new FileSystemMock()
               .WithDrive(d => d.SetTotalSize(fileSize));
            sut.RandomSystem.Random.Shared.NextBytes(bytes);

            sut.File.WriteAllBytes(path, bytes);

            IFileSystem.IDriveInfo drive = sut.DriveInfo.GetDrives().Single();

            drive.AvailableFreeSpace.Should().Be(0);
        }

        [Theory]
        [AutoData]
        [FileSystemTests.DriveInfo(nameof(IFileSystem.IDriveInfo.AvailableFreeSpace))]
        public void AvailableFreeSpace_ShouldBeReleasedWhenDeletingAFile(
            int fileSize, string path)
        {
            byte[] bytes = new byte[fileSize];
            FileSystemMock sut = new FileSystemMock()
               .WithDrive(d => d.SetTotalSize(fileSize));
            sut.RandomSystem.Random.Shared.NextBytes(bytes);

            sut.File.WriteAllBytes(path, bytes);
            sut.File.Delete(path);

            IFileSystem.IDriveInfo drive = sut.DriveInfo.GetDrives().Single();

            drive.AvailableFreeSpace.Should().Be(fileSize);
        }

        [Theory]
        [AutoData]
        [FileSystemTests.DriveInfo(nameof(IFileSystem.IDriveInfo.AvailableFreeSpace))]
        public void AvailableFreeSpace_ShouldBeChangedWhenAppendingToAFile(
            string fileContent1, string fileContent2, int expectedRemainingBytes,
            string path, Encoding encoding)
        {
            int fileSize1 = encoding.GetBytes(fileContent1).Length;
            int fileSize2 = encoding.GetBytes(fileContent2).Length;
            FileSystemMock sut = new FileSystemMock()
               .WithDrive(d
                    => d.SetTotalSize(fileSize1 + fileSize2 + expectedRemainingBytes));
            IFileSystem.IDriveInfo drive = sut.DriveInfo.GetDrives().Single();

            sut.File.WriteAllText(path, fileContent1, encoding);
            drive.AvailableFreeSpace.Should().Be(expectedRemainingBytes + fileSize2);
            sut.File.AppendAllText(path, fileContent2, encoding);

            drive.AvailableFreeSpace.Should().Be(expectedRemainingBytes);
        }
    }
}