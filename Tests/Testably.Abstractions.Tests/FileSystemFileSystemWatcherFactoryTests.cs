using System.IO;

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
	public void New_ShouldInitializeWithDefaultValues()
	{
		IFileSystem.IFileSystemWatcher result =
			FileSystem.FileSystemWatcher.New();

		result.Path.Should().Be("");
#if NETFRAMEWORK
		result.Filter.Should().Be("*.*");
#else
		result.Filter.Should().Be("*");
#endif
		result.IncludeSubdirectories.Should().BeFalse();
		result.InternalBufferSize.Should().Be(8192);
		result.NotifyFilter.Should().Be(NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite);
		result.EnableRaisingEvents.Should().BeFalse();
	}
}