namespace Testably.Abstractions.Tests.FileSystem.FileSystemWatcher;

public abstract partial class FileSystemFileSystemWatcherTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	public abstract string BasePath { get; }
	public TFileSystem FileSystem { get; }
	public ITimeSystem TimeSystem { get; }

	protected FileSystemFileSystemWatcherTests(
		TFileSystem fileSystem,
		ITimeSystem timeSystem)
	{
		FileSystem = fileSystem;
		TimeSystem = timeSystem;

		Test.SkipIfTestsOnRealFileSystemShouldBeSkipped(FileSystem);
	}

	[SkippableTheory]
	[InlineData(-1, 4096)]
	[InlineData(4095, 4096)]
	[InlineData(4097, 4097)]
	public void InternalBufferSize_ShouldAtLeastHave4096Bytes(
		int bytes, int expectedBytes)
	{
		FileSystem.Initialize();
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New(BasePath);

		fileSystemWatcher.InternalBufferSize = bytes;

		fileSystemWatcher.InternalBufferSize.Should().Be(expectedBytes);
	}

	[SkippableTheory]
	[AutoData]
	public void Path_SetToNotExistingPath_ShouldThrowArgumentException(string path)
	{
		IFileSystem.IFileSystemWatcher fileSystemWatcher =
			FileSystem.FileSystemWatcher.New();

		Exception? exception = Record.Exception(() =>
		{
			fileSystemWatcher.Path = path;
		});

		exception.Should().BeOfType<ArgumentException>()
		   .Which.Message.Should().Contain(path);
	}
}