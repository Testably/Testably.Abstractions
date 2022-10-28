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
	public void VolumeLabel_ShouldBeWritableOnlyOnWindows()
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		IDriveInfo result =
			FileSystem.DriveInfo.New(FileTestHelper.RootDrive());
		string previousVolumeLabel = result.VolumeLabel;

		try
		{
			Exception? exception = Record.Exception(() =>
			{
#pragma warning disable CA1416
				result.VolumeLabel = "TEST";
#pragma warning restore CA1416
			});

			if (Test.RunsOnWindows)
			{
				exception.Should().BeNull();
				result.VolumeLabel.Should().Be("TEST");
			}
			else
			{
				exception.Should().BeOfType<PlatformNotSupportedException>();
			}
		}
		finally
		{
			if (Test.RunsOnWindows)
			{
#pragma warning disable CA1416
				result.VolumeLabel = previousVolumeLabel;
#pragma warning restore CA1416
			}
		}
	}
}