namespace Testably.Abstractions.Tests.FileSystem.DriveInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Tests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void ToString_ShouldReturnDriveName()
	{
		Skip.IfNot(Test.RunsOnWindows);

		IDriveInfo result =
			FileSystem.DriveInfo.New(FileTestHelper.RootDrive(Test));

		result.ToString().Should().Be("C:\\");
	}

	[SkippableFact]
	public void VolumeLabel_ShouldBeWritable_OnWindows()
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		IDriveInfo result =
			FileSystem.DriveInfo.New(FileTestHelper.RootDrive(Test));
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
				exception.Should().BeException<PlatformNotSupportedException>(hResult: -2146233031);
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
