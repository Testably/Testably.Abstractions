using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Statistics.FileSystem;

public sealed class DriveInfoStatisticsTests
{
	[SkippableFact]
	public void AvailableFreeSpace_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").AvailableFreeSpace;

		sut.Statistics.DriveInfo["F:"].ShouldOnlyContainPropertyGetAccess(nameof(IDriveInfo.AvailableFreeSpace));
	}

	[SkippableFact]
	public void DriveFormat_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").DriveFormat;

		sut.Statistics.DriveInfo["F:"].ShouldOnlyContainPropertyGetAccess(nameof(IDriveInfo.DriveFormat));
	}

	[SkippableFact]
	public void DriveType_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").DriveType;

		sut.Statistics.DriveInfo["F:"].ShouldOnlyContainPropertyGetAccess(nameof(IDriveInfo.DriveType));
	}

	[SkippableFact]
	public void IsReady_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").IsReady;

		sut.Statistics.DriveInfo["F:"].ShouldOnlyContainPropertyGetAccess(nameof(IDriveInfo.IsReady));
	}

	[SkippableFact]
	public void Name_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").Name;

		sut.Statistics.DriveInfo["F:"].ShouldOnlyContainPropertyGetAccess(nameof(IDriveInfo.Name));
	}

	[SkippableFact]
	public void RootDirectory_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").RootDirectory;

		sut.Statistics.DriveInfo["F:"].ShouldOnlyContainPropertyGetAccess(nameof(IDriveInfo.RootDirectory));
	}

	[SkippableFact]
	public void TotalFreeSpace_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").TotalFreeSpace;

		sut.Statistics.DriveInfo["F:"].ShouldOnlyContainPropertyGetAccess(nameof(IDriveInfo.TotalFreeSpace));
	}

	[SkippableFact]
	public void TotalSize_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").TotalSize;

		sut.Statistics.DriveInfo["F:"].ShouldOnlyContainPropertyGetAccess(nameof(IDriveInfo.TotalSize));
	}

	[SkippableFact]
	public void VolumeLabel_Get_ShouldRegisterPropertyAccess()
	{
		MockFileSystem sut = new();

		_ = sut.DriveInfo.New("F:").VolumeLabel;

		sut.Statistics.DriveInfo["F:"].ShouldOnlyContainPropertyGetAccess(nameof(IDriveInfo.VolumeLabel));
	}

	[SkippableFact]
	public void VolumeLabel_Set_ShouldRegisterPropertyAccess()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem sut = new();
		string value = "F:";

		#pragma warning disable CA1416
		sut.DriveInfo.New("F:").VolumeLabel = value;
		#pragma warning restore CA1416

		sut.Statistics.DriveInfo["F:"].ShouldOnlyContainPropertySetAccess(nameof(IDriveInfo.VolumeLabel));
	}
}
