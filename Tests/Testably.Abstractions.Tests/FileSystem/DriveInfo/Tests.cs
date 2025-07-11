namespace Testably.Abstractions.Tests.FileSystem.DriveInfo;

[FileSystemTests]
public partial class Tests
{
	[Fact]
	public async Task ToString_ShouldReturnDriveName()
	{
		Skip.IfNot(Test.RunsOnWindows);

		IDriveInfo result =
			FileSystem.DriveInfo.New(FileTestHelper.RootDrive(Test));

		await That(result.ToString()).IsEqualTo("C:\\");
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
			void Act()
			{
				#pragma warning disable CA1416
				result.VolumeLabel = "TEST";
				#pragma warning restore CA1416
			}

			if (Test.RunsOnWindows)
			{
				await That(Act).DoesNotThrow();
				await That(result.VolumeLabel).IsEqualTo("TEST");
			}
			else
			{
				await That(Act).Throws<PlatformNotSupportedException>().WithHResult(-2146233031);
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
