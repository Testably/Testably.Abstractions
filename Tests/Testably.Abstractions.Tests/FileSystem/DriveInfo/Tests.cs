namespace Testably.Abstractions.Tests.FileSystem.DriveInfo;

[FileSystemTests]
public partial class Tests
{
	[Fact]
	public void ToString_ShouldReturnDriveName()
	{
		Skip.IfNot(Test.RunsOnWindows);

		IDriveInfo result =
			FileSystem.DriveInfo.New(FileTestHelper.RootDrive(Test));

		result.ToString().Should().Be("C:\\");
	}

	[Fact]
	public async Task VolumeLabel_ShouldBeWritable_OnWindows()
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
				await That(exception).IsNull();
				await That(result.VolumeLabel).IsEqualTo("TEST");
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
