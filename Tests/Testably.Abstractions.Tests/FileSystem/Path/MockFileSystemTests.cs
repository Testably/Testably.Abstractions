namespace Testably.Abstractions.Tests.FileSystem.Path;

public sealed class MockFileSystemTests : FileSystemPathTests<FileSystemMock>
{
	public MockFileSystemTests() : base(new FileSystemMock())
	{
	}
}