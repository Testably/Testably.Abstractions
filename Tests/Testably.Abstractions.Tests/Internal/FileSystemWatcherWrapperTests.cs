namespace Testably.Abstractions.Tests.Internal;

public class FileSystemWatcherWrapperTests
{
	[Fact]
	public void FromFileSystemWatcher_Null_ShouldReturnNull()
	{
		RealFileSystem fileSystem = new();

		using FileSystemWatcherWrapper? result = FileSystemWatcherWrapper
			.FromFileSystemWatcher(null, fileSystem);

		result.Should().BeNull();
	}
}
