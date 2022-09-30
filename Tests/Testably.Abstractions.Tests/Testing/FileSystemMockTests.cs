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
        IFileSystem.IDriveInfo drive = drives.Single();

        drive.Name.Should().Be(expectedDriveName);
        drive.AvailableFreeSpace.Should().BeGreaterThan(0);
        drive.DriveFormat.Should().Be(FileSystemMock.DriveInfoMock.DefaultDriveFormat);
        drive.DriveType.Should().Be(FileSystemMock.DriveInfoMock.DefaultDriveType);
        drive.VolumeLabel.Should().NotBeNullOrEmpty();
    }

    [SkippableTheory]
    [InlineData("D:\\")]
    [Trait(nameof(Testing), nameof(FileSystemMock))]
    public void WithDrive_NewName_ShouldCreateNewDrives(string driveName)
    {
        Skip.IfNot(Test.RunsOnWindows, "Linux does not support different drives.");
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

    [Theory]
    [AutoData]
    [Trait(nameof(Testing), nameof(FileSystemMock))]
    public void WithDrive_WithCallback_ShouldUp22dateDrive(long totalSize)
    {
        FileSystemMock sut = new();
        sut.WithDrive(d => d.SetTotalSize(totalSize));

        IFileSystem.IDriveInfo drive = sut.DriveInfo.GetDrives().Single();

        drive.TotalSize.Should().Be(totalSize);
        drive.TotalFreeSpace.Should().Be(totalSize);
        drive.AvailableFreeSpace.Should().Be(totalSize);
    }
}