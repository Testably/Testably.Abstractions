﻿using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public sealed class DriveInfoStatisticsTests
{
	[SkippableFact]
	public void Property_AvailableFreeSpace_Get_ShouldRegisterPropertyAccess()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").AvailableFreeSpace;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DriveInfo["F:"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDriveInfo.AvailableFreeSpace));
	}

	[SkippableFact]
	public void Property_DriveFormat_Get_ShouldRegisterPropertyAccess()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").DriveFormat;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DriveInfo["F:"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDriveInfo.DriveFormat));
	}

	[SkippableFact]
	public void Property_DriveType_Get_ShouldRegisterPropertyAccess()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").DriveType;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DriveInfo["F:"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDriveInfo.DriveType));
	}

	[SkippableFact]
	public void Property_IsReady_Get_ShouldRegisterPropertyAccess()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").IsReady;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DriveInfo["F:"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDriveInfo.IsReady));
	}

	[SkippableFact]
	public void Property_Name_Get_ShouldRegisterPropertyAccess()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").Name;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DriveInfo["F:"].ShouldOnlyContainPropertyGetAccess(nameof(IDriveInfo.Name));
	}

	[SkippableFact]
	public void Property_RootDirectory_Get_ShouldRegisterPropertyAccess()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").RootDirectory;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DriveInfo["F:"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDriveInfo.RootDirectory));
	}

	[SkippableFact]
	public void Property_TotalFreeSpace_Get_ShouldRegisterPropertyAccess()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").TotalFreeSpace;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DriveInfo["F:"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDriveInfo.TotalFreeSpace));
	}

	[SkippableFact]
	public void Property_TotalSize_Get_ShouldRegisterPropertyAccess()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").TotalSize;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DriveInfo["F:"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDriveInfo.TotalSize));
	}

	[SkippableFact]
	public void Property_VolumeLabel_Get_ShouldRegisterPropertyAccess()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").VolumeLabel;

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DriveInfo["F:"]
			.ShouldOnlyContainPropertyGetAccess(nameof(IDriveInfo.VolumeLabel));
	}

	[SkippableFact]
	public void Property_VolumeLabel_Set_ShouldRegisterPropertyAccess()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem sut = new();
		string value = "F:";

		#pragma warning disable CA1416
		sut.DriveInfo.New("F:").VolumeLabel = value;
		#pragma warning restore CA1416

		sut.Statistics.TotalCount.Should().Be(2);
		sut.Statistics.DriveInfo["F:"]
			.ShouldOnlyContainPropertySetAccess(nameof(IDriveInfo.VolumeLabel));
	}

	[SkippableFact]
	public void ToString_ShouldBeDriveInfoWithPath()
	{
		IStatistics sut = new MockFileSystem().Statistics.DriveInfo[@"x:"];

		string? result = sut.ToString();

		result.Should().Be(@"DriveInfo[x:]");
	}
}
