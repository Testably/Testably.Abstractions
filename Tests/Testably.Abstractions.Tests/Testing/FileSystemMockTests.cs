using System.Linq;

namespace Testably.Abstractions.Tests.Testing;

public class FileSystemMockTests
{
    [Fact]
    [Trait(nameof(Testing), nameof(FileSystemMock))]
    public void FileSystemMock_ShouldBeInitializedWithASingleDefaultDrive()
    {
        string expectedDriveName = "".PrefixRoot();
        FileSystemMock sut = new();

        IFileSystem.IDriveInfo[] drives = sut.DriveInfo.GetDrives();

        drives.Length.Should().Be(1);
        drives.Should().ContainSingle(d => d.Name == expectedDriveName);
    }

    [Theory]
    [InlineData("D:\\")]
    [Trait(nameof(Testing), nameof(FileSystemMock))]
    public void WithDrive_NewName_ShouldCreateNewDrives(string driveName)
    {
        FileSystemMock sut = new();
        sut.WithDrive(driveName);

        IFileSystem.IDriveInfo[] drives = sut.DriveInfo.GetDrives();

        drives.Length.Should().Be(2);
        drives.Should().ContainSingle(d => d.Name == driveName);
    }

    [Fact]
    [Trait(nameof(Testing), nameof(FileSystemMock))]
    public void WithDrive_ExistingName_ShouldUpdateDrive()
    {
        string driveName = "".PrefixRoot();
        FileSystemMock sut = new();
        sut.WithDrive(driveName);

        IFileSystem.IDriveInfo[] drives = sut.DriveInfo.GetDrives();

        drives.Length.Should().Be(1);
        drives.Should().ContainSingle(d => d.Name == driveName);
    }

    [Theory]
    [AutoData]
    [Trait(nameof(Testing), nameof(FileSystemMock))]
    public void WithDrive_WithCallback_ShouldUpdateDrive(long totalSize)
    {
        FileSystemMock sut = new();
        sut.WithDrive(d => d.SetTotalSize(totalSize));

        IFileSystem.IDriveInfo drive = sut.DriveInfo.GetDrives().Single();

        drive.TotalSize.Should().Be(totalSize);
        drive.TotalFreeSpace.Should().Be(totalSize);
        drive.AvailableFreeSpace.Should().Be(totalSize);
    }
}