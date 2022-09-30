using System.IO;

namespace Testably.Abstractions.Tests;

public abstract class FileSystemDriveInfoFactoryTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    #region Test Setup

    public string BasePath { get; }

    public TFileSystem FileSystem { get; }
    public ITimeSystem TimeSystem { get; }

    protected FileSystemDriveInfoFactoryTests(
        TFileSystem fileSystem,
        ITimeSystem timeSystem,
        string basePath)
    {
        FileSystem = fileSystem;
        TimeSystem = timeSystem;
        BasePath = basePath;
    }

    #endregion

    [Fact]
    [FileSystemTests.DriveInfoFactory(nameof(IFileSystem.IDriveInfoFactory.GetDrives))]
    public void GetDrives_ShouldNotBeEmpty()
    {
        IFileSystem.IDriveInfo[] result = FileSystem.DriveInfo.GetDrives();

        result.Should().NotBeEmpty();
    }

    [Theory]
    [AutoData]
    [FileSystemTests.DriveInfoFactory(nameof(IFileSystem.IDriveInfoFactory.New))]
    public void New_InvalidDriveName_ShouldThrowArgumentNullException(
        string invalidDriveName)
    {
        Exception? exception = Record.Exception(() =>
        {
            _ = FileSystem.DriveInfo.New(invalidDriveName);
        });

        exception.Should().BeOfType<ArgumentException>();
    }

    [Fact]
    [FileSystemTests.DriveInfoFactory(nameof(IFileSystem.IDriveInfoFactory.New))]
    public void New_Null_ShouldThrowArgumentNullException()
    {
        Exception? exception = Record.Exception(() =>
        {
            _ = FileSystem.DriveInfo.New(null!);
        });

        exception.Should().BeOfType<ArgumentNullException>();
    }

    [Theory]
    [InlineData('A')]
    [InlineData('C')]
    [InlineData('X')]
    [FileSystemTests.DriveInfoFactory(nameof(IFileSystem.IDriveInfoFactory.New))]
    public void New_WithDriveLetter_ShouldReturnDriveInfo(char driveLetter)
    {
        IFileSystem.IDriveInfo result = FileSystem.DriveInfo.New($"{driveLetter}");

        result.Name.Should().Be($"{driveLetter}:\\");
    }

    [Theory]
    [InlineAutoData('A')]
    [InlineAutoData('C')]
    [InlineAutoData('Y')]
    [FileSystemTests.DriveInfoFactory(nameof(IFileSystem.IDriveInfoFactory.New))]
    public void New_WithRootedPath_ShouldReturnDriveInfo(char driveLetter, string path)
    {
        string rootedPath = path.PrefixRoot(driveLetter);

        IFileSystem.IDriveInfo result = FileSystem.DriveInfo.New(rootedPath);

        result.Name.Should().Be($"{driveLetter}:\\");
    }

    [Fact]
    [FileSystemTests.DriveInfoFactory(nameof(IFileSystem.IDriveInfoFactory.GetDrives))]
    public void Wrap_ShouldReturnDriveInfoWithSameName()
    {
        DriveInfo driveInfo = new("C");
        IFileSystem.IDriveInfo result = FileSystem.DriveInfo.Wrap(driveInfo);

        result.Name.Should().Be(driveInfo.Name);
    }
}