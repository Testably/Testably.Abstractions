using Testably.Abstractions.Tests.TestHelpers.Traits;

namespace Testably.Abstractions.Tests.FileSystem;

// ReSharper disable once UnusedMember.Global
[SystemTest(nameof(DriveInfoFactory.MockFileSystemTests))]
public sealed class MockFileSystemTests : FileSystemTests<FileSystemMock>
{
	public MockFileSystemTests() : base(new FileSystemMock())
	{
	}
}