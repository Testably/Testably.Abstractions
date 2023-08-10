using DriveInfoWrapper = Testably.Abstractions.FileSystem.DriveInfoWrapper;

namespace Testably.Abstractions.Tests.Internal;

public class DriveInfoWrapperTests
{
	[Fact]
	public void FromDriveInfo_Null_ShouldReturnNull()
	{
		RealFileSystem fileSystem = new();

		DriveInfoWrapper? result = DriveInfoWrapper
			.FromDriveInfo(null, fileSystem);

		result.Should().BeNull();
	}
}
