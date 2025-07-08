namespace Testably.Abstractions.Tests.Internal;

public class FileSystemWatcherWrapperTests
{
	[Fact]
	public async Task FromFileSystemWatcher_Null_ShouldReturnNull()
	{
		RealFileSystem fileSystem = new();

		using FileSystemWatcherWrapper? result = FileSystemWatcherWrapper
			.FromFileSystemWatcher(null, fileSystem);

		await That(result).IsNull();
	}
}
