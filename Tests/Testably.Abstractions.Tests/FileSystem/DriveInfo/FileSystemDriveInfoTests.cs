namespace Testably.Abstractions.Tests.FileSystem.DriveInfo;

public abstract class FileSystemDriveInfoTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	public abstract string BasePath { get; }
	public TFileSystem FileSystem { get; }
	public ITimeSystem TimeSystem { get; }

	protected FileSystemDriveInfoTests(
		TFileSystem fileSystem,
		ITimeSystem timeSystem)
	{
		FileSystem = fileSystem;
		TimeSystem = timeSystem;

		Test.SkipIfTestsOnRealFileSystemShouldBeSkipped(FileSystem);
	}

	[SkippableFact]
	public void VolumeLabel_ShouldNotBeWritable()
	{
		IFileSystem.IDriveInfo result =
			FileSystem.DriveInfo.New(FileTestHelper.RootDrive());

		Exception? exception = Record.Exception(() =>
		{
#pragma warning disable CA1416
			result.VolumeLabel = "TEST";
#pragma warning restore CA1416
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<UnauthorizedAccessException>();
		}
		else
		{
			exception.Should().BeOfType<PlatformNotSupportedException>();
		}
	}
}