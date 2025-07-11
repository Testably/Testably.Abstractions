using System.IO;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public sealed class DriveInfoFactoryStatisticsTests
{
	[Fact]
	public async Task Method_GetDrives_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.DriveInfo.GetDrives();

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.DriveInfo)
			.OnlyContainsMethodCall(nameof(IDriveInfoFactory.GetDrives));
	}

	[Fact]
	public async Task Method_New_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string driveName = "X";

		sut.DriveInfo.New(driveName);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.DriveInfo).OnlyContainsMethodCall(nameof(IDriveInfoFactory.New),
			driveName);
	}

	[Fact]
	public async Task Method_Wrap_DriveInfo_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		DriveInfo driveInfo = DriveInfo.GetDrives()[0];

		sut.DriveInfo.Wrap(driveInfo);

		await That(sut.Statistics.TotalCount).IsEqualTo(1);
		await That(sut.Statistics.DriveInfo).OnlyContainsMethodCall(nameof(IDriveInfoFactory.Wrap),
			driveInfo);
	}

	[Fact]
	public async Task ToString_ShouldBeDriveInfo()
	{
		IPathStatistics<IDriveInfoFactory, IDriveInfo> sut
			= new MockFileSystem().Statistics.DriveInfo;

		string? result = sut.ToString();

		await That(result).IsEqualTo("DriveInfo");
	}
}
