namespace Testably.Abstractions.Tests.Internal;

public class DriveInfoWrapperTests
{
	[Fact]
	public async Task FromDriveInfo_Null_ShouldReturnNull()
	{
		RealFileSystem fileSystem = new();

		DriveInfoWrapper? result = DriveInfoWrapper
			.FromDriveInfo(null, fileSystem);

		await That(result).IsNull();
	}
}
