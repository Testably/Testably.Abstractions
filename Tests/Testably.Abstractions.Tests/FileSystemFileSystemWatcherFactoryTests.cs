namespace Testably.Abstractions.Tests;

public abstract class FileSystemFileSystemWatcherFactoryTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	public abstract string BasePath { get; }
	public TFileSystem FileSystem { get; }
	public ITimeSystem TimeSystem { get; }

	protected FileSystemFileSystemWatcherFactoryTests(
		TFileSystem fileSystem,
		ITimeSystem timeSystem)
	{
		FileSystem = fileSystem;
		TimeSystem = timeSystem;

		Test.SkipIfTestsOnRealFileSystemShouldBeSkipped(FileSystem);
	}

	[SkippableFact]
	[FileSystemTests.FileSystemWatcherFactory(nameof(IFileSystem.IFileSystemWatcherFactory.New))]
	public void New_ShouldUseEmptyPath()
	{
		IFileSystem.IFileSystemWatcher result =
			FileSystem.FileSystemWatcher.New();

		result.Path.Should().Be("");
		result.EnableRaisingEvents.Should().BeFalse();
	}
}