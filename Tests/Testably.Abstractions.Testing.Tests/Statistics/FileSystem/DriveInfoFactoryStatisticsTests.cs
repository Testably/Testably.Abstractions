﻿using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public sealed class DriveInfoFactoryStatisticsTests
{
	[Fact]
	public void Method_GetDrives_ShouldRegisterCall()
	{
		MockFileSystem sut = new();

		sut.DriveInfo.GetDrives();

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.DriveInfo.ShouldOnlyContainMethodCall(nameof(IDriveInfoFactory.GetDrives));
	}

	[Fact]
	public void Method_New_String_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		string driveName = "X";

		sut.DriveInfo.New(driveName);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.DriveInfo.ShouldOnlyContainMethodCall(nameof(IDriveInfoFactory.New),
			driveName);
	}

	[Fact]
	public void Method_Wrap_DriveInfo_ShouldRegisterCall()
	{
		MockFileSystem sut = new();
		DriveInfo driveInfo = DriveInfo.GetDrives()[0];

		sut.DriveInfo.Wrap(driveInfo);

		sut.Statistics.TotalCount.Should().Be(1);
		sut.Statistics.DriveInfo.ShouldOnlyContainMethodCall(nameof(IDriveInfoFactory.Wrap),
			driveInfo);
	}

	[Fact]
	public void ToString_ShouldBeDriveInfo()
	{
		IPathStatistics<IDriveInfoFactory, IDriveInfo> sut
			= new MockFileSystem().Statistics.DriveInfo;

		string? result = sut.ToString();

		result.Should().Be("DriveInfo");
	}
}
