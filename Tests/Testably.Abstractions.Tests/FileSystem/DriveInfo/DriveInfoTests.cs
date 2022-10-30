using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.DriveInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class DriveInfoTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
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