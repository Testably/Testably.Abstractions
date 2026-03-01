using Testably.Abstractions.Testing.Statistics;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public sealed class DriveInfoStatisticsTests
{
	[Test]
	public async Task Property_AvailableFreeSpace_Get_ShouldRegisterPropertyAccess()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").AvailableFreeSpace;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DriveInfo["F:"])
			.OnlyContainsPropertyGetAccess(nameof(IDriveInfo.AvailableFreeSpace));
	}

	[Test]
	public async Task Property_DriveFormat_Get_ShouldRegisterPropertyAccess()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").DriveFormat;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DriveInfo["F:"])
			.OnlyContainsPropertyGetAccess(nameof(IDriveInfo.DriveFormat));
	}

	[Test]
	public async Task Property_DriveType_Get_ShouldRegisterPropertyAccess()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").DriveType;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DriveInfo["F:"])
			.OnlyContainsPropertyGetAccess(nameof(IDriveInfo.DriveType));
	}

	[Test]
	public async Task Property_IsReady_Get_ShouldRegisterPropertyAccess()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").IsReady;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DriveInfo["F:"])
			.OnlyContainsPropertyGetAccess(nameof(IDriveInfo.IsReady));
	}

	[Test]
	public async Task Property_Name_Get_ShouldRegisterPropertyAccess()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").Name;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DriveInfo["F:"])
			.OnlyContainsPropertyGetAccess(nameof(IDriveInfo.Name));
	}

	[Test]
	public async Task Property_RootDirectory_Get_ShouldRegisterPropertyAccess()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").RootDirectory;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DriveInfo["F:"])
			.OnlyContainsPropertyGetAccess(nameof(IDriveInfo.RootDirectory));
	}

	[Test]
	public async Task Property_TotalFreeSpace_Get_ShouldRegisterPropertyAccess()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").TotalFreeSpace;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DriveInfo["F:"])
			.OnlyContainsPropertyGetAccess(nameof(IDriveInfo.TotalFreeSpace));
	}

	[Test]
	public async Task Property_TotalSize_Get_ShouldRegisterPropertyAccess()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").TotalSize;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DriveInfo["F:"])
			.OnlyContainsPropertyGetAccess(nameof(IDriveInfo.TotalSize));
	}

	[Test]
	public async Task Property_VolumeLabel_Get_ShouldRegisterPropertyAccess()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").VolumeLabel;

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DriveInfo["F:"])
			.OnlyContainsPropertyGetAccess(nameof(IDriveInfo.VolumeLabel));
	}

	[Test]
	public async Task Property_VolumeLabel_Set_ShouldRegisterPropertyAccess()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem sut = new();
		string value = "F:";

		#pragma warning disable CA1416
		sut.DriveInfo.New("F:").VolumeLabel = value;
		#pragma warning restore CA1416

		await That(sut.Statistics.TotalCount).IsEqualTo(2);
		await That(sut.Statistics.DriveInfo["F:"])
			.OnlyContainsPropertySetAccess(nameof(IDriveInfo.VolumeLabel));
	}

	[Test]
	public async Task ToString_ShouldBeDriveInfoWithPath()
	{
		IStatistics sut = new MockFileSystem().Statistics.DriveInfo[@"x:"];

		string? result = sut.ToString();

		await That(result).IsEqualTo(@"DriveInfo[x:]");
	}
}
