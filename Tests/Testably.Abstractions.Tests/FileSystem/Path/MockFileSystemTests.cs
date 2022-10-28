namespace Testably.Abstractions.Tests.FileSystem.Path;

public sealed class MockFileSystemTests : FileSystemPathTests<MockFileSystem>
{
	public MockFileSystemTests() : base(new MockFileSystem())
	{
	}
}