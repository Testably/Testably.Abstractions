using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.DriveInfo;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Tests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void VolumeLabel_ShouldBeWritable_OnWindows()
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
				exception.Should().BeOfType<PlatformNotSupportedException>()
					.Which.HResult.Should().Be(-2146233031);
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

	[SkippableFact]
	public void ToString_ShouldReturnDriveName()
	{
		Skip.IfNot(Test.RunsOnWindows);

		IDriveInfo result =
			FileSystem.DriveInfo.New(FileTestHelper.RootDrive());

		result.ToString().Should().Be("C:\\");
	}
}