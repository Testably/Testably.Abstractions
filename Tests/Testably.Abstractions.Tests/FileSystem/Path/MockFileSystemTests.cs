using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.FileSystem.Path;

[SystemTest(nameof(DriveInfoFactory.MockFileSystemTests))]
public sealed class MockFileSystemTests : FileSystemPathTests<FileSystemMock>
{
	public MockFileSystemTests() : base(new FileSystemMock())
	{
	}
}