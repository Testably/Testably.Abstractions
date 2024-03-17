using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public sealed class DriveInfoFactoryStatisticsTests
{
	[SkippableFact]
	public void Method_GetDrives_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.DriveInfo.GetDrives();

		sut.Statistics.DriveInfo.ShouldOnlyContainMethodCall(nameof(IDriveInfoFactory.GetDrives));
	}

	[SkippableFact]
	public void Method_New_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string driveName = "X";

		sut.DriveInfo.New(driveName);

		sut.Statistics.DriveInfo.ShouldOnlyContainMethodCall(nameof(IDriveInfoFactory.New),
			driveName);
	}

	[SkippableFact]
	public void Method_Wrap_DriveInfo_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		DriveInfo driveInfo = DriveInfo.GetDrives().First();

		sut.DriveInfo.Wrap(driveInfo);

		sut.Statistics.DriveInfo.ShouldOnlyContainMethodCall(nameof(IDriveInfoFactory.Wrap),
			driveInfo);
	}

	[SkippableFact]
	public void ToString_ShouldBeDriveInfo()
	{
		IPathStatistics sut = new MockFileSystem().Statistics.DriveInfo;

		string? result = sut.ToString();

		result.Should().Be("DriveInfo");
	}
}
