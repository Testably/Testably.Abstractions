namespace Testably.Abstractions.Tests.FileSystem.Path;

// ReSharper disable once UnusedMember.Global
public sealed class MockFileSystemTests : FileSystemPathTests<MockFileSystem>
{
	public MockFileSystemTests() : base(new MockFileSystem())
	{
	}
}