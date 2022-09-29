using System.IO;

namespace Testably.Abstractions.Tests;

public abstract class FileSystemDriveInfoTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    #region Test Setup

    public TFileSystem FileSystem { get; }
    public ITimeSystem TimeSystem { get; }
    public string BasePath { get; }

    protected FileSystemDriveInfoTests(
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
    [Trait(nameof(FileSystem), nameof(DriveInfo))]
    public void New_Null_ShouldThrowArgumentNullException()
    {
        Exception? exception = Record.Exception(() =>
        {
            _ = FileSystem.DriveInfo.New(null!);
        });

        exception.Should().BeOfType<ArgumentNullException>();
    }

    [Fact(Skip = "Not yet implemented")]
    [Trait(nameof(FileSystem), nameof(DriveInfo))]
    public void GetDrives_ShouldNotBeEmpty()
    {
        IFileSystem.IDriveInfo[] drives = FileSystem.DriveInfo.GetDrives();

        drives.Length.Should().BeGreaterThan(0);
    }
}