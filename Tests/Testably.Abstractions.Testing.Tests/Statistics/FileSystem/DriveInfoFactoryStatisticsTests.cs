using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public sealed class DriveInfoFactoryStatisticsTests
{
	[SkippableFact]
	public void GetDrives_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.DriveInfo.GetDrives();

		sut.Statistics.DriveInfo.ShouldOnlyContain(nameof(IDriveInfoFactory.GetDrives));
	}

	[SkippableFact]
	public void New_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string driveName = "X";

		sut.DriveInfo.New(driveName);

		sut.Statistics.DriveInfo.ShouldOnlyContain(nameof(IDriveInfoFactory.New),
			driveName);
	}

	[SkippableFact]
	public void Wrap_DriveInfo_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		DriveInfo driveInfo = DriveInfo.GetDrives().First();

		sut.DriveInfo.Wrap(driveInfo);

		sut.Statistics.DriveInfo.ShouldOnlyContain(nameof(IDriveInfoFactory.Wrap),
			driveInfo);
	}
}
